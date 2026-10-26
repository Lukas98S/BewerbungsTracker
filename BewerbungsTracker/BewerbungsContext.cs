using System;
using System.IO;
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
        public BewerbungsContext()
        {
            Database.EnsureCreated();
        }
        /// <summary>
        /// Konfiguriert die Datenbankverbindung beim Initialisieren des DbContext.
        /// Nutzt SQLite mit der lokalen Datenbankdatei "meine_bewerbungen.db".
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Verbindung zur SQLite-Datenbank wird konfiguriert
            optionsBuilder.UseSqlite($"Data Source={AppPfade.DatenbankDatei}");
        }
    }

    public static class  AppPfade
    {
        public static string Ordner
        {
            get
            {
                string pfad = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BewerbungsTracker");
                Directory.CreateDirectory(pfad); // Erstellt den Ordner, falls er nicht existiert
                return pfad;
            }
        }

        public static string SecretsDatei => Path.Combine(Ordner, "secrets.json");
        public static string DatenbankDatei => Path.Combine(Ordner, "meine_bewerbungen.db");
    }
}
