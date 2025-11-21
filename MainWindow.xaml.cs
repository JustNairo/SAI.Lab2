using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lab2
{
    public partial class MainWindow : Window
    {
        const int PxPerHour = 80; // Количество пикселей для блока времени в час
        const int maxWorkHours = 8; // Число рабочих часов

        private List<Task> tasks;
        private List<Task> fixedTasks;

        private AlgorithmInfo infoContainer;

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
            infoContainer = new AlgorithmInfo()
            {
                Name = "Генетический алгоритм для составления расписания",
                TaskCount = tasks.Count,
                TotalTime = tasks.Sum(t => t.DurationHours),
                MaxWorkingHours = maxWorkHours,
            };

            // Запускаем генетический алгоритм
            var ga = new ScheduleGeneticAlgorithm(
                infoContainer,
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

        // Метод вывода блоков задач в правую колонку на окне
        private void ShowTasks(List<Task> tasks, List<Task> fixedTasks)
        {
            // Создаем кисти для покраски блоков
            string[] colors = { "#ef9bfa", "#ff9dc4", "#ffb1a1", "#fac593", 
                "#e8da85", "#bbf28f", "#6ef7c8", "#5ee9f7", "#9bd4fa" };
            Brush[] brushes = new Brush[tasks.Count];
            for(int i  = 0; i < tasks.Count; i++)
                brushes[i] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[i]));

            // Создаём сами блоки
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

                // Добавляем на окно
                tasksPannel.Children.Add(newTask);
            }

        }

        // Метод вывода на экран сформированного расписания
        private void ShowSchedule(Schedule schedule)
        {
            DoubleAnimation appearAnim = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.5),
            };

            double totalTime = 0; // Время, занятое задачами

            for (int i = 0; i < schedule.TaskOrder.Count; i++)
            {
                Task task = schedule.TaskOrder[i];

                if (totalTime + task.DurationHours > maxWorkHours)
                    break;
                // if (totalTime + task.DurationHours <= maxWorkHours)
                // {
                totalTime += task.DurationHours;

                // Выводим на окно элемент
                // tasksPannel.Children - Блоки с задачами
                foreach (UIElement element in tasksPannel.Children) // КРИНЖ! - переписать
                {
                    Border childBorder = (Border)element; // Изначально element типа Border
                    Label childLabel = (Label)childBorder.Child; // У каждого элемента обязательно есть Child - Label

                    string taskToString = task.ToString();
                    if (childLabel.Content.ToString() == taskToString) 
                    {
                        Border scheduledTask = new Border()
                        {
                            Height = task.DurationHours * PxPerHour,
                            Background = childBorder.Background,
                            CornerRadius = new CornerRadius(3),
                        };
                        Label label = new Label() { Content = childLabel.Content };

                        scheduledTask.Child = label;

                        element.Visibility = Visibility.Collapsed;
                        scheduledTask.BeginAnimation(OpacityProperty, appearAnim);
                        schedulePannel.Children.Add(scheduledTask);
                    }
                }
                // }
            }
        }

        private void GetNewScheduleBtn_Click(object sender, RoutedEventArgs e)
        {
            Schedule schedule = RunGeneticAlgorithm(tasks, fixedTasks);

            ShowSchedule(schedule);

            RestartBtn.IsEnabled = true;
            AnalyzeBtn.Visibility = Visibility.Visible;
            GetNewScheduleBtn.IsEnabled = false;
        }

        private void RestartBtn_Click(object sender, RoutedEventArgs e)
        {
            GetNewScheduleBtn.IsEnabled = true;
            RestartBtn.IsEnabled = false;
            AnalyzeBtn.Visibility = Visibility.Collapsed;
            Restart();
        }

        private void Restart()
        {
            schedulePannel.Children.Clear();
            foreach(UIElement element in tasksPannel.Children)
            {
                element.Visibility = Visibility.Visible;
                element.Opacity = 1.0;
            }
        }

        private void AnalyzeBtn_Click(object sender, RoutedEventArgs e)
        {
            AnalyzeWindow analyzeWindow = new AnalyzeWindow(infoContainer);
            analyzeWindow.Owner = this;
            analyzeWindow.ShowDialog();
        }
    }
}