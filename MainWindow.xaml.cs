using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lab2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int PxPerHour = 80;
        const int maxWorkHours = 8;

        private List<Task> tasks;
        private List<Task> fixedTasks;

        public MainWindow()
        {
            InitializeComponent();

            tasks = new List<Task>
            {
                new Task("Важная встреча", 1.5, 1),
                new Task("Написание отчета", 3.0, 1),
                new Task("Проверка почты", 0.5, 2),
                new Task("Планирование", 1.0, 2),
                new Task("Обучение", 2.0, 3),
                new Task("Совещание", 1.0, 2),
                new Task("Анализ данных", 2.5, 1),
                new Task("Кофе-брейк", 0.25, 3),
                new Task("Работа с документами", 2.0, 2)
            };
            fixedTasks = new List<Task>
            {
                new Task("Важная встреча", 1.5, 1) {StartTime = new TimeOnly(2, 0)},
                new Task("Совещание", 1.0, 2) {StartTime = new TimeOnly(7, 0)},
            };

            ShowTasks(tasks, fixedTasks);
        }

        private Schedule RunGeneticAlgorithm(List<Task> tasks, List<Task> fixedTasks)
        {
            // Запускаем генетический алгоритм
            var ga = new ScheduleGeneticAlgorithm(
                tasksToSchedule: tasks,
                fixedTasks: fixedTasks,
                populationSize: 100,
                mutationRate: 0.08,
                crossoverRate: 0.85,
                maxWorkHours: maxWorkHours
            );

            var bestSchedule = ga.Run(maxGenerations: 200);

            return bestSchedule;
        }

        private void ShowTasks(List<Task> tasks, List<Task> fixedTasks)
        {
            Brush[] brushes = new Brush[]
            {
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ef9bfa")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff9dc4")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffb1a1")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fac593")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8da85")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#bbf28f")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6ef7c8")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5ee9f7")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9bd4fa")),
            };

            for (int i = 0; i < tasks.Count; i++)
            {
                Border newTask = new Border()
                {
                    Margin = new Thickness(0, 0, 0, 5),
                    Height = tasks[i].DurationHours * PxPerHour,
                    Background = brushes[i],
                    CornerRadius = new CornerRadius(3),
                };
                Label label = new Label() { Content = tasks[i].ToString() };

                newTask.Child = label;

                tasksPannel.Children.Add(newTask);
            }

        }

        private void ShowSchedule(Schedule schedule)
        {
            double totalTime = 0;

            for (int i = 0; i < schedule.TaskOrder.Count; i++)
            {
                Task task = schedule.TaskOrder[i];
                if (totalTime + task.DurationHours <= maxWorkHours)
                {
                    totalTime += task.DurationHours;

                    foreach (UIElement element in tasksPannel.Children)
                    {
                        var childBorder = (Border)element;
                        var childLabel = (Label)childBorder.Child;

                        string taskToString = task.ToString();
                        if (childLabel.Content.ToString() == taskToString)
                        {
                            element.Visibility = Visibility.Collapsed;

                            Border scheduledTask = new Border()
                            {
                                Height = task.DurationHours * PxPerHour,
                                Background = childBorder.Background,
                                CornerRadius = new CornerRadius(3),
                            };
                            Label label = new Label() { Content = childLabel.Content };

                            scheduledTask.Child = label;

                            schedulePannel.Children.Add(scheduledTask);
                        }
                    }
                }
            }
        }

        private void GetNewSchedule_Click(object sender, RoutedEventArgs e)
        {
            Schedule schedule = RunGeneticAlgorithm(tasks, fixedTasks);

            ShowSchedule(schedule);
        }
    }
}