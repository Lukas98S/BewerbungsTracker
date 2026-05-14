using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.IO;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;
using MailKit;
using MailKit.Search;
using MailKit.Net.Imap;
using System.Text.Json;

namespace BewerbungsTracker
{
    public class BewerberAnsichtViewModel : INotifyPropertyChanged
    {

        private string _neueFirma;
        private string _neuePosition;

        public string NeueFirma
        {
            get => _neueFirma;
            set { _neueFirma = value; OnPropertyChanged(nameof(NeueFirma)); }
        }

        public  string NeuePosition
        {
            get => _neuePosition;
            set { _neuePosition = value; OnPropertyChanged(nameof(NeuePosition)); }
        }

        public ICommand HinzufuegenCommand { get; }
        public ICommand PdfImportCommand { get; }
        public ICommand AlleLoeschenCommand { get; }

        public ObservableCollection<Bewerbung> Bewerbungen { get; set; }
        public BewerberAnsichtViewModel()
        {
            Bewerbungen = new ObservableCollection<Bewerbung>();
            HinzufuegenCommand = new RelayCommand(ManuellHinzufuegen);
            PdfImportCommand = new RelayCommand(PdfImport);
            AlleLoeschenCommand = new RelayCommand(AlleLoeschen);

            LadeDatenAusDatenbank();

            Task.Run(() => PruefeMails());
        }

        private void PruefeMails()
        {
            if (!File.Exists("secrets.json")) return;
            var jsonText = File.ReadAllText("secrets.json");
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);

            string imapServer = "imap.aol.com";
            int port = 993;
            string emailAdresse = config["Email"];
            string passwort = config["Passwort"];

            try
            {
                using (var client = new ImapClient())
                {
                    client.Connect(imapServer, port, true);
                    client.Authenticate(emailAdresse, passwort);

                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly);

                    var query = SearchQuery.DeliveredAfter(DateTime.Now.AddDays(-14));
                    var wort = SearchQuery.MessageContains("leider").Or(SearchQuery.MessageContains("absage"))
                           .Or(SearchQuery.MessageContains("andere kanidaten")).Or(SearchQuery.MessageContains("bedauern"));

                    var uids = inbox.Search(query.And(wort));
                    int absagenGefunden = 0;

                    using (var db = new BewerbungsContext())
                    {
                        var offene = db.Bewerbungen.Where(b => b.Status == "Offen").ToList();

                        foreach (var uid in uids)
                        {
                            var message = inbox.GetMessage(uid);
                            string gesamtText = (message.Subject + " " + (message.TextBody ?? "")+ " " + message.From).ToLower();

                            foreach (var bewerbung in offene)
                            {
                                if (gesamtText.Contains(bewerbung.Firma.ToLower()))
                                {
                                    bewerbung.Status = "Abgelehnt";
                                    absagenGefunden++;
                                    break;
                                }
                            }
                        }

                        if (absagenGefunden > 0)
                        {
                            db.SaveChanges();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                LadeDatenAusDatenbank();
                                MessageBox.Show($"{absagenGefunden} neue Absagen per E-Mail gefunden !");
                            });
                        }
                    }
                    client.Disconnect(true);
                }
                
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show("E-Mail-Check fehlgeschlagen: " + ex.Message));
            }
        }

        private void LadeDatenAusDatenbank()
        {
            using (var db = new BewerbungsContext())
            {
                var liste = db.Bewerbungen.Where(b => b.Status != "Abgelehnt").ToList();
                Bewerbungen.Clear();
                foreach (var b in liste)
                {
                    Bewerbungen.Add(b);
                }
            }
        }

        private void ManuellHinzufuegen()
        {
            if (string.IsNullOrWhiteSpace(NeueFirma) || string.IsNullOrWhiteSpace(NeuePosition))
            {
                MessageBox.Show("Bitte Firma und Position eintragen");
                return;
            }

            using (var db = new BewerbungsContext())
            {
                var neueBewerbung = new Bewerbung
                {
                    Firma = NeueFirma.Trim(),
                    Position = NeuePosition.Trim(),
                    Datum = DateTime.Now,
                    Status = "Offen"
                };

                db.Bewerbungen.Add(neueBewerbung);
                db.SaveChanges();
                Bewerbungen.Add(neueBewerbung);
            }
            NeueFirma = string.Empty;
            NeuePosition = string.Empty;

            MessageBox.Show("Erfolgreich hinzugefügt");
        }

        private void AlleLoeschen()
        {
            var antwort = MessageBox.Show("Willst du alle Bewerbungen Löschen", "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (antwort == MessageBoxResult.Yes)
            {
                using (var db = new BewerbungsContext())
                {
                    db.Bewerbungen.RemoveRange(db.Bewerbungen);
                    db.SaveChanges();

                    db.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name='Bewerbungen';");
                }

                Bewerbungen.Clear();
            }
        }

        private void PdfImport()
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
                        string dateiName = Path.GetFileNameWithoutExtension(dateiPfad);

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
                                Bewerbungen.Add(neueBewerbung);
                                newAdd++;
                            }
                        }
                    }
                    if (newAdd > 0)
                    {
                        db.SaveChanges();
                        MessageBox.Show($"{newAdd} Neue Bewerbungen erfolgreich hinzugefügt");
                    }
                    else
                    {
                        MessageBox.Show("Keine neuen Bewerbungen gefunden.");
                    }
                }
            }
        }

        public string ReadPosition(string pdfPfad)
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
