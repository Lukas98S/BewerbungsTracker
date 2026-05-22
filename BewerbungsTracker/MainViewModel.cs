using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;

namespace BewerbungsTracker
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _aktuelleAnsicht;
        public object AktuelleAnsicht
        {
            get => _aktuelleAnsicht;
            set { _aktuelleAnsicht = value; OnPropertyChanged(nameof(AktuelleAnsicht)); }
        }
        private bool _menuVisible;
        public bool MenuVisible
        {
            get => _menuVisible;
            set { _menuVisible = value; OnPropertyChanged(nameof(MenuVisible)); }
        }

        public ICommand ZeigeAktiveCommand { get; }
        public ICommand ZeigeArchivCommand { get; }

        public MainViewModel()
        {
            ZeigeAktiveCommand = new RelayCommand(() => AktuelleAnsicht = new BewerberAnsichtViewModel());
            ZeigeArchivCommand = new RelayCommand(() => AktuelleAnsicht = new ArchivAnsichtViewModel());

            AktuelleAnsicht = new BewerberAnsichtViewModel();

            if (System.IO.File.Exists("secrets.json"))
            {
                MenuVisible = true;
                AktuelleAnsicht = new BewerberAnsichtViewModel();
            }
            else
            {
                MenuVisible = false;
                var setupVm = new SetupViewModel();
                setupVm.SetupSuccess = () => { MenuVisible = true; AktuelleAnsicht = new BewerberAnsichtViewModel(); };
            
                AktuelleAnsicht = setupVm;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
