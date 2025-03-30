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

        /// <summary>
        /// Initialise une nouvelle instance de la classe Station.
        /// </summary>
        /// <param name="nom">Le nom de la station.</param>
        /// <param name="lat">La latitude de la station.</param>
        /// <param name="lon">La longitude de la station.</param>
        /// <param name="commune">La commune de la station.</param>
        /// <param name="tempsChangement">Le temps de changement à la station.</param>
        public Station(string nom, double lat, double lon, string commune, double tempsChangement)
        {
            Nom = nom;
            Latitude = lat;
            Longitude = lon;
            Commune = commune;
            this.tempsChangement = tempsChangement;
            StationPrécé = null;
            Poids = 0;
        }

        /// <summary>
        /// Ajoute une ligne à la liste des lignes de la station.
        /// </summary>
        /// <param name="ligne">Le numéro de la ligne à ajouter.</param>
        public void AjouterLigne(string ligne)
        {
            if (!listeLignes.Contains(ligne))
            {
                listeLignes.Add(ligne);
            }
        }
    }
}
