using dm.KAE.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace dm.KAE.Data.ViewModels
{
    public class AllInfo
    {
        public Stat Stat { get; set; }
        public Price Price { get; set; }
        public int Holders { get; set; }

        public bool IsOutOfSync()
        {
            var oosStat = Stat.Date.AddMinutes(30) <= DateTime.UtcNow;
            var oosPrice = Price.Date.AddMinutes(30) <= DateTime.UtcNow;
            return (oosStat || oosPrice);
        }
    }
}
