using System.Windows.Controls;

namespace BewerbungsTracker
{
    /// <summary>
    /// Code-Behind für die BewerberAnsicht (UserControl).
    /// Zeigt aktive Bewerbungen an und ermöglicht deren Verwaltung.
    /// Setzt das BewerberAnsichtViewModel als DataContext für WPF Data Binding.
    /// </summary>
    public partial class BewerberAnsicht : UserControl
    {
        /// <summary>
        /// Initialisiert das BewerberAnsicht-UserControl und setzt das BewerberAnsichtViewModel als DataContext.
        /// Das ViewModel verwaltet die aktiven Bewerbungen, PDF-Importe und E-Mail-Überprüfungen.
        /// </summary>
        public BewerberAnsicht()
        {
            InitializeComponent();
            // BewerberAnsichtViewModel wird als DataContext gesetzt
            this.DataContext = new BewerberAnsichtViewModel();
        }
    }
}
