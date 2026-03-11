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
using Microsoft.EntityFrameworkCore;

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
            LadeDaten();
        }

        public void LadeDaten()
        {
            using (var db = new BewerbungsContext())
            {
                db.Database.Migrate();
                if (!db.Bewerbungen.Any())
                {
                    var testBewerbung = new Bewerbung
                    {
                        Firma = "Version AG",
                        Position = "Softwareentwickler",
                        Datum = DateTime.Now,
                        Status = "Offen"
                    };

                    db.Bewerbungen.Add(testBewerbung);
                    db.SaveChanges();
                }

                var alleBewerbungen = db.Bewerbungen.ToList();
                BewerbungsTabelle.ItemsSource = alleBewerbungen;
            }
        }
    }
}