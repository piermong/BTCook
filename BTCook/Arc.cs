using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BTCook;
using MySql.Data.MySqlClient;

namespace BTCook
{
        class Arc
        {
        public Station Depart { get; set; }
        public Station Arrivee { get; set; }
        public string Ligne { get; set; }
        public double Distance { get; } // Distance en kilomètres

        public Arc(Station depart, Station arrivee, string ligne)
        {
            Depart = depart;
            Arrivee = arrivee;
            Ligne = ligne;
            Distance = CalculerDistance(depart.Latitude, depart.Longitude, arrivee.Latitude, arrivee.Longitude);
        }

        private double CalculerDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Rayon de la Terre en km

            // Conversion des degrés en radians
            double phi1 = lat1 * Math.PI / 180;
            double phi2 = lat2 * Math.PI / 180;
            double deltaPhi = (lat2 - lat1) * Math.PI / 180;
            double deltaLambda = (lon2 - lon1) * Math.PI / 180;

            // Formule de Haversine
            double a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                       Math.Cos(phi1) * Math.Cos(phi2) *
                       Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Distance en kilomètres
        }
    }
}
