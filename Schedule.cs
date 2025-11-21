namespace Lab2
{
    public class Schedule
    {
        public List<Task> TaskOrder { get; set; } // Список задач в пордяке их выполнения
        public double Fitness { get; set; } // Приспособленность расписания

        public Schedule(List<Task> tasks)
        {
            TaskOrder = new List<Task>();
            foreach (Task task in tasks) 
                TaskOrder.Add(new Task(task));
            Fitness = 0;
            CalculateTasksStartTime();
        }

        public override string ToString()
        {
            return string.Join(" → ", TaskOrder.Select(t => t.Name)) + $" (Fitness: {Fitness:F2})";
        }

        // Метод для определения стратового времени для каждой задачи в списке
        public void CalculateTasksStartTime()
        {
            TimeOnly time = new TimeOnly(0, 0);
            TaskOrder[0].StartTime = time;
            for (int i = 1; i < TaskOrder.Count; i++)
                TaskOrder[i].StartTime = TaskOrder[i - 1].StartTime.Add(TimeSpan.FromHours(TaskOrder[i - 1].DurationHours));
        }
    }
}
