using System.Windows.Controls;

namespace BewerbungsTracker
{
    /// <summary>
    /// Interaktionslogik für BewerberAnsicht.xaml
    /// </summary>
    public partial class BewerberAnsicht : UserControl
    {
        public BewerberAnsicht()
        {
            InitializeComponent();
            this.DataContext = new BewerberAnsichtViewModel();
        }
    }
}
