using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Input;

namespace BewerbungsTracker
{
    public class SetupViewModel : INotifyPropertyChanged
    {
        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                AutoFill();
            }
        }

        private string _passwort;
        public string Passwort
        {
            get => _passwort;
            set { _passwort = value; OnPropertyChanged(nameof(Passwort)); }
        }

        private string _imapServer;
        public string ImapServer
        {
            get => _imapServer;
            set { _imapServer = value; OnPropertyChanged(nameof(ImapServer)); }
        }

        public ICommand SaveCommand { get; }
        public Action SetupSuccess { get; set; }

        public SetupViewModel()
        {
            SaveCommand = new RelayCommand(Save);
        }
        private void AutoFill()
        {
            if(string.IsNullOrEmpty(Email) || !Email.Contains("@")) return;

            string[] parts = Email.Split('@');
            if(parts.Length < 2) return;

            string domain = parts[1].ToLower();
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

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Passwort) || string.IsNullOrWhiteSpace(ImapServer)) return;
            
            var config = new Dictionary<string, string>
            {
             {"Email", Email.Trim()},
             {"Passwort",Krypto.HashEn(Passwort.Trim())},
             {"ImapServer", ImapServer.Trim()}
            };
            string jsonText = JsonSerializer.Serialize(config);
            File.WriteAllText("secrets.json", jsonText);

            SetupSuccess?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
