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
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Commune { get; set; }
        public double tempsChangement { get; set; }
        public float X { get; set; } // Coordonnée sur l'écran
        public float Y { get; set; }
        public double Poids { get; set; } 
        public Station StationPrécé { get; set; }
        public List<string> listeLignes { get; set; } = new List<string>();


        public Station(string nom, double lat, double lon, string commune, double tempsChangement)
        {
            Nom = nom;
            Latitude = lat;
            Longitude = lon;
            Commune = commune;
            this.tempsChangement = tempsChangement;
            StationPrécé = null;
        }


        // Méthode pour ajouter une ligne à la station
        public void AjouterLigne(string ligne)
        {
            if (!listeLignes.Contains(ligne))
            {
                listeLignes.Add(ligne);
            }
        }

    }

}
