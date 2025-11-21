namespace Lab2
{
    /// <summary>
    /// Клас для сбора и вывода информации о результатах алгоритма
    /// </summary>
    public class AlgorithmInfo
    {
        public AlgorithmInfo()
        {
            GenerationFitness = new Dictionary<int, (double, double)>();
        }

        public string Name { get; set; }
        public int TaskCount { get; set; } 
        public double TotalTime { get; set; } 
        public double MaxWorkingHours { get; set; }

        public Schedule InitialBestSchedule { get; set; }
        public Dictionary<int, (double, double)> GenerationFitness {  get; set; }

        public string GenerationFitnessString
        {
            get
            {
                string result = "";
                foreach(var pair in GenerationFitness)
                {
                    result += "Поколение " + pair.Key.ToString() + ": "
                    + "Лучшая приспособленность = " + Math.Round(pair.Value.Item1, 2) + ", "
                    + "Средняя = " + Math.Round(pair.Value.Item2, 2) + "\n";
                }
                return result;
            }
        }

        public int CompletedTasksCount { get; set; }
        public double UsedTime { get; set; }
        public double Efficency { get; set; }
    }
}
