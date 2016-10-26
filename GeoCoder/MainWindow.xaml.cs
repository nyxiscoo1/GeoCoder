using System.Windows;

namespace GeoCoder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new GeoCoderViewModel();

            Title = "GeoCoder v" + GetType().Assembly.GetName().Version;
        }
    }
}
