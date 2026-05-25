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
    /// <summary>
    /// ViewModel für die Anzeige und Verwaltung von aktiven Bewerbungen.
    /// Bietet Funktionalität zum:
    /// - Hinzufügen von Bewerbungen (manuell oder per PDF-Import)
    /// - Automatische Überprüfung von Absagen via Email (IMAP)
    /// - Statusverwaltung von Bewerbungen
    /// - Anzeige und Verwaltung in der Bewerber-Ansicht
    /// </summary>
    public class BewerberAnsichtViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Backing-Field für die neue Firma.
        /// </summary>
        private string _neueFirma;

        /// <summary>
        /// Backing-Field für die neue Position.
        /// </summary>
        private string _neuePosition;

        /// <summary>
        /// Liste der möglichen Status für eine Bewerbung.
        /// Wird in der UI als Dropdown angeboten.
        /// </summary>
        public List<string> StatusListe { get; } = new List<string> { "Offen", "Abgelehnt", "Zusage!" };

        /// <summary>
        /// Die neue Firma für manuelle Bewerbungseingabe.
        /// </summary>
        public string NeueFirma
        {
            get => _neueFirma;
            set { _neueFirma = value; OnPropertyChanged(nameof(NeueFirma)); }
        }

        /// <summary>
        /// Die neue Position für manuelle Bewerbungseingabe.
        /// </summary>
        public  string NeuePosition
        {
            get => _neuePosition;
            set { _neuePosition = value; OnPropertyChanged(nameof(NeuePosition)); }
        }

        /// <summary>
        /// Command zum manuellen Hinzufügen einer Bewerbung.
        /// </summary>
        public ICommand HinzufuegenCommand { get; }

        /// <summary>
        /// Command zum Importieren von Bewerbungen aus PDF-Dateien.
        /// </summary>
        public ICommand PdfImportCommand { get; }

        /// <summary>
        /// Command zum Löschen aller Bewerbungen.
        /// </summary>
        public ICommand AlleLoeschenCommand { get; }

        /// <summary>
        /// ObservableCollection mit allen aktiven Bewerbungen.
        /// Wird an die ListView in der UI gebunden.
        /// </summary>
        public ObservableCollection<Bewerbung> Bewerbungen { get; set; }

        /// <summary>
        /// Initialisiert das BewerberAnsichtViewModel.
        /// Lädt Bewerbungen aus der Datenbank und startet die automatische E-Mail-Überprüfung.
        /// </summary>
        public BewerberAnsichtViewModel()
        {
            // ObservableCollection wird initialisiert
            Bewerbungen = new ObservableCollection<Bewerbung>();

            // Commands werden mit ihren Handler-Methoden verbunden
            HinzufuegenCommand = new RelayCommand(ManuellHinzufuegen);
            PdfImportCommand = new RelayCommand(PdfImport);
            AlleLoeschenCommand = new RelayCommand(AlleLoeschen);

            // Vorhandene Bewerbungen werden aus der Datenbank geladen
            LadeDatenAusDatenbank();

            // Asynchrone Überprüfung von neuen Absagen-E-Mails wird gestartet
            _ = PruefeMails();
        }

        /// <summary>
        /// Überprüft die E-Mails der letzten 14 Tage auf automatische Absagen.
        /// Verbindet sich mit dem konfigurierten IMAP-Server und sucht nach Schlüsselwörtern
        /// wie "leider", "absage", etc. bei den konfigurierten Firmen.
        /// Aktualisiert den Status zu "Abgelehnt" bei gefundenen Absagen.
        /// </summary>
        private async Task PruefeMails()
        {
            // Wenn secrets.json nicht existiert, wurde die App nicht konfiguriert
            if (!File.Exists("secrets.json")) return;

            // Konfiguration wird aus secrets.json gelesen
            var jsonText = await File.ReadAllTextAsync("secrets.json");
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);

            // IMAP-Verbindungsparameter werden aus der Konfiguration ausgelesen
            int port = 993;
            string imapServer = config["ImapServer"];
            string emailAdresse = config["Email"];
            // Passwort wird entschlüsselt
            string passwort = Krypto.HashDe(config["Passwort"]);

            try
            {
                using (var client = new ImapClient())
                {
                    // Verbindung zum IMAP-Server wird hergestellt
                    await client.ConnectAsync(imapServer, port, true);
                    // Authentifizierung mit Email und Passwort
                    await client.AuthenticateAsync(emailAdresse, passwort);

                    // Posteingang wird geöffnet (Read-Only)
                    var inbox = client.Inbox;
                    await inbox.OpenAsync(FolderAccess.ReadOnly);

                    // Suche nach E-Mails der letzten 14 Tage
                    var query = SearchQuery.DeliveredAfter(DateTime.Now.AddDays(-14));

                    // Suche nach E-Mails mit Absage-Schlüsselwörtern
                    var wort = SearchQuery.MessageContains("leider").Or(SearchQuery.MessageContains("absage"))
                           .Or(SearchQuery.MessageContains("andere kanidaten")).Or(SearchQuery.MessageContains("bedauern"));

                    // Suche wird kombiniert: E-Mails der letzten 14 Tage UND mit Absage-Schlüsselwörtern
                    var uids = await inbox.SearchAsync(query.And(wort));
                    int absagenGefunden = 0;

                    using (var db = new BewerbungsContext())
                    {
                        // Alle noch offenen Bewerbungen werden aus der Datenbank geholt
                        var offene = await db.Bewerbungen.Where(b => b.Status == "Offen").ToListAsync();

                        // Jede gefundene E-Mail wird überprüft
                        foreach (var uid in uids)
                        {
                            var message = await inbox.GetMessageAsync(uid);
                            // Text aus Betreff, Body und Absender wird kombiniert
                            string gesamtText = (message.Subject + " " + (message.TextBody ?? "")+ " " + message.From).ToLower();

                            // Überprüfung ob die E-Mail zu einer offenen Bewerbung passt
                            foreach (var bewerbung in offene)
                            {
                                // Wenn der Firmenname in der E-Mail vorkommt, wird die Bewerbung als abgelehnt markiert
                                if (gesamtText.Contains(bewerbung.Firma.ToLower()))
                                {
                                    bewerbung.Status = "Abgelehnt";
                                    absagenGefunden++;
                                    break;
                                }
                            }
                        }

                        // Wenn Absagen gefunden wurden, werden sie gespeichert und die UI aktualisiert
                        if (absagenGefunden > 0)
                        {
                            await db.SaveChangesAsync();
                            LadeDatenAusDatenbank();
                            MessageBox.Show($"{absagenGefunden} neue Absagen per E-Mail gefunden !");

                        }
                    }
                    await client.DisconnectAsync(true);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("E-Mail-Check fehlgeschlagen: " + ex.Message);
            }
        }

        /// <summary>
        /// Lädt alle nicht abgelehnten Bewerbungen aus der Datenbank und zeigt sie an.
        /// Überschreibt die aktuelle Liste und registriert PropertyChanged-Events.
        /// </summary>
        private void LadeDatenAusDatenbank()
        {
            using (var db = new BewerbungsContext())
            {
                // Alle nicht abgelehnten Bewerbungen werden aus der Datenbank geladen
                var liste = db.Bewerbungen.Where(b => b.Status != "Abgelehnt").ToList();

                // Die ObservableCollection wird geleert
                Bewerbungen.Clear();

                // Jede Bewerbung wird hinzugefügt und erhält einen Event-Handler
                foreach (var b in liste)
                {
                    // PropertyChanged-Event wird registriert, um Statusänderungen zu verfolgen
                    b.PropertyChanged += Bewerbung_StatusGeandert;
                    Bewerbungen.Add(b);
                }
            }
        }

        /// <summary>
        /// Fügt eine Bewerbung manuell zur Datenbank und zur Anzeige hinzu.
        /// Validiert, dass Firma und Position nicht leer sind.
        /// Das Datum wird auf das aktuelle Datum gesetzt und der Status auf "Offen".
        /// </summary>
        private void ManuellHinzufuegen()
        {
            // Validierung: Firma und Position müssen gefüllt sein
            if (string.IsNullOrWhiteSpace(NeueFirma) || string.IsNullOrWhiteSpace(NeuePosition))
            {
                MessageBox.Show("Bitte Firma und Position eintragen");
                return;
            }

            using (var db = new BewerbungsContext())
            {
                // Neue Bewerbung wird erstellt
                var neueBewerbung = new Bewerbung
                {
                    Firma = NeueFirma.Trim(),
                    Position = NeuePosition.Trim(),
                    Datum = DateTime.Now,
                    Status = "Offen"
                };

                // Bewerbung wird zur Datenbank hinzugefügt
                db.Bewerbungen.Add(neueBewerbung);
                db.SaveChanges();

                // Event-Handler wird registriert
                neueBewerbung.PropertyChanged += Bewerbung_StatusGeandert;

                // Bewerbung wird zur ObservableCollection hinzugefügt (wird in der UI angezeigt)
                Bewerbungen.Add(neueBewerbung);
            }

            // Eingabefelder werden geleert
            NeueFirma = string.Empty;
            NeuePosition = string.Empty;

            MessageBox.Show("Erfolgreich hinzugefügt");
        }

        /// <summary>
        /// Event-Handler, der aufgerufen wird, wenn sich der Status einer Bewerbung ändert.
        /// Wenn der Status auf "Abgelehnt" gesetzt wird, wird die Bewerbung in der Datenbank
        /// aktualisiert und aus der ObservableCollection entfernt.
        /// </summary>
        /// <param name="sender">Die Bewerbung, deren Status sich geändert hat</param>
        /// <param name="e">Die PropertyChanged-Event-Argumente</param>
        private void Bewerbung_StatusGeandert(object sender, PropertyChangedEventArgs e)
        {
            // Nur wenn die Status-Property sich ändert, wird diese Methode relevant
            if (e.PropertyName == "Status")
            {
                // Wenn die Bewerbung auf "Abgelehnt" gesetzt wird, wird sie archiviert
                if(sender is Bewerbung geanderteBewerbung && geanderteBewerbung.Status == "Abgelehnt")
                {
                    // Die Änderung wird in der Datenbank gespeichert
                    using(var db = new BewerbungsContext())
                    {
                        db.Bewerbungen.Update(geanderteBewerbung);
                        db.SaveChanges();
                    }

                    // Die Bewerbung wird aus der Anzeige entfernt (wird per Dispatcher aufgerufen, da es in Event stattfindet)
                    Application.Current.Dispatcher.InvokeAsync(() => Bewerbungen.Remove(geanderteBewerbung));
                }
            }
        }

        /// <summary>
        /// Löscht ALLE Bewerbungen nach Bestätigung durch den Nutzer.
        /// Leert sowohl die Datenbank als auch die ID-Sequenz (für SQLite).
        /// </summary>
        private void AlleLoeschen()
        {
            // Bestätigungsdialog wird angezeigt
            var antwort = MessageBox.Show("Willst du alle Bewerbungen Löschen", "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (antwort == MessageBoxResult.Yes)
            {
                using (var db = new BewerbungsContext())
                {
                    // Alle Bewerbungen werden aus der Datenbank gelöscht
                    db.Bewerbungen.RemoveRange(db.Bewerbungen);
                    db.SaveChanges();

                    // Die SQLite-Sequenz wird zurückgesetzt (damit neue IDs wieder bei 1 anfangen)
                    db.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name='Bewerbungen';");
                }

                // Die ObservableCollection wird geleert
                Bewerbungen.Clear();
            }
        }

        /// <summary>
        /// Importiert Bewerbungen aus PDF-Anschreiben aus einem Ordner.
        /// Erkennt PDF-Dateien mit Namen "Anschreiben-[Firmenname].pdf" und extrahiert
        /// die Position aus dem PDF-Text durch Suche nach "Bewerbung als".
        /// </summary>
        /// <summary>
        /// Importiert Bewerbungen aus PDF-Anschreiben aus einem Ordner.
        /// Erkennt PDF-Dateien mit Namen "Anschreiben-[Firmenname].pdf" und extrahiert
        /// die Position aus dem PDF-Text durch Suche nach "Bewerbung als".
        /// </summary>
        private void PdfImport()
        {
            // Dialog zur Auswahl eines Ordners wird angezeigt
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Wählen den Ordner mit deinen PDF-Anschreiben"
            };

            if (dialog.ShowDialog() == true)
            {

                // Der ausgewählte Ordner wird geholt
                string ordnerPfad = dialog.FolderName;

                // Alle PDF-Dateien im Ordner werden aufgelistet
                string[] pdfDateien = Directory.GetFiles(ordnerPfad, "*.pdf");

                using (var db = new BewerbungsContext())
                {
                    int newAdd = 0;

                    // Jede PDF-Datei wird überprüft
                    foreach (var dateiPfad in pdfDateien)
                    {
                        // Der Dateiname ohne Erweiterung wird geholt
                        string dateiName = Path.GetFileNameWithoutExtension(dateiPfad);

                        // Überprüfung, ob die Datei dem Namensformat "Anschreiben-[Firmenname]" entspricht
                        if (dateiName.StartsWith("Anschreiben-"))
                        {
                            // Der Firmenname wird aus dem Dateinamen extrahiert
                            string firma = dateiName.Replace("Anschreiben-", "").Trim();

                            // Überprüfung, ob diese Firma bereits in der Datenbank existiert
                            if (!db.Bewerbungen.Any(b => b.Firma == firma))
                            {
                                // Das Erstellungsdatum der PDF-Datei wird als Bewerbungsdatum verwendet
                                DateTime bewerbungsDatum = File.GetCreationTime(dateiPfad);

                                // Neue Bewerbung wird erstellt
                                var neueBewerbung = new Bewerbung
                                {
                                    Firma = firma,
                                    // Die Position wird aus der PDF extrahiert
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
                    // Feedback an den Nutzer
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
                    // Die erste Seite des PDFs wird geholt
                    var seite = document.GetPage(1);
                    // Der gesamte Text wird extrahiert
                    string text = seite.Text;

                    // Suchwort wird definiert
                    string suchWort = "Bewerbung als";
                    // Position des Suchwortes wird gesucht
                    int startIndex = text.IndexOf(suchWort, StringComparison.OrdinalIgnoreCase);

                    if (startIndex != -1)
                    {
                        // Der Index wird nach dem Suchwort positioniert
                        startIndex += suchWort.Length;
                        // Der Text ab dieser Position wird extrahiert
                        string restText = text.Substring(startIndex);

                        // Die nächsten Zeichen bis zu einem Zeilenumbruch werden extrahiert
                        int endIndexN = restText.IndexOf('\n');
                        int endIndexR = restText.IndexOf('\r');

                        // Der früheste Zeilenumbruch wird als Ende verwendet
                        int endIndex = restText.Length;
                        if (endIndexN != -1) endIndex = Math.Min(endIndex, endIndexN);
                        if (endIndexR != -1) endIndex = Math.Min(endIndex, endIndexR);

                        // Die Position wird extrahiert und Leerzeichen entfernt
                        string findPosition = restText.Substring(0, endIndex).Trim();

                        // Überprüfung, ob die Anrede "Sehr geehrte" direkt danach folgt
                        int anredeIndex = findPosition.IndexOf("Sehr geehrte", StringComparison.OrdinalIgnoreCase);
                        if (anredeIndex != -1)
                        {
                            // Die Anrede wird entfernt
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

        /// <summary>
        /// Event, das ausgelöst wird, wenn sich eine Property ändert.
        /// Wird für WPF Data Binding verwendet, um die UI automatisch zu aktualisieren.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Löst das PropertyChanged-Event aus und benachrichtigt die UI über Änderungen.
        /// </summary>
        /// <param name="name">Der Name der Property, die sich geändert hat</param>
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
