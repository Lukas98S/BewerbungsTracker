using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BewerbungsTracker
{
    /// <summary>
    /// Entity Framework Core DbContext für Bewerbungsverwaltung.
    /// Konfiguriert die Datenbankverbindung zu SQLite und definiert die Entitäten.
    /// </summary>
    public class BewerbungsContext : DbContext
    {
        /// <summary>
        /// DbSet für alle Bewerbungen in der Datenbank.
        /// Ermöglicht CRUD-Operationen auf Bewerbungsdatensätze.
        /// </summary>
        public DbSet<Bewerbung> Bewerbungen { get; set; }

        /// <summary>
        /// Konfiguriert die Datenbankverbindung beim Initialisieren des DbContext.
        /// Nutzt SQLite mit der lokalen Datenbankdatei "meine_bewerbungen.db".
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Verbindung zur SQLite-Datenbank wird konfiguriert
            optionsBuilder.UseSqlite("Data Source=meine_bewerbungen.db");
        }
    }
}
