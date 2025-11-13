using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab2
{
    

    internal class ScheduleGeneticAlgorithm
    {
        private readonly Random random;
        private readonly int populationSize;
        private readonly double mutationRate;
        private readonly double crossoverRate;
        private readonly int maxWorkHours;

        private List<Task> availableTasks;
        private List<Task> fixedTasks;

        private AlgorithmInfo infoContainer;

        public ScheduleGeneticAlgorithm(AlgorithmInfo infoContainer,
                                        List<Task> tasksToSchedule, List<Task> fixedTasks,
                                        int populationSize = 50,
                                        double mutationRate = 0.05, double crossoverRate = 0.8,
                                        int maxWorkHours = 8)
        {
            random = new Random();
            this.populationSize = populationSize;
            this.mutationRate = mutationRate;
            this.crossoverRate = crossoverRate;
            this.maxWorkHours = maxWorkHours;

            this.availableTasks = tasksToSchedule;
            this.fixedTasks = fixedTasks;

            this.infoContainer = infoContainer;
        }

        // 1. Создание начальной популяции
        private List<Schedule> CreateInitialPopulation()
        {
            List<Schedule> population = new List<Schedule>();

            for (int i = 0; i < populationSize; i++)
            {
                List<Task> shuffledTasks = availableTasks.OrderBy(x => random.Next()).ToList();
                population.Add(new Schedule(shuffledTasks));
            }

            return population;
        }

        // 2. Фитнес-функция
        private void CalculateFitness(List<Schedule> population)
        {
            foreach (var schedule in population)
            {
                double fitness = 0;
                double totalTime = 0;
                int scheduledTasks = 0;

                // Критерий - приоритет. Чем выше приоритет, тем больше бонус
                foreach (var task in schedule.TaskOrder)
                {
                    if (totalTime + task.DurationHours <= maxWorkHours)
                    {
                        scheduledTasks++;
                        totalTime += task.DurationHours;

                        fitness += (4 - task.Priority) * 0.1;
                    }
                    else
                        break;
                }

                // Критерий - количество задач
                fitness += scheduledTasks * 2;

                // Критерий - занятое время. Чем больше времени занято, тем лучше
                double timeEfficiency = 1.0 - Math.Abs(totalTime - maxWorkHours) / maxWorkHours;
                fitness += timeEfficiency;

                // Штраф - у фиксированной задачи не то время
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
        private Schedule TournamentSelection(List<Schedule> population, int tournamentSize = 3)
        {
            List<Schedule> tournament = population.OrderBy(x => random.Next()).Take(tournamentSize).ToList();
            return tournament.OrderByDescending(s => s.Fitness).First();
        }

        // 4. Скрещивание для расписаний
        private (Schedule, Schedule) OrderCrossover(Schedule parent1, Schedule parent2)
        {
            Task[] child1Tasks = new Task[parent1.TaskOrder.Count];
            Task[] child2Tasks = new Task[parent2.TaskOrder.Count];

            if (random.NextDouble() > crossoverRate)
            {
                return (new Schedule(new List<Task>(parent1.TaskOrder)),
                        new Schedule(new List<Task>(parent2.TaskOrder)));
            }

            // Выбираем случайный сегмент
            int start = random.Next(0, parent1.TaskOrder.Count);
            int end = random.Next(start, parent1.TaskOrder.Count);

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

            Schedule child1Schedule = new Schedule(child1Tasks.ToList());
            Schedule child2Schedule = new Schedule(child1Tasks.ToList());

            return (child1Schedule, child2Schedule);
        }

        // 5. Мутация 
        private void SwapMutate(Schedule schedule)
        {
            if (random.NextDouble() > mutationRate)
                return;

            int index1 = random.Next(schedule.TaskOrder.Count);
            int index2 = random.Next(schedule.TaskOrder.Count);

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

            Schedule bestSchedule = population.OrderByDescending(s => s.Fitness).First();
            infoContainer.InitialBestSchedule = new Schedule(bestSchedule.TaskOrder)
            {
                Fitness = bestSchedule.Fitness,
            };

            for (int generation = 1; generation <= maxGenerations; generation++)
            {
                List<Schedule> newPopulation = new List<Schedule>()
                { 
                    new Schedule(new List<Task>(bestSchedule.TaskOrder)),
                };

                // Создаем новую популяцию
                while (newPopulation.Count < populationSize)
                {
                    var parent1 = TournamentSelection(population);
                    var parent2 = TournamentSelection(population);

                    var (child1, child2) = OrderCrossover(parent1, parent2);

                    SwapMutate(child1);
                    SwapMutate(child2);

                    newPopulation.Add(child1);
                    if (newPopulation.Count < populationSize)
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
                    infoContainer.PopulationFitness[generation] = (bestSchedule.Fitness, avgFitness);
                }
            }

            GetInfo(bestSchedule, infoContainer);

            return bestSchedule;
        }

        private void GetInfo(Schedule schedule, AlgorithmInfo infoContainer)
        {
            double totalTime = 0;
            int taskNumber = 1;

            foreach (var task in schedule.TaskOrder)
            {
                if (totalTime + task.DurationHours <= maxWorkHours)
                {
                    totalTime += task.DurationHours;
                    taskNumber++;
                }
                else
                    break;
            }

            infoContainer.CompletedTasksCount = taskNumber - 1;
            infoContainer.UsedTime = totalTime;
            infoContainer.Efficency = totalTime / maxWorkHours * 100;
        }
    }
}
