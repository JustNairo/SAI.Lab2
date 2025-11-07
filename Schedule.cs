namespace Lab2
{
    internal class Schedule
    {
        public List<Task> TaskOrder { get; set; } // Порядок выполнения задач
        public double Fitness { get; set; }

        public Schedule Parent1 { get; set; }
        public Schedule Parent2 { get; set; }

        public Schedule(List<Task> tasks)
        {
            TaskOrder = new List<Task>();
            foreach (Task task in tasks)
            {
                TaskOrder.Add(task.Copy());
            }
            Fitness = 0;
            CalculateTasksStartTime();
        }

        public override string ToString()
        {
            return string.Join(" → ", TaskOrder.Select(t => t.Name)) + $" (Fitness: {Fitness:F2})";
        }

        public void CalculateTasksStartTime()
        {
            TimeOnly time = new TimeOnly(0, 0);
            TaskOrder[0].StartTime = time;
            for (int i = 1; i < TaskOrder.Count; i++)
            {
                TaskOrder[i].StartTime = TaskOrder[i - 1].StartTime.Add(TimeSpan.FromHours(TaskOrder[i - 1].DurationHours));
            }
        }
    }
}
