using System.Windows;

namespace Lab2
{
    /// <summary>
    /// Логика взаимодействия для AnalyzeWindow.xaml
    /// </summary>
    public partial class AnalyzeWindow : Window
    {
        private AlgorithmInfo info;

        public AnalyzeWindow(AlgorithmInfo info)
        {
            InitializeComponent();

            this.info = info;
            DataContext = info;
        }
    }
}
