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

        public (Station , Station) RetrouveStation(string departId, string arriveeId)
        {

            // Recherche des stations de départ et d'arrivée à partir de leur ID
            Station depart = null;
            Station arrivee = null;

            foreach (Station s in Stations)
            {
                if (s.IDstation == departId)
                    depart = s;
                if (s.IDstation == arriveeId)
                    arrivee = s;

                // Si on a trouvé les deux stations, on peut sortir de la boucle
                if (depart != null && arrivee != null)
                    break;
            }
            if (depart.Nom == arrivee.Nom)
            {
                Console.WriteLine("Vous avez séléctionné la même station");
                depart = null; arrivee = null;
            }          
            return(depart, arrivee);
        }


        public List<Station> Dijkstra(string departId, string arriveeId)
        {
            List<Station> stationDijkstra = Stations;
            List<Arc> arcDijkstra = Arcs;
            (Station depart, Station arrivee) = RetrouveStation(departId, arriveeId);
            if (depart == null || arrivee == null)
                return null;

            // 1. Initialisation
            foreach (Station s in stationDijkstra)
            {
                s.Poids = double.MaxValue;
                s.StationPrécé = null;
            }

            depart.Poids = 0;
            depart.tempsChangement = 0; // si je part d'une station avec plusieurs ligne je n'ajoute pas le temps de changement 

            // A = ensemble des stations validées
            List<Station> A = new List<Station>();

            // B = ensemble des stations adjacentes à explorer
            List<Station> B = new List<Station>();
            B.Add(depart);

            while (B.Count > 0)
            {
                // 2. Sélectionner la station de poids minimal dans B
                Station stationCourante = B[0];
                foreach (Station s in B)
                {
                    if (s.Poids < stationCourante.Poids)
                    {
                        stationCourante = s;
                    }
                }

                // 3. Retirer stationCourante de B et l'ajouter à A
                B.Remove(stationCourante);
                A.Add(stationCourante);

                // Si on a atteint la destination, on peut s'arrêter
                if (stationCourante.IDstation == arrivee.IDstation)
                {
                    break;
                }

                // 4. Parcourir tous les arcs sortants de stationCourante
                foreach (Arc arc in arcDijkstra)
                {
                    if (arc.Depart.IDstation == stationCourante.IDstation)
                    {
                        Station stationSuivante = arc.Arrivee;

                        // Calculer le nouveau poids avec temps de changement de ligne si nécessaire
                        double nouveauPoids = stationCourante.Poids + arc.Temps;

                        // Ajout du temps de changement si changement de ligne
                        if (stationCourante.DeterminerLigne() != stationSuivante.DeterminerLigne())
                        {
                            nouveauPoids += stationCourante.tempsChangement;
                        }

                        // 5. Mise à jour du poids si meilleur chemin trouvé
                        if (nouveauPoids < stationSuivante.Poids)
                        {
                            stationSuivante.Poids = nouveauPoids;
                            stationSuivante.StationPrécé = stationCourante;

                            // 6. Gestion des stations de même nom mais ID différents
                            // Vérifier s'il existe déjà une station de même nom dans B
                            bool existeDeja = false;
                            foreach (Station s in B)
                            {
                                if (s.Nom == stationSuivante.Nom && s.IDstation != stationSuivante.IDstation)
                                {
                                    // Garder celle de poids minimum
                                    if (stationSuivante.Poids < s.Poids)
                                    {
                                        existeDeja = true;
                                        B.Remove(s);
                                        B.Add(stationSuivante);
                                    }
                                }
                            }
                            // Si la station n'est pas déjà dans B et pas dans A
                            if (!A.Contains(stationSuivante) && !existeDeja)
                            {
                                B.Add(stationSuivante);
                            }
                        }
                    }
                }
            }

            // 7. Reconstruction du chemin
            List<Station> cheminOptimal = new List<Station>();
            Station stationArrivee = arrivee;
            while (stationArrivee != null)
            {
                cheminOptimal.Insert(0, stationArrivee);
                stationArrivee = stationArrivee.StationPrécé;
            }
            return cheminOptimal;
        }


        public List<Station> BellmanFord(string departId, string arriveeId)
        {
            List<Station> stationBellmanFord = Stations;
            List<Arc> arcBellmanFord = Arcs;
            (Station depart, Station arrivee) = RetrouveStation(departId, arriveeId);
            if (depart == null || arrivee == null)
                return null;

            // 1. Initialisation
            foreach (Station s in stationBellmanFord)
            {
                s.Poids = double.MaxValue;
                s.StationPrécé = null;
            }
            depart.Poids = 0;
            depart.tempsChangement = 0; // si je part d'une station avec plusieurs ligne je n'ajoute pas le temps de changement 


            // 2. Relaxation des arcs et on fait n-1 passages
            int nombreStations = stationBellmanFord.Count;
            for (int iteration = 1; iteration < nombreStations; iteration++)
            {
                bool modificationEffectuee = false;

                // Parcourir tous les arcs
                foreach (Arc arc in arcBellmanFord)
                {
                    Station stationDepart = arc.Depart;
                    Station stationArrivee = arc.Arrivee;

                    // Vérifier si le chemin actuel peut être amélioré
                    if (stationDepart.Poids != double.MaxValue)
                    {
                        // Calcul du nouveau poids
                        double nouveauPoids = stationDepart.Poids + arc.Temps;

                        // Ajouter le temps de changement de ligne si nécessaire
                        if (stationDepart.DeterminerLigne() != stationArrivee.DeterminerLigne())
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


        public void AfficherCheminOptimal(List<Station> cheminOptimal)
        {

            if (cheminOptimal == null || cheminOptimal.Count == 0)
            {
                Console.WriteLine("Aucun chemin trouvé entre ces stations.");
                return;
            }

            // Afficher les informations sur le chemin
            double tempsTotal = cheminOptimal[cheminOptimal.Count - 1].Poids;
            Console.WriteLine($"Chemin optimal de {cheminOptimal[0].Nom} à {cheminOptimal[cheminOptimal.Count - 1].Nom}");
            Console.WriteLine($"Temps total : {tempsTotal} minutes");
            Console.WriteLine($"Nombre de stations : {cheminOptimal.Count}");
            Console.WriteLine("----------------------------------------------------");

            // Tracer le chemin station par station avec les informations sur les changements de ligne
            string ligneCourante = cheminOptimal[0].DeterminerLigne();
            Console.WriteLine($"Départ: {cheminOptimal[0].Nom} (Ligne {ligneCourante})");

            for (int i = 1; i < cheminOptimal.Count; i++)
            {
                Station stationActuelle = cheminOptimal[i];
                string ligneActuelle = stationActuelle.DeterminerLigne();

                // Vérifier s'il y a un changement de ligne
                if (ligneActuelle != ligneCourante)
                {
                    Console.WriteLine($"Changement de ligne à {stationActuelle.Nom}: Ligne {ligneCourante} → Ligne {ligneActuelle}");
                    ligneCourante = ligneActuelle;
                }
                else
                {
                    Console.WriteLine($"→ {stationActuelle.Nom} (Ligne {ligneActuelle})");
                }
            }

            Console.WriteLine("----------------------------------------------------");
        }
    }
}
