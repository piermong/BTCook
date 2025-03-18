using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Diagnostics;

namespace BTCook
{
    public class Carte
    {
        public List<Station> Stations { get; private set; }
        public List<Arc> Arcs { get; private set; }

        public Carte()
        {
            Stations = new List<Station>();
            Arcs = new List<Arc>();
        }

        public void ConstruireGraphe(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                string line;
                bool firstLine = true;
                List<string[]> lignes = new List<string[]>();

                // Lire toutes les lignes et stocker les données
                while ((line = reader.ReadLine()) != null)
                {
                    if (firstLine)
                    {
                        firstLine = false;
                        continue; // Ignorer l'en-tête
                    }
                    lignes.Add(line.Split(';'));
                }

                // Ajouter toutes les stations
                foreach (var values in lignes)
                {
                    if (values.Length < 12) continue;

                    string idStation = values[0].Trim();
                    string nomStation = values[2].Trim();
                    double longitude = double.Parse(values[3], CultureInfo.InvariantCulture); // permet de convertir les . en , dans le tableau pour convertir en nb
                    double latitude = double.Parse(values[4], CultureInfo.InvariantCulture);
                    string commune = values[6].Trim();
                    double tempsChangement = double.Parse(values[11], CultureInfo.InvariantCulture);

                    // Vérifier si la station existe déjà dans la liste
                    if (!Stations.Any(s => s.IDstation == idStation))
                    {
                        Stations.Add(new Station(nomStation, idStation, latitude, longitude, commune, tempsChangement));
                    }
                }


                // Ajouter les arcs et afficher les connexions
                foreach (var values in lignes)
                {
                    if (values.Length < 12) continue;

                    string idStation = values[0].Trim();
                    double temps = double.Parse(values[10], CultureInfo.InvariantCulture);
                    string[] idPrecedents = values[8].Split(',');
                    string[] idSuivants = values[9].Split(',');

                    Station stationActuelle = null;
                    foreach (Station s in Stations)
                    {
                        if (s.IDstation == idStation)
                        {
                            stationActuelle = s;
                            break;
                        }
                    }
                    if (stationActuelle == null) continue;

                    bool aUnPrecedent = false;

                    // Vérifier si la station a un précédent
                    foreach (var idPrec in idPrecedents)
                    {
                        string idTrim = idPrec.Trim();
                        if (idTrim != "0")
                        {
                            Station stationPrecedente = null;
                            foreach (Station s in Stations)
                            {
                                if (s.IDstation == idTrim)
                                {
                                    stationPrecedente = s;
                                    break;
                                }
                            }
                            if (stationPrecedente != null)
                            {
                                Arcs.Add(new Arc(stationActuelle, stationPrecedente, temps));
                                Console.WriteLine($"{idTrim} <-> {idStation}");
                                aUnPrecedent = true;
                            }
                        }
                    }

                    // Si elle n'a pas de précédent, utiliser la simple flèche
                    foreach (var idSuiv in idSuivants)
                    {
                        string idTrim = idSuiv.Trim();
                        if (idTrim != "0")
                        {
                            Station stationSuivante = null;
                            foreach (Station s in Stations)
                            {
                                if (s.IDstation == idTrim)
                                {
                                    stationSuivante = s;
                                    break;
                                }
                            }
                            if (stationSuivante != null)
                            {
                                Arcs.Add(new Arc(stationActuelle, stationSuivante, temps));
                                if (!aUnPrecedent) // Éviter les doublons d'affichage
                                {
                                    Console.WriteLine($"{idStation} -> {idTrim}");
                                }
                            }
                        }
                    }

                }
            }            
        }

        public void AfficherArcs()
        {
            Console.WriteLine("Liste des arcs du graphe :");
            foreach (var arc in Arcs)
            {
                Console.WriteLine($"{arc.Depart.IDstation} -> {arc.Arrivee.IDstation}");
            }
        }


        public void AlgoDijkstra(Station départ, Station arrivée)
        {
            
        }
    }
}
