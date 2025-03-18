using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BTCook;

namespace BTCook
{
    public class Station
    {
        public string Nom { get; set; }
        public string IDstation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Commune { get; set; }
        public double tempsChangement { get; set; }

        public Station(string nom, string iDstation, double lat, double lon, string commune, double tempsChangement)
        {
            Nom = nom;
            Latitude = lat;
            Longitude = lon;
            Commune = commune;
            IDstation = iDstation;
            this.tempsChangement = tempsChangement;
        }


        public string DeterminerLigne()
        {
            string ligne = "";
            if (IDstation.Count() == 4) ligne = IDstation.Substring(0, 2);
            else if (IDstation.Contains("bis")) ligne = IDstation[0] + "bis";
            else ligne = IDstation[0].ToString();
            return ligne;
        }
    }

}
