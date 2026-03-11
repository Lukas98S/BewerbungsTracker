using System;
using System.Collections.Generic;
using System.Text;

namespace BewerbungsTracker
{
    public class Bewerbung
    {
        public int Id { get; set; }
        public string? Firma { get; set; }
        public string? Position { get; set; }
        public DateTime Datum { get; set; }
        public string Status { get; set; } = "Offen";
    }
}
