using System.Windows;

namespace Lab2
{
    /// <summary>
    /// Окно для вывода инвормации об результатах алгоритма
    /// </summary>
    public partial class AnalyzeWindow : Window
    {
        public AnalyzeWindow(AlgorithmInfo info)
        {
            InitializeComponent();

            DataContext = info;
        }
    }
}
