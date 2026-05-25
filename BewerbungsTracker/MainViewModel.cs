using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;

namespace BewerbungsTracker
{
    /// <summary>
    /// Hauptes ViewModel für die Anwendung.
    /// Verwaltet die Navigation zwischen den verschiedenen Ansichten (Bewerber, Archiv, Setup)
    /// und den Sichtbarkeitsstatus des Menüs.
    /// Implementiert INotifyPropertyChanged für WPF Data Binding.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Backing-Field für die aktuell angezeigte Ansicht (ViewModel).
        /// </summary>
        private object _aktuelleAnsicht;

        /// <summary>
        /// Die aktuell in der Anwendung angezeigte Ansicht.
        /// Wird an die ContentControl der MainWindow gebunden und ermöglicht View-Wechsel.
        /// </summary>
        public object AktuelleAnsicht
        {
            get => _aktuelleAnsicht;
            set { _aktuelleAnsicht = value; OnPropertyChanged(nameof(AktuelleAnsicht)); }
        }

        /// <summary>
        /// Backing-Field für die Menü-Sichtbarkeit.
        /// </summary>
        private bool _menuVisible;

        /// <summary>
        /// Bestimmt, ob das Menü sichtbar sein soll.
        /// Das Menü ist nur sichtbar, wenn die Anwendung bereits konfiguriert wurde (secrets.json existiert).
        /// </summary>
        public bool MenuVisible
        {
            get => _menuVisible;
            set { _menuVisible = value; OnPropertyChanged(nameof(MenuVisible)); }
        }

        /// <summary>
        /// Command zum Anzeigen der aktiven Bewerbungen (BewerberAnsicht).
        /// </summary>
        public ICommand ZeigeAktiveCommand { get; }

        /// <summary>
        /// Command zum Anzeigen der archivierten Bewerbungen (ArchivAnsicht).
        /// </summary>
        public ICommand ZeigeArchivCommand { get; }

        /// <summary>
        /// Initialisiert das MainViewModel und konfiguriert die Navigation.
        /// Prüft, ob die Anwendung bereits konfiguriert wurde (secrets.json existiert).
        /// Falls nicht, wird das Setup angezeigt; andernfalls die aktiven Bewerbungen.
        /// </summary>
        public MainViewModel()
        {
            // Initialisiert die Commands für die Navigation zwischen Ansichten
            ZeigeAktiveCommand = new RelayCommand(() => AktuelleAnsicht = new BewerberAnsichtViewModel());
            ZeigeArchivCommand = new RelayCommand(() => AktuelleAnsicht = new ArchivAnsichtViewModel());

            // Standard-Ansicht wird gesetzt
            AktuelleAnsicht = new BewerberAnsichtViewModel();

            // Prüfung: Wenn secrets.json vorhanden ist, war die App bereits konfiguriert
            if (System.IO.File.Exists("secrets.json"))
            {
                // Menü ist sichtbar, wenn die App konfiguriert wurde
                MenuVisible = true;
                AktuelleAnsicht = new BewerberAnsichtViewModel();
            }
            else
            {
                // Menü wird versteckt, während das Setup läuft
                MenuVisible = false;

                // Setup-ViewModel wird erstellt und konfiguriert
                var setupVm = new SetupViewModel();

                // SetupSuccess-Callback wird definiert: Nach erfolgreichem Setup wird das Menü sichtbar gemacht
                // und zur Bewerber-Ansicht gewechselt
                setupVm.SetupSuccess = () => { MenuVisible = true; AktuelleAnsicht = new BewerberAnsichtViewModel(); };

                // Setup-Ansicht wird angezeigt
                AktuelleAnsicht = setupVm;
            }
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
