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

            // Выводим блоки задач на экран
            ShowTasks(tasks);
        }

        private Schedule RunGeneticAlgorithm(List<Task> tasks, List<Task> fixedTasks)
        {
            // Создаём контейнер для хранении информации об алгоритме
            infoContainer = new AlgorithmInfo()
            {
                Name = "Генетический алгоритм для составления расписания",
                TaskCount = tasks.Count,
                TotalTime = tasks.Sum(t => t.DurationHours),
                MaxWorkingHours = maxWorkHours,
            };

            // Создаём генетический алгоритм
            ScheduleGeneticAlgorithm ga = new ScheduleGeneticAlgorithm(
                infoContainer,
                tasksToSchedule: tasks,
                fixedTasks: fixedTasks,
                populationSize: 100,
                mutationRate: 0.08,
                crossoverRate: 0.85,
                maxWorkHours: maxWorkHours
            );

            //Запускаем генетический алгоритм
            Schedule bestSchedule = ga.Run(maxGenerations: 200);

            return bestSchedule;
        }

        // Метод вывода блоков задач (списка задач) в правую колонку на окне 
        private void ShowTasks(List<Task> tasks)
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
            // Анимация появляения блоков задач в расписании
            DoubleAnimation appearAnim = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.5),
            };

            double totalTime = 0; // Время, занятое задачами

            foreach(Task task in schedule.TaskOrder)
            {
                if (totalTime + task.DurationHours > maxWorkHours)
                    break;

                totalTime += task.DurationHours;

                Label label = new Label() { Content = task.ToString() };
                Border scheduledTaskBlock = new Border()
                {
                    Height = task.DurationHours * PxPerHour,
                    CornerRadius = new CornerRadius(3),
                    Child = label,
                };

                // Ищем эту задачу в выведенных на экран
                foreach (UIElement element in tasksPannel.Children)
                {
                    Border childBorder = (Border)element; // Изначально element типа Border
                    Label childLabel = (Label)childBorder.Child; // У каждого элемента обязательно есть Child - Label

                    if (childLabel.Content.ToString() == task.ToString()) 
                    {
                        scheduledTaskBlock.Background = childBorder.Background;

                        element.Visibility = Visibility.Collapsed;
                        scheduledTaskBlock.BeginAnimation(OpacityProperty, appearAnim);
                        schedulePannel.Children.Add(scheduledTaskBlock);

                        break;
                    }
                }
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