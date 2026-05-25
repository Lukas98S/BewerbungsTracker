using System.Windows;


namespace BewerbungsTracker
{
    /// <summary>
    /// Code-Behind für das Hauptfenster (MainWindow) der Anwendung.
    /// Setzt das MainViewModel als DataContext, um WPF Data Binding zu ermöglichen.
    /// Das MainWindow stellt den Container für die Navigation zwischen Ansichten bereit.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initialisiert das MainWindow und setzt das MainViewModel als DataContext.
        /// Das ViewModel übernimmt die Verwaltung der Ansichten und Navigation.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // MainViewModel wird als DataContext gesetzt - alle Bindings werden damit verknüpft
            this.DataContext = new MainViewModel();
        }
    }
}
