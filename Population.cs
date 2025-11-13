using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
