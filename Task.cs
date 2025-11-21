namespace Lab2
{
    public class Task
    {
        public string Name { get; set; }
        public int Priority { get; set; } // Приоритет (1-высокий, 2-средний, 3-низкий)

        public double DurationHours { get; set; }
        public TimeOnly StartTime { get; set; }

        public Task(string name, double duration, int priority = 2)
        {
            Name = name;
            DurationHours = duration;
            Priority = priority;
        }

        public Task(Task task)
        {
            Name = task.Name;
            Priority = task.Priority;
            DurationHours = task.DurationHours;
        }

        public override string ToString() => $"{Name} ({DurationHours}ч)";

        public override bool Equals(object? obj)
        {
            var item = obj as Task;
            if (item == null)
                return false;

            return item.Name == Name;
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        /*
        public Task Copy()
        {
            return new Task(Name, DurationHours, Priority);
        }
        */
    }
}
