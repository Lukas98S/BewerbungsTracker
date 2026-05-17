using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BewerbungsTracker
{
    public class Bewerbung
    {
        public int Id { get; set; }
        public string? Firma { get; set; }
        public string? Position { get; set; }
        public DateTime Datum { get; set; }
        public string? _status;
        public string Status
        {
            get => _status;
            set
            {
                if(_status != value)
                {
                    _status = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
