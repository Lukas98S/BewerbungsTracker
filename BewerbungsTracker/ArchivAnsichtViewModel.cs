using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;

namespace BewerbungsTracker
{
    public class ArchivAnsichtViewModel
    {
        public ObservableCollection<Bewerbung> AbgelehnteBewerbungen { get; set; }

        public ArchivAnsichtViewModel()
        {
            AbgelehnteBewerbungen = new ObservableCollection<Bewerbung>();
            LadeAbsagen();
        }

        private void LadeAbsagen()
        {
            using (var db = new BewerbungsContext())
            {
                var absagen = db.Bewerbungen.Where(b => b.Status == "Abgelehnt").ToList();
                foreach (var b in absagen)
                {
                    AbgelehnteBewerbungen.Add(b);
                }
            }
        }

    }
}
