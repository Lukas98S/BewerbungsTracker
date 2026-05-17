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

        public ICommand ZeigeAktiveCommand { get; }
        public ICommand ZeigeArchivCommand { get; }

        public MainViewModel()
        {
            ZeigeAktiveCommand = new RelayCommand(() => AktuelleAnsicht = new BewerberAnsichtViewModel());
            ZeigeArchivCommand = new RelayCommand(() => AktuelleAnsicht = new ArchivAnsichtViewModel());

            AktuelleAnsicht = new BewerberAnsichtViewModel();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
