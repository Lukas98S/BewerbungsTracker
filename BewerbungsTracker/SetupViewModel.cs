using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Input;

namespace BewerbungsTracker
{
    /// <summary>
    /// ViewModel für die Konfiguration der Email-Verbindung.
    /// Verwaltet Email-Adresse, Passwort und IMAP-Server-Einstellungen.
    /// Bietet Auto-Fill-Funktionalität für bekannte Email-Provider.
    /// Speichert Konfiguration verschlüsselt in secrets.json.
    /// </summary>
    public class SetupViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Backing-Field für die Email-Adresse.
        /// </summary>
        private string _email;

        /// <summary>
        /// Die Email-Adresse des Benutzers.
        /// Auslösen des AutoFill-Vorgangs, um den IMAP-Server automatisch zu setzen.
        /// </summary>
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                // Auto-Fill wird ausgelöst, wenn Email geändert wird
                AutoFill();
            }
        }

        /// <summary>
        /// Backing-Field für das Email-Passwort.
        /// </summary>
        private string _passwort;

        /// <summary>
        /// Das Passwort für die Email-Authentifizierung.
        /// </summary>
        public string Passwort
        {
            get => _passwort;
            set { _passwort = value; OnPropertyChanged(nameof(Passwort)); }
        }

        /// <summary>
        /// Backing-Field für den IMAP-Server.
        /// </summary>
        private string _imapServer;

        /// <summary>
        /// Die Adresse des IMAP-Servers (z.B. imap.gmail.com).
        /// Wird automatisch basierend auf der Email-Domain gefüllt.
        /// </summary>
        public string ImapServer
        {
            get => _imapServer;
            set { _imapServer = value; OnPropertyChanged(nameof(ImapServer)); }
        }

        /// <summary>
        /// Command zum Speichern der Konfiguration.
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Callback-Action, die nach erfolgreichem Setup aufgerufen wird.
        /// Wird vom MainViewModel gesetzt, um die Navigation zur Hauptansicht zu auslösen.
        /// </summary>
        public Action SetupSuccess { get; set; }

        /// <summary>
        /// Initialisiert das SetupViewModel und konfiguriert das SaveCommand.
        /// </summary>
        public SetupViewModel()
        {
            SaveCommand = new RelayCommand(Save);
        }

        /// <summary>
        /// Automatische Ausfüllung des IMAP-Servers basierend auf der Email-Domain.
        /// Unterstützt gängige Email-Provider wie Gmail, GMX, Web.de, Outlook, Yahoo, etc.
        /// </summary>
        private void AutoFill()
        {
            // Wenn keine gültige Email mit @ vorhanden ist, wird abgebrochen
            if(string.IsNullOrEmpty(Email) || !Email.Contains("@")) return;

            // Email wird in Lokalteil und Domain aufgeteilt
            string[] parts = Email.Split('@');
            if(parts.Length < 2) return;

            // Domain wird extrahiert und in Kleinbuchstaben konvertiert
            string domain = parts[1].ToLower();

            // IMAP-Server wird basierend auf der Email-Domain gesetzt
            switch (domain)
            {
                case "gmail.com":
                case "googlemail.com":
                    ImapServer = "imap.gmail.com";
                    break;

                case "gmx.de":
                case "gmx.net":
                    ImapServer = "imap.gmx.net";
                    break;

                case "web.de":
                    ImapServer = "imap.web.de";
                    break;

                case "outlook.com":
                case "hotmail.com":
                    ImapServer = "outlook.office365.com";
                    break;

                case "yahoo.com":
                case "yahoo.de":
                    ImapServer = "imap.mail.yahoo.com";
                    break;

                case "aol.com":
                    ImapServer = "imap.aol.com";
                    break;

                case "t-online.de":
                    ImapServer = "secureimap.t-oonline.de";
                    break;

                case "icloud.com":
                    ImapServer = "imap.mail.me.com";
                    break;
            }
        }

        /// <summary>
        /// Speichert die Konfiguration in die secrets.json-Datei.
        /// Validiert, dass alle erforderlichen Felder gefüllt sind.
        /// Verschlüsselt das Passwort mit der Krypto-Klasse.
        /// Ruft SetupSuccess auf, um die UI zu aktualisieren.
        /// </summary>
        private void Save()
        {
            // Validierung: Alle Felder müssen gefüllt sein
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Passwort) || string.IsNullOrWhiteSpace(ImapServer)) return;

            // Dictionary mit den Konfigurationswerten wird erstellt
            var config = new Dictionary<string, string>
            {
             {"Email", Email.Trim()},
             // Passwort wird mit der Krypto-Klasse verschlüsselt gespeichert
             {"Passwort", Krypto.HashEn(Passwort.Trim())},
             {"ImapServer", ImapServer.Trim()}
            };

            // Dictionary wird zu JSON serialisiert
            string jsonText = JsonSerializer.Serialize(config);

            // JSON wird in secrets.json gespeichert
            File.WriteAllText("secrets.json", jsonText);

            // SetupSuccess-Callback wird aufgerufen, um die Anwendung fortzusetzen
            SetupSuccess?.Invoke();
        }

        /// <summary>
        /// Event, das ausgelöst wird, wenn sich eine Property ändert.
        /// Wird für WPF Data Binding verwendet, um die UI automatisch zu aktualisieren.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Löst das PropertyChanged-Event aus und benachrichtigt die UI über Änderungen.
        /// </summary>
        /// <param name="name">Der Name der Property, die sich geändert hat</param>
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
