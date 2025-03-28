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
                                    break; // On arrête la boucle dès qu'on trouve la station
                                }
                            }

                            // Trouver la ligne commune
                            if (!string.IsNullOrEmpty(ligne))
                            {
                                // Créer l'arc avec la ligne commune
                                Arc nouvelArc = new Arc(stationActuelle, stationSuivante, temps, ligne);
                                Arcs.Add(nouvelArc);
                            }
                        }
                    }
                }

            }         
        }
        
        public void AfficherArcs()
        {
            foreach (Arc a in Arcs)
            {
                Console.WriteLine(a.Depart.Nom +" -> "+a.Arrivee.Nom);
            }
        }

        /// <summary>
        /// retrouve 2 stations dans la liste Stations à partir du nom de la station
        /// </summary>
        /// <param name="departNom"></param>
        /// <param name="arriveeNom"></param>
        /// <returns></returns>
        public (Station , Station) RetrouveStation(string departNom, string arriveeNom)
        {
            if (departNom == "" || arriveeNom == ""|| departNom == arriveeNom)
            {
                Console.WriteLine("Erreur de saisi !");
                return (null, null);
            }
            
            // Recherche des stations de départ et d'arrivée à partir de leur nom
            Station depart = null;
            Station arrivee = null;
            
            foreach (Station s in Stations)
            {
                if (s.Nom == departNom)
                    depart = s;
                if (s.Nom == arriveeNom)
                    arrivee = s;
            }                   
            return(depart, arrivee);
        }


        /// <summary>
        /// l'algorithme de Dijkstra est composé d'une liste des stations non-visité qui doit être vide à la fin
        /// une fois la station validé on remplit son précédent et on retrouve le chemin du point de départ au point d'arrivé 
        /// à partir des précédents
        /// </summary>
        /// <param name="departNom"></param>
        /// <param name="arriveeNom"></param>
        /// <returns></returns>
        public List<Station> Dijkstra(string departNom, string arriveeNom)
        {
            // Recherche des stations par leur nom
            (Station depart, Station arrivee) = RetrouveStation(departNom, arriveeNom);

            if (depart == null || arrivee == null)
                return null;

            // 1. Initialisation
            List<Station> stationNonVisitees = new List<Station>(Stations);

            // Initialiser toutes les stations
            foreach (Station station in stationNonVisitees)
            {
                station.Poids = double.MaxValue;  
                station.StationPrécé = null;      
            }
            depart.Poids = 0;
            depart.tempsChangement = 0; // Pas de temps de changement pour la station de départ

            // Continuer jusqu'à ce que toutes les stations soient traitées
            while (stationNonVisitees.Count > 0)
            {
                // 2. Sélection de la station courante
                Station stationCourante = stationNonVisitees[0];
                for (int i = 1; i < stationNonVisitees.Count; i++)
                {
                    if (stationNonVisitees[i].Poids < stationCourante.Poids)
                    {
                        stationCourante = stationNonVisitees[i];
                    }
                }

                // 3. Si on a atteint la destination, arrêter
                if (stationCourante == arrivee)
                    break;

                // 4. Retirer la station courante déjà visitées
                stationNonVisitees.Remove(stationCourante);

                // 5. Recherche des arcs adjacents
                List<Arc> arcsAdjacents = new List<Arc>();
                for (int i = 0; i < Arcs.Count; i++)
                {
                    if (Arcs[i].Depart == stationCourante)
                    {
                        arcsAdjacents.Add(Arcs[i]);
                    }
                }

                foreach (Arc arc in arcsAdjacents)
                {
                    Station stationVoisine = arc.Arrivee;

                    // Ignorer si la station a déjà été traitée
                    if (!stationNonVisitees.Contains(stationVoisine))
                        continue;

                    // Calcul du nouveau poids
                    double nouveauPoids = stationCourante.Poids + arc.Temps;

                    // Vérification du changement de ligne
                    bool changementLigne = (stationCourante.listeLignes[0] != arc.Ligne);

                    if (changementLigne)
                    {
                        nouveauPoids += stationCourante.tempsChangement;
                    }

                    // 6. Mettre à jour la distance si un chemin plus court est trouvé
                    if (nouveauPoids < stationVoisine.Poids)
                    {
                        stationVoisine.Poids = nouveauPoids;
                        stationVoisine.StationPrécé = stationCourante;
                    }
                }
            }

            // 7. Reconstruire le chemin optimal
            List<Station> cheminOptimal = new List<Station>();
            Station stationActuelle = arrivee;

            // Remonter de la destination au départ
            while (stationActuelle != null)
            {
                cheminOptimal.Insert(0, stationActuelle);
                stationActuelle = stationActuelle.StationPrécé;
            }

            return cheminOptimal;
        }


        public List<Station> BellmanFord(string departId, string arriveeId)
        {
            (Station depart, Station arrivee) = RetrouveStation(departId, arriveeId);
            if (depart == null || arrivee == null)
                return null;

            // 1. Initialisation
            foreach (Station s in Stations)
            {
                s.Poids = double.MaxValue;
                s.StationPrécé = null;
            }
            depart.Poids = 0;
            depart.tempsChangement = 0; // Pas de temps de changement pour la station de départ


            // 2. Relaxation des arcs et on fait n-1 passages
            int nombreStations = Stations.Count;
            for (int iteration = 1; iteration < nombreStations; iteration++)
            {
                bool modificationEffectuee = false;
                string ligneActuelle = Arcs[0].Ligne;
                // Parcourir tous les arcs
                foreach (Arc arc in Arcs)
                {
                    Station stationDepart = arc.Depart;
                    Station stationArrivee = arc.Arrivee;

                    // Vérifier si le chemin actuel peut être amélioré
                    if (stationDepart.Poids != double.MaxValue)
                    {
                        // Calcul du nouveau poids
                        double nouveauPoids = stationDepart.Poids + arc.Temps;

                        // Vérification du changement de ligne
                        bool changementLigne = (stationDepart.listeLignes[0] != arc.Ligne);                                             
                        if (changementLigne)
                        {
                            nouveauPoids += stationDepart.tempsChangement;
                        }
                        // Relaxation de l'arc
                        if (nouveauPoids < stationArrivee.Poids)
                        {
                            stationArrivee.Poids = nouveauPoids;
                            stationArrivee.StationPrécé = stationDepart;
                            modificationEffectuee = true;
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


        public string RechercheLigne(Station depart, Station arrivee)
        {
            foreach(Arc a in Arcs)
            {
                if (a.Depart == depart && a.Arrivee == arrivee)
                {
                    return a.Ligne;
                }
            }
            return "";
        }

        public void AfficherCheminOptimal(List<Station> cheminOptimal)
        {

            if (cheminOptimal == null || cheminOptimal.Count <2)
            {
                Console.WriteLine("Aucun chemin trouvé entre ces stations.");
                return;
            }
            else{
                // Afficher les informations sur le chemin
                double tempsTotal = cheminOptimal[cheminOptimal.Count - 1].Poids;
                Console.WriteLine($"Chemin optimal de {cheminOptimal[0].Nom} à {cheminOptimal[cheminOptimal.Count - 1].Nom}");
                Console.WriteLine($"Temps total : {tempsTotal} minutes");
                Console.WriteLine($"Nombre de stations : {cheminOptimal.Count}");
                Console.WriteLine("----------------------------------------------------");

                // Tracer le chemin station par station avec les informations sur les changements de ligne
                string ligneCourante = RechercheLigne(cheminOptimal[0], cheminOptimal[1]);
                Console.WriteLine($"Départ: {cheminOptimal[0].Nom} (Ligne {ligneCourante})");

                for (int i = 1; i < cheminOptimal.Count-1; i++)
                {
                    Station stationActuelle = cheminOptimal[i];
                    string ligneActuelle = RechercheLigne(cheminOptimal[i], cheminOptimal[i + 1]); ;

                    // Vérifier s'il y a un changement de ligne
                    if (ligneActuelle != ligneCourante)
                    {
                        Console.WriteLine($"→ Changement de ligne à {stationActuelle.Nom}:\n       Ligne {ligneCourante} → Ligne {ligneActuelle} Temps de changement : {stationActuelle.tempsChangement} min");
                        ligneCourante = ligneActuelle;
                    }
                    else
                    {
                        Console.WriteLine($"→ {stationActuelle.Nom} (Ligne {ligneActuelle})");
                    }
                }
                Console.WriteLine($"→ {cheminOptimal[cheminOptimal.Count-1].Nom} (Ligne {ligneCourante})");

                Console.WriteLine("----------------------------------------------------");
            }

        }
    }
}
