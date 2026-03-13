using System.Windows;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Graphics.Operations;

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
                var alleBewerbungen = db.Bewerbungen.ToList();
                BewerbungsTabelle.ItemsSource = alleBewerbungen;
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            string ordnerPfad = @"C:\Users\Lukas\Desktop\Anschreiben";

            if (!Directory.Exists(ordnerPfad))
            {
                MessageBox.Show("Der Ordner wurde nicht gefunden.");
            }

            string[] pdfDateien = Directory.GetFiles(ordnerPfad, "*.pdf");

            using (var db = new BewerbungsContext())
            {
                int newAdd = 0;

                foreach (var dateiPfad in pdfDateien)
                {
                    string dateiName = System.IO.Path.GetFileNameWithoutExtension(dateiPfad);

                    if (dateiName.StartsWith("Anschreiben-"))
                    {
                        string firma = dateiName.Replace("Anschreiben-", "").Trim();

                        if (!db.Bewerbungen.Any(b => b.Firma == firma))
                        {
                            DateTime bewerbungsDatum = File.GetCreationTime(dateiPfad);

                            var neueBewerbung = new Bewerbung
                            {
                                Firma = firma,
                                Position = ReadPosition(dateiPfad),
                                Datum = bewerbungsDatum,
                                Status = "Offen"
                            };

                            db.Bewerbungen.Add(neueBewerbung);
                            newAdd++;
                        }
                    }
                }
                if (newAdd > 0)
                {
                    db.SaveChanges();
                    MessageBox.Show($"{newAdd} Neue Bewerbungen erfolgreich hinzugefügt");
                    LadeDaten();
                }
                else
                {
                    MessageBox.Show("Keine neuen Bewerbungen gefunden.");
                }
            }
        }

        private void BtnManuell_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFirma.Text) || string.IsNullOrWhiteSpace(TxtPosition.Text))
            {
                MessageBox.Show("Bitte Firma und Position eintragen");
                return;
            }

            using (var db = new BewerbungsContext())
            {
                var neueBewerbung = new Bewerbung
                {
                    Firma = TxtFirma.Text.Trim(),
                    Position = TxtPosition.Text.Trim(),
                    Datum = DateTime.Now,
                    Status = "Offen"
                };

                db.Bewerbungen.Add(neueBewerbung);
                db.SaveChanges();
            }
            TxtFirma.Clear();
            TxtPosition.Clear();
            LadeDaten();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var antwort = MessageBox.Show("Willst du alle Bewerbungen Löschen","Achtung", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (antwort == MessageBoxResult.Yes)
            {
                using (var db = new BewerbungsContext())
                {
                    db.Bewerbungen.RemoveRange(db.Bewerbungen);
                    db.SaveChanges();

                    db.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name='Bewerbungen';");
                }
                LadeDaten();
            }
        }
        private string ReadPosition(string pdfPfad)
        {
            try
            {
                using (var document = PdfDocument.Open(pdfPfad))
                {
                    var seite = document.GetPage(1);
                    string text = seite.Text;

                    string suchWort = "Bewerbung als";
                    int startIndex = text.IndexOf(suchWort, StringComparison.OrdinalIgnoreCase);

                    if (startIndex != -1)
                    {
                        startIndex += suchWort.Length;
                        string restText = text.Substring(startIndex);

                        int endIndexN = restText.IndexOf('\n');
                        int endIndexR = restText.IndexOf('\r');

                        int endIndex = restText.Length;
                        if (endIndexN != -1) endIndex = Math.Min(endIndex, endIndexN);
                        if (endIndexR != -1) endIndex = Math.Min(endIndex, endIndexR);

                        string findPosition = restText.Substring(0, endIndex).Trim();

                        int anredeIndex = findPosition.IndexOf("Sehr geehrte", StringComparison.OrdinalIgnoreCase);
                        if (anredeIndex != -1)
                        {
                            findPosition = findPosition.Substring(0, anredeIndex).Trim();
                        }
                        return findPosition;
                    }
                }
            }

            catch
            {
                MessageBox.Show("Fehler beim Lesen der PDF-Datei.");
            }
            return "";
        }
    }
}