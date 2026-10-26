using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace BewerbungsTracker
{
    /// <summary>
    /// ViewModel für die Anzeige der abgelehnten Bewerbungen (Archiv).
    /// Lädt alle Bewerbungen mit dem Status "Abgelehnt" aus der Datenbank
    /// und stellt sie in einer ObservableCollection zur Verfügung.
    /// </summary>
    public class ArchivAnsichtViewModel
    {
        /// <summary>
        /// ObservableCollection mit allen abgelehnten Bewerbungen.
        /// Wird an die ListView in der Archiv-Ansicht gebunden.
        /// </summary>
        public ObservableCollection<Bewerbung> AbgelehnteBewerbungen { get; set; }

        public List<string> StatusListe { get; } = new List<string> { "Offen", "Abgelehnt", "Vorstellungsgespräch", "Zusage!" };
        /// <summary>
        /// Initialisiert das ArchivAnsichtViewModel und lädt alle abgelehnten Bewerbungen aus der Datenbank.
        /// </summary>
        public ArchivAnsichtViewModel()
        {
            AbgelehnteBewerbungen = new ObservableCollection<Bewerbung>();
            LadeAbsagen();
        }

        /// <summary>
        /// Lädt alle Bewerbungen mit dem Status "Abgelehnt" aus der Datenbank
        /// und fügt sie zur ObservableCollection hinzu.
        /// </summary>
        private void LadeAbsagen()
        {
            using (var db = new BewerbungsContext())
            {
                // Alle abgelehnten Bewerbungen werden aus der Datenbank geholt
                var absagen = db.Bewerbungen.Where(b => b.Status == "Abgelehnt").ToList();

                // Jede abgelehnte Bewerbung wird zur Collection hinzugefügt
                foreach (var b in absagen)
                {
                    b.PropertyChanged += Bewerbung_StatusGeandert;
                    AbgelehnteBewerbungen.Add(b);
                }
            }
        }

        private void Bewerbung_StatusGeandert(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Status" && sender is Bewerbung geanderteBewerbung)
            {
                using (var db = new BewerbungsContext())
                {
                    db.Bewerbungen.Update(geanderteBewerbung);
                    db.SaveChanges();
                }
                if(geanderteBewerbung.Status != "Abgelehnt")
                {
                    Application.Current.Dispatcher.InvokeAsync(() => AbgelehnteBewerbungen.Remove(geanderteBewerbung));
                }
            }
        }
    }
}
