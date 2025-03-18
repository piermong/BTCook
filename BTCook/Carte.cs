using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace BTCook
{
    public class Carte<T>
    {
        public Dictionary<string, Station<T>> Stations { get; private set; }
        public List<Arc<T>> Arcs { get; private set; }

        public Carte()
        {
            Stations = new Dictionary<string, Station<T>>();
            Arcs = new List<Arc<T>>();
        }

        public void ConstruireGraphe(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                string line;
                bool firstLine = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (firstLine)
                    {
                        firstLine = false;
                        continue; // Ignorer l'en-tête
                    }

                    string[] values = line.Split(';'); // Séparateur CSV

                    if (values.Length < 12) continue; // Vérification des colonnes

                    // Extraction des valeurs du CSV
                    string idStation = values[0].Trim();
                    string nomStation = values[2].Trim();
                    double longitude = double.Parse(values[3], CultureInfo.InvariantCulture);
                    double latitude = double.Parse(values[4], CultureInfo.InvariantCulture);
                    string commune = values[6].Trim(); // "Commune nom"
                    string idPrecedent = values[8].Trim(); 
                    string idSuivant = values[9].Trim();
                    double temps = double.Parse(values[10], CultureInfo.InvariantCulture);
                    double tempsChangement = double.Parse(values[11], CultureInfo.InvariantCulture);

                    // Ajout de la station si elle n'existe pas encore
                    if (!Stations.ContainsKey(idStation))
                    {
                        Stations[idStation] = new Station<T>(nomStation, idStation, latitude, longitude, commune, tempsChangement);
                    }

                    // Création des arcs avec les stations précédentes et suivantes
                    if (idPrecedent != "0" && Stations.ContainsKey(idPrecedent))
                    {
                        AjouterArc(Stations[idPrecedent], Stations[idStation], temps);
                    }

                    if (idSuivant != "0" && Stations.ContainsKey(idSuivant))
                    {
                        AjouterArc(Stations[idStation], Stations[idSuivant], temps);
                    }
                }
            }
        }

        private void AjouterArc(Station<T> depart, Station<T> arrivee, double temps)
        {
            Arcs.Add(new Arc<T>(depart, arrivee, temps));
        }
    }
}
