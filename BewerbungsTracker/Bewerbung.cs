using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BewerbungsTracker
{
    /// <summary>
    /// Entität-Klasse für eine Bewerbung.
    /// Repräsentiert eine einzelne Bewerbung mit Informationen zu Firma, Position und Status.
    /// Implementiert INotifyPropertyChanged, um UI-Binding bei Statusänderungen zu unterstützen.
    /// </summary>
    public class Bewerbung
    {
        /// <summary>
        /// Eindeutige Identifikation der Bewerbung in der Datenbank.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Der Name der Firma, bei der die Bewerbung eingereicht wurde.
        /// </summary>
        public string? Firma { get; set; }

        /// <summary>
        /// Die Position, auf die sich beworben wurde.
        /// </summary>
        public string? Position { get; set; }

        /// <summary>
        /// Datum, an dem die Bewerbung eingereicht wurde.
        /// </summary>
        public DateTime Datum { get; set; }

        /// <summary>
        /// Backing-Field für die Status-Property.
        /// Speichert den aktuellen Status der Bewerbung (z.B. "Ausstehend", "Abgelehnt", "Interview").
        /// </summary>
        public string? _status;

        /// <summary>
        /// Der aktuelle Status der Bewerbung.
        /// Löst PropertyChanged-Event aus, wenn sich der Wert ändert, um die UI zu aktualisieren.
        /// </summary>
        public string Status
        {
            // Gibt den aktuellen Status zurück
            get => _status;
            set
            {
                // Nur wenn sich der Wert tatsächlich ändert, wird das Event ausgelöst
                // Dies verhindert unnötige UI-Updates
                if(_status != value)
                {
                    _status = value;
                    // PropertyChanged-Event wird ausgelöst, damit die UI über die Änderung benachrichtigt wird
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
                }
            }
        }

        /// <summary>
        /// Event, das ausgelöst wird, wenn sich eine Property ändert.
        /// Wird für WPF Data Binding verwendet, um die UI automatisch zu aktualisieren.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
