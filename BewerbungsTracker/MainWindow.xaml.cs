using System.Windows;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Graphics.Operations;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;

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
            Task.Run(() =>
            {
                PruefeMails();
            });
        }


        public void LadeDaten()
        {
            using (var db = new BewerbungsContext())
            {
                db.Database.Migrate();
                var alleBewerbungen = db.Bewerbungen.Where(b => b.Status != "Abgelehnt").ToList();
                BewerbungsTabelle.ItemsSource = alleBewerbungen;
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Wählen den Ordner mit deinen PDF-Anschreiben"
            };

            if (dialog.ShowDialog() == true)
            {

                string ordnerPfad = dialog.FolderName;

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

        private void PruefeMails()
        {
            string imapServer = "imap.aol.com";
            int port = 993;
            string emailAdresse = "Lukasschuetz98@aol.com";
            string passwort = "sewbkfqsgmgqskxv";

            try
            {
                using (var client =  new ImapClient())
                {
                    client.Connect(imapServer, port, true);
                    client.Authenticate(emailAdresse, passwort);

                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly);

                    var query = SearchQuery.DeliveredAfter(DateTime.Now.AddDays(-14));
                    var uids = inbox.Search(query);

                    int absagen = 0;

                    using ( var db = new BewerbungsContext())
                    {
                        var offene = db.Bewerbungen.Where(b => b.Status == "Offen").ToList();

                        foreach (var uid in uids)
                        {
                           try { 
                                var message = inbox.GetMessage(uid);
                                string text = (message.TextBody ?? "") + " " + (message.HtmlBody ?? "");
                                string betreff = message.Subject ?? "";
                                string absender = message.From.ToString();

                                string gesamtText = (betreff + " " + text + " " + absender).ToLower();

                                if (gesamtText.Contains("leider") || gesamtText.Contains("absage") || gesamtText.Contains("andere kandidaten") || gesamtText.Contains("bedauern"))
                                {
                                    foreach (var bewerbung in offene.ToList())
                                    {
                                        string firmenName = (bewerbung.Firma ?? "").ToLower();
                                        if (!string.IsNullOrWhiteSpace(firmenName) && gesamtText.Contains(firmenName))
                                        {
                                            bewerbung.Status = "Abgelehnt";
                                            absagen++;
                                            offene.Remove(bewerbung);
                                            break;
                                        }
                                    }
                                }
                           }
                            catch
                            {
                                continue;
                            }
                        }

                        if (absagen > 0)
                        {
                            db.SaveChanges();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show($"{absagen} neue Absage per E-Mail gefunden Startus wurde aktualisiert!");
                                LadeDaten();
                            });
                        }
                    }
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("E-Mails Prüfen ist fehlgeschlagen" + ex.Message);
                });
            }
        }
    }
}