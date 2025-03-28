using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Forms;

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

        /// <summary>
        /// on remplit la liste des arcs et des staions en lisant le fichier csv et ses colonnes
        /// </summary>
        /// <param name="filePath"></param>
        public void ConstruireGraphe(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                string line;
                bool firstLine = true;
                List<string[]> lines = new List<string[]>();

                // Lire toutes les lignes et stocker les données
                while ((line = reader.ReadLine()) != null)
                {
                    if (firstLine)
                    {
                        firstLine = false;
                        continue; // Ignorer l'en-tête
                    }
                    lines.Add(line.Split(';'));
                }

                // Dictionnaire pour stocker les stations uniques
                Dictionary<string, Station> stationsUniques = new Dictionary<string, Station>();

                // Ajouter toutes les stations et leurs lignes
                foreach (var values in lines)
                {
                    if (values.Length < 11) continue;

                    string nomStation = values[2].Trim();
                    string ligne = values[1].Trim(); // Numéro de ligne
                    double longitude = double.Parse(values[3], CultureInfo.InvariantCulture);
                    double latitude = double.Parse(values[4], CultureInfo.InvariantCulture);
                    string commune = values[5];
                    double tempsChangement = double.Parse(values[10], CultureInfo.InvariantCulture);

                    // faire la liste des lignes d'une station
                    // Chercher si la station existe déjà
                    Station stationExistante = null;
                    foreach (var station in Stations)
                    {
                        if (station.Nom == nomStation)
                        {
                            stationExistante = station;
                            break;
                        }
                    }

                    if (stationExistante == null)
                    {
                        // Créer une nouvelle station si elle n'existe pas
                        Station nouvelleStation = new Station(nomStation, latitude, longitude, commune, tempsChangement);
                        nouvelleStation.AjouterLigne(ligne);
                        Stations.Add(nouvelleStation);
                    }
                    else
                    {
                        // Ajouter la ligne à la station existante
                        stationExistante.AjouterLigne(ligne);
                    }
                }

                // Ajouter les arcs et afficher les connexions
                foreach (var values in lines)
                {
                    if (values.Length < 11) continue;

                    string nomStation = values[2].Trim();
                    string ligne = values[1]; // Numéro de ligne
                    string[] nomStationPrecedente = values[7].Split(',');
                    string[] nomStationSuivante = values[8].Split(',');
                    double temps = double.Parse(values[9], CultureInfo.InvariantCulture);

                    Station stationActuelle = null;
                    foreach (Station s in Stations)
                    {
                        if (s.Nom == nomStation)
                        {
                            stationActuelle = s;
                            break; // Arrête la boucle dès qu'on trouve la station
                        }
                    }
                    if (stationActuelle == null) continue;


                    foreach (var nomPrece in nomStationPrecedente)
                    {
                        string idTrim = nomPrece.Trim();
                        if (idTrim != "-")
                        {
                            // Trouver la station précédente
                            Station stationPrecedente = null;
                            foreach (Station s in Stations)
                            {
                                if (s.Nom == idTrim)
                                {
                                    stationPrecedente = s;
                                    break; 
                                }
                            }

                            // Trouver la ligne commune
                            if (!string.IsNullOrEmpty(ligne))
                            {
                                // Créer l'arc avec la ligne commune
                                Arc nouvelArc = new Arc(stationActuelle, stationPrecedente, temps, ligne);
                                Arcs.Add(nouvelArc);
                            }
                        }
                    }

                    // Si elle n'a pas de précédent, utiliser la simple flèche
                    foreach (var nomSuiv in nomStationSuivante)
                    {
                        string idTrim = nomSuiv.Trim();
                        if (idTrim != "-")
                        {
                            // Trouver la station suivante
                            Station stationSuivante = null;
                            foreach (Station s in Stations)
                            {
                                if (s.Nom == idTrim)
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

                // Optimisation : si aucune modification, on peut s'arrêter
                if (!modificationEffectuee)
                    break;
            }
            // 3. Reconstruction du chemin
            List<Station> cheminOptimal = new List<Station>();
            Station stationCourante = arrivee;

            while (stationCourante != null)
            {
                cheminOptimal.Insert(0, stationCourante);
                stationCourante = stationCourante.StationPrécé;
            }
            return cheminOptimal;
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
