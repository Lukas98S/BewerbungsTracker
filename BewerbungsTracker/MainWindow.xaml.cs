using System.Windows;


namespace BewerbungsTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new BewerberAnsicht();
        }

        private void BtnAktive_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new BewerberAnsicht();
        }

        private void BtnArchiv_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hier wird noch gebaut");
        }
    }
}