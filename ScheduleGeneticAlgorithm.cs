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
                // Получаем задачи в случайном порядке
                List<Task> shuffledTasks = availableTasks.OrderBy(x => random.Next()).ToList();
                population.Add(new Schedule(shuffledTasks));
            }

            return population;
        }

        // 2. Фитнес-функция
        private void CalculateFitness(List<Schedule> population)
        {
            // Определяем приспособленность для каждой популяции
            foreach (var schedule in population)
            {
                double fitness = 0;
                double totalTime = 0; // Время задач, которые можно было выполнить
                int scheduledTasks = 0; // Колчиество задач, которые могут быть выполнены

                // Определяем приспособленность по критериям
                // Критерий №1 - приоритет. Чем выше приоритет, тем больше бонус
                foreach (var task in schedule.TaskOrder)
                {
                    if (totalTime + task.DurationHours > maxWorkHours)
                        break;

                    scheduledTasks++;
                    totalTime += task.DurationHours;

                    fitness += (4 - task.Priority) * 0.1;
                }

                // Критерий №2 - количество задач
                fitness += scheduledTasks * 2;

                // Критерий №3 - занятое время. Чем больше времени занято, тем лучше
                double timeEfficiency = 1.0 - Math.Abs(totalTime - maxWorkHours) / maxWorkHours;
                fitness += timeEfficiency;

                // Штраф - фиксированная задача находится не там в расписании
                foreach (var task in fixedTasks)
                {
                    // Находим индекс задачи в расписании
                    int index = schedule.TaskOrder.FindIndex(t => t.Equals(task));
                    
                    // Если время начала задачи, которая должна быть в определённом месте,
                    // не совпадает с заданным, то не гуд, не гуд  
                    if (!schedule.TaskOrder[index].StartTime.Equals(task.StartTime))
                        fitness = 0;
                }

                schedule.Fitness = fitness;
            }
        }

        // 3. Турнирный отбор
        private Schedule TournamentSelection(List<Schedule> population, int tournamentSize = 3)
        {
            // Выбираем случайный список расписаний размером tournamentSize
            List<Schedule> tournament = population.OrderBy(x => random.Next())
                .Take(tournamentSize).ToList();
            // Возвращаем самое приспособленное
            return tournament.OrderByDescending(s => s.Fitness).First();
        }

        // 4. Скрещивание для расписаний
        private (Schedule, Schedule) OrderCrossover(Schedule parent1, Schedule parent2)
        {
            // Если не судьба, то не проводим скрещивание
            if (random.NextDouble() > crossoverRate)
            {
                return (new Schedule(new List<Task>(parent1.TaskOrder)),
                        new Schedule(new List<Task>(parent2.TaskOrder)));
            }

            // Определения массивов задач для child1 и child2
            Task[] child1Tasks = new Task[parent1.TaskOrder.Count];
            Task[] child2Tasks = new Task[parent2.TaskOrder.Count];

            // Выбираем случайный сегмент
            // Этот сегмент останется у ребенка от одного родителя, остальные задачи будут от другого
            int start = random.Next(0, parent1.TaskOrder.Count);
            int end = random.Next(start, parent1.TaskOrder.Count);

            // Список задач для child1. Эти задачи взяты из parent2 при этом остуствуют в сегменте parent1, определённом выше выше
            List<Task> child1Remaining = parent2.TaskOrder.Where(t => !parent1.TaskOrder.GetRange(start, end - start + 1).Contains(t)).ToList();
            List<Task> child2Remaining = parent1.TaskOrder.Where(t => !parent2.TaskOrder.GetRange(start, end - start + 1).Contains(t)).ToList();

            int child1Index = 0, child2Index = 0;

            // Заполняем список задач для детей
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
            // Если не судьба, то без мутации
            if (random.NextDouble() > mutationRate)
                return;

            // Индексы случайных задач
            int index1 = random.Next(schedule.TaskOrder.Count);
            int index2 = random.Next(schedule.TaskOrder.Count);

            // Меняем местами две случайные задачи
            var temp = schedule.TaskOrder[index1];
            schedule.TaskOrder[index1] = schedule.TaskOrder[index2];
            schedule.TaskOrder[index2] = temp;

            //Определем стартовое время для нового списка задач
            schedule.CalculateTasksStartTime();
        }

        // Основной метод
        public Schedule Run(int maxGenerations = 100)
        {
            List<Schedule> population = CreateInitialPopulation();
            CalculateFitness(population);
            Schedule bestSchedule = population.OrderByDescending(s => s.Fitness).First();

            // Сохраняем информацию об алгоритме
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

                // Определемя лучшее расписание
                var currentBest = population.OrderByDescending(s => s.Fitness).First();
                if (currentBest.Fitness > bestSchedule.Fitness)
                    bestSchedule = currentBest;

                // Сохраняем информацию у каждого 20 поколения
                if (generation % 20 == 0)
                {
                    double avgFitness = population.Average(s => s.Fitness);
                    infoContainer.GenerationFitness[generation] = (bestSchedule.Fitness, avgFitness);
                }
            }

            // Заполняем оставшуюся информацию
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
