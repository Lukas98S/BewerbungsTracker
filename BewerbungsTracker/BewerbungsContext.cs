using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BewerbungsTracker
{
    public class BewerbungsContext : DbContext
    {
        public DbSet<Bewerbung> Bewerbungen { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=meine_bewerbungen.db");
        }
    }
}
