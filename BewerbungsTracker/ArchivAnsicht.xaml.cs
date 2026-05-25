using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BewerbungsTracker
{
    /// <summary>
    /// Code-Behind für die ArchivAnsicht (UserControl).
    /// Zeigt abgelehnte Bewerbungen (Archiv) an.
    /// Das ViewModel wird vom XAML-Designer automatisch initialisiert.
    /// </summary>
    public partial class ArchivAnsicht : UserControl
    {
        /// <summary>
        /// Initialisiert das ArchivAnsicht-UserControl.
        /// Anmerkung: Das DataContext wird üblicherweise im XAML definiert, nicht im Code-Behind.
        /// </summary>
        public ArchivAnsicht()
        {
            InitializeComponent();
        }
    }
}
