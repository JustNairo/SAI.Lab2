using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab2
{
    class Population
    {
        private Random random;

        public int PopulationSize { get; }
        public List<Schedule> Schedules { get; set; }

        public Population(int populationSize)
        {
            Schedules = new List<Schedule>();



            PopulationSize = populationSize;
            random = new Random();
        }

        public void CreateInitialPopulation(List<Task> availableTasks)
        {
            for (int i = 0; i < PopulationSize; i++)
            {
                var shuffledTasks = availableTasks.OrderBy(x => random.Next()).ToList();
                Schedules.Add(new Schedule(shuffledTasks));
            }
        }

        public void CalculateFitness(int maxWorkHours, List<Task> fixedTasks)
        {
            foreach (var schedule in Schedules)
            {
                double fitness = 0;
                double totalTime = 0;
                int scheduledTasks = 0;

                // Критерий 1: Количество выполненных задач в рабочий день
                foreach (var task in schedule.TaskOrder)
                {
                    if (totalTime + task.DurationHours <= maxWorkHours)
                    {
                        scheduledTasks++;
                        totalTime += task.DurationHours;

                        // Критерий 2: Приоритетные задачи должны выполняться раньше
                        double priorityBonus = (4 - task.Priority) * 0.1; // Высокий приоритет = больше бонус
                        fitness += priorityBonus;
                    }
                    else
                    {
                        break;
                    }
                }

                // Основной критерий - максимизировать количество выполненных задач
                fitness += scheduledTasks * 2;

                // Критерий 3: Минимизировать простои (эффективность использования времени)
                double timeEfficiency = 1.0 - Math.Abs(totalTime - maxWorkHours) / maxWorkHours;
                fitness += timeEfficiency;

                // Штраф: существующая задача не в том времени
                foreach (var task in fixedTasks)
                {
                    int index = schedule.TaskOrder.FindIndex(t => t.Name.Equals(task.Name)); // Имя даёт уникальную идентификацию
                    if (!schedule.TaskOrder[index].StartTime.Equals(task.StartTime))
                        fitness = 0;
                }

                schedule.Fitness = fitness;
            }
        }

        // 3. Турнирный отбор
        public Schedule TournamentSelection(int tournamentSize = 3)
        {
            var tournament = Schedules.OrderBy(x => random.Next()).Take(tournamentSize).ToList();
            return tournament.OrderByDescending(s => s.Fitness).First();
        }
    }

    internal class ScheduleGeneticAlgorithm
    {
        private readonly Random _random;
        private readonly int _populationSize;
        private readonly double _mutationRate;
        private readonly double _crossoverRate;
        private readonly int _maxWorkHours;

        public List<Task> AvailableTasks { get; private set; }
        public List<Task> FixedTasks { get; private set; }

        public ScheduleGeneticAlgorithm(ref string description,
                                      List<Task> tasksToSchedule, List<Task> fixedTasks,
                                      int populationSize = 50,
                                      double mutationRate = 0.05, double crossoverRate = 0.8,
                                      int maxWorkHours = 8)
        {
            _random = new Random();
            _populationSize = populationSize;
            _mutationRate = mutationRate;
            _crossoverRate = crossoverRate;
            _maxWorkHours = maxWorkHours;

            AvailableTasks = tasksToSchedule;
            FixedTasks = fixedTasks;
        }

        // 1. Создание начальной популяции (случайные перестановки)
        private List<Schedule> CreateInitialPopulation()
        {
            var population = new List<Schedule>();

            for (int i = 0; i < _populationSize; i++)
            {
                var shuffledTasks = AvailableTasks.OrderBy(x => _random.Next()).ToList();
                population.Add(new Schedule(shuffledTasks));
            }

            return population;
        }

        // 2. Фитнес-функция - САМАЯ ВАЖНАЯ ЧАСТЬ!
        private void CalculateFitness(List<Schedule> population)
        {
            foreach (var schedule in population)
            {
                double fitness = 0;
                double totalTime = 0;
                int scheduledTasks = 0;

                // Критерий 1: Количество выполненных задач в рабочий день
                foreach (var task in schedule.TaskOrder)
                {
                    if (totalTime + task.DurationHours <= _maxWorkHours)
                    {
                        scheduledTasks++;
                        totalTime += task.DurationHours;

                        // Критерий 2: Приоритетные задачи должны выполняться раньше
                        double priorityBonus = (4 - task.Priority) * 0.1; // Высокий приоритет = больше бонус
                        fitness += priorityBonus;
                    }
                    else
                    {
                        break;
                    }
                }

                // Основной критерий - максимизировать количество выполненных задач
                fitness += scheduledTasks * 2;

                // Критерий 3: Минимизировать простои (эффективность использования времени)
                double timeEfficiency = 1.0 - Math.Abs(totalTime - _maxWorkHours) / _maxWorkHours;
                fitness += timeEfficiency;

                // Штраф: существующая задача не в том времени
                foreach (var task in FixedTasks)
                {
                    int index = schedule.TaskOrder.FindIndex(t => t.Name.Equals(task.Name)); // Имя даёт уникальную идентификацию
                    if (!schedule.TaskOrder[index].StartTime.Equals(task.StartTime))
                        fitness = 0;
                }

                schedule.Fitness = fitness;
            }
        }

        // 3. Турнирный отбор
        private Schedule TournamentSelection(List<Schedule> population, int tournamentSize = 3)
        {
            var tournament = population.OrderBy(x => _random.Next()).Take(tournamentSize).ToList();
            return tournament.OrderByDescending(s => s.Fitness).First();
        }

        // 4. Скрещивание для расписаний (Order Crossover - OX)
        private (Schedule, Schedule) OrderCrossover(Schedule parent1, Schedule parent2)
        {
            var child1Tasks = new Task[parent1.TaskOrder.Count];
            var child2Tasks = new Task[parent2.TaskOrder.Count];

            if (_random.NextDouble() > _crossoverRate)
            {
                return (new Schedule(new List<Task>(parent1.TaskOrder)),
                        new Schedule(new List<Task>(parent2.TaskOrder)));
            }

            // Выбираем случайный сегмент
            int start = _random.Next(0, parent1.TaskOrder.Count);
            int end = _random.Next(start, parent1.TaskOrder.Count);

            // Для первого ребенка
            var troubleShootig1 = parent1.TaskOrder.GetRange(start, end - start + 1);
            var troubleShootig2 = parent2.TaskOrder.GetRange(start, end - start + 1);

            var child1Remaining = parent2.TaskOrder.Where(t => !parent1.TaskOrder.GetRange(start, end - start + 1).Contains(t)).ToList();
            var child2Remaining = parent1.TaskOrder.Where(t => !parent2.TaskOrder.GetRange(start, end - start + 1).Contains(t)).ToList();

            int child1Index = 0, child2Index = 0;

            for (int i = 0; i < parent1.TaskOrder.Count; i++)
            {
                if (i >= start && i <= end)
                {
                    child1Tasks[i] = parent1.TaskOrder[i];
                    child2Tasks[i] = parent2.TaskOrder[i];
                }
                else
                {
                    child1Tasks[i] = child1Remaining[child1Index++];
                    child2Tasks[i] = child2Remaining[child2Index++];
                }
            }

            Schedule child1Schedule = new Schedule(child1Tasks.ToList())
            {
                Parent1 = parent1,
                Parent2 = parent2,
            };
            Schedule child2Schedule = new Schedule(child1Tasks.ToList())
            {
                Parent1 = parent1,
                Parent2 = parent2,
            };

            if (child1Schedule.TaskOrder.Select(t => t.Name).Distinct().Count() != 9
                || child1Schedule.TaskOrder.Select(t => t.Name).Distinct().Count() != 9)
                throw new Exception("А какого фига!");

            return (child1Schedule, child2Schedule);
        }

        // 5. Мутация (swap mutation)
        private void SwapMutate(Schedule schedule)
        {
            if (_random.NextDouble() > _mutationRate)
                return;

            int index1 = _random.Next(schedule.TaskOrder.Count);
            int index2 = _random.Next(schedule.TaskOrder.Count);

            // Меняем местами две случайные задачи
            var temp = schedule.TaskOrder[index1];
            schedule.TaskOrder[index1] = schedule.TaskOrder[index2];
            schedule.TaskOrder[index2] = temp;

            schedule.CalculateTasksStartTime();
        }

        // Основной метод
        public Schedule Run(int maxGenerations = 100)
        {
            List<Schedule> population = CreateInitialPopulation();
            CalculateFitness(population);

            Population testPopulation = new Population(_populationSize);
            testPopulation.CreateInitialPopulation(AvailableTasks);
            testPopulation.CalculateFitness(_maxWorkHours, FixedTasks);

            Schedule bestSchedule = population.OrderByDescending(s => s.Fitness).First();
            // Console.WriteLine($"Начальное поколение: Лучшее расписание = {bestSchedule}");
            // description += $"Начальное поколение: Лучшее расписание = {bestSchedule} \n";

            Schedule testBestSchedule = testPopulation.Schedules.OrderByDescending(s => s.Fitness).First();

            for (int generation = 1; generation <= maxGenerations; generation++)
            {
                List<Schedule> newPopulation = new List<Schedule>()
                { 
                    new Schedule(new List<Task>(bestSchedule.TaskOrder)),
                };

                Population testNewPopulation = new Population(_populationSize);
                testNewPopulation.Schedules.Add(new Schedule(new List<Task>(bestSchedule.TaskOrder)));

                // Создаем новую популяцию
                while (newPopulation.Count < _populationSize)
                {
                    var parent1 = TournamentSelection(population);
                    var parent2 = TournamentSelection(population);

                    var (child1, child2) = OrderCrossover(parent1, parent2);

                    SwapMutate(child1);
                    SwapMutate(child2);

                    newPopulation.Add(child1);
                    if (newPopulation.Count < _populationSize)
                        newPopulation.Add(child2);
                }

                population = newPopulation;
                CalculateFitness(population);

                var currentBest = population.OrderByDescending(s => s.Fitness).First();
                if (currentBest.Fitness > bestSchedule.Fitness)
                    bestSchedule = currentBest;

                if (generation % 20 == 0)
                {
                    double avgFitness = population.Average(s => s.Fitness);
                    // description = $"Поколение {generation}: Лучшая приспособленность = {bestSchedule.Fitness:F2}, Средняя = {avgFitness:F2} \n";
                }
            }

            return bestSchedule;
        }
    }
}
