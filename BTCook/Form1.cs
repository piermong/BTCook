using System.Data;
using System.Windows.Forms;
using BTCook;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace BTCook
{
    public partial class Form1 : Form
    {
        private Carte carte;
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        private float zoomFactor = 1.0f; // Facteur de zoom
        private const float zoomStep = 0.1f; // Incrément de zoom
        private PointF zoomCenter; // Centre du zoom

        /// <summary>
        /// Constructeur de la classe Form1.
        /// Initialise les composants et construit le graphe à partir d'un fichier CSV.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            if (!Debugger.IsAttached)
            {
                AllocConsole(); // Alloue une console si elle n'est pas déjà attachée
            }

            carte = new Carte();
            string nomFichier = "MetroParisModif.csv"; // permet d'ouvrir le fichier quelque soit l'endroit ou j'enregistre mon code
            string cheminFichier = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nomFichier); //donne le chemin du dossier d'exécution (bin\Debug\net8.0-windows\).
            carte.ConstruireGraphe(cheminFichier);

            string[] lignes = File.ReadAllLines(cheminFichier, Encoding.UTF8);
            this.Paint += DessinerGraphe; // Attacher l'événement Paint

            // Ajout des gestionnaires d'événements pour le zoom avec la molette
            this.MouseWheel += Form1_MouseWheel;

            this.WindowState = FormWindowState.Maximized; // Maximiser la fenêtre
            zoomCenter = new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2); // Initialiser le centre du zoom

            Console.WriteLine($"Nombre de stations : {carte.Stations.Count}");
            Console.WriteLine($"Nombre d'arcs : {carte.Arcs.Count}");
            List<Station> cheminOptimal1 = carte.Dijkstra("Porte Maillot", "Nationale");
            List<Station> cheminOptimal2 = carte.BellmanFord("Porte Maillot", "Nationale");
            if (cheminOptimal1[^1].Poids <= cheminOptimal2[^1].Poids)
            {
                Console.WriteLine("\n\n");
                Console.WriteLine("Le chemin optimal est celui trouvé par Dijkstra");
                Console.WriteLine("     Dijkstra : ");
                carte.AfficherCheminOptimal(carte.Dijkstra("Porte Maillot", "Nationale"));
            }
            else
            {
                Console.WriteLine("\n\n");
                Console.WriteLine("Le chemin optimal est celui trouvé par Bellman Ford");
                Console.WriteLine("     Bellman Ford : ");
                carte.AfficherCheminOptimal(carte.BellmanFord("Porte Maillot", "Nationale"));
            }
        }

        /// <summary>
        /// Gère l'événement de la molette de souris pour le zoom.
        /// </summary>
        /// <param name="sender">L'objet source de l'événement.</param>
        /// <param name="e">Les données de l'événement de la molette.</param>
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            // Mettre à jour le centre du zoom à la position de la souris
            zoomCenter = new PointF(e.X, e.Y);

            // Modifier le facteur de zoom en fonction de la direction de la molette
            if (e.Delta > 0)
            {
                // Zoom avant
                zoomFactor += zoomStep;
                if (zoomFactor > 5.0f) zoomFactor = 5.0f; // Limiter le zoom maximum
            }
            else
            {
                // Zoom arrière
                zoomFactor -= zoomStep;
                if (zoomFactor < 0.2f) zoomFactor = 0.2f; // Limiter le zoom minimum
            }

            // Redessiner le graphe avec le nouveau facteur de zoom
            this.Invalidate(); // Force le redessinage du formulaire
        }

        /// <summary>
        /// Dessine le graphe des stations et des arcs sur le formulaire.
        /// </summary>
        /// <param name="sender">L'objet source de l'événement.</param>
        /// <param name="e">Les données de l'événement de dessin.</param>
        private void DessinerGraphe(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            // Appliquer le zoom et le centrage
            g.TranslateTransform(zoomCenter.X, zoomCenter.Y); // Déplacer l'origine au centre du zoom
            g.ScaleTransform(zoomFactor, zoomFactor); // Appliquer le facteur de zoom
            g.TranslateTransform(-zoomCenter.X, -zoomCenter.Y); // Remettre l'origine à sa position initiale

            // Activer l'antialiasing pour un rendu plus lisse
            g.SmoothingMode = SmoothingMode.AntiAlias;

            double minLat = double.MaxValue; // en mettant à 1000 minlat ne peut que diminuer
            double maxLat = 0; // en mettant à 0 maxlat ne peut qu'auguementer 
            double minLon = double.MaxValue;
            double maxLon = 0;

            foreach (var station in carte.Stations)
            {
                // Mise à jour des valeurs minimales et maximales de latitude
                if (station.Latitude < minLat) minLat = station.Latitude;
                if (station.Latitude > maxLat) maxLat = station.Latitude;
                // Mise à jour des valeurs minimales et maximales de longitude
                if (station.Longitude < minLon) minLon = station.Longitude;
                if (station.Longitude > maxLon) maxLon = station.Longitude;
            }

            // Définition des marges et des dimensions de la zone de dessin
            int marge = 50;
            int largeurEcran = this.ClientSize.Width - 2 * marge;
            int hauteurEcran = this.ClientSize.Height - 2 * marge;
            int rayonSommet = 10; // Rayon des sommets

            Dictionary<string, PointF> positions = new Dictionary<string, PointF>();

            foreach (var station in carte.Stations)
            {
                // Calcul de la position X en fonction de la longitude
                station.X = (float)((station.Longitude - minLon) / (maxLon - minLon) * largeurEcran) + marge;
                // Calcul de la position Y en fonction de la latitude
                station.Y = (float)((1 - (station.Latitude - minLat) / (maxLat - minLat)) * hauteurEcran) + marge;

                // Ajouter la position de la station dans le dictionnaire
                positions[station.Nom] = new PointF(station.X, station.Y);
            }

            // Dessiner les arcs
            foreach (var arc in carte.Arcs)
            {
                if (positions.TryGetValue(arc.Depart.Nom, out PointF depart) && positions.TryGetValue(arc.Arrivee.Nom, out PointF arrivee))
                {
                    // Calculer un nouveau point d'arrivée avant d'atteindre le sommet
                    PointF arriveeAjustee = ReculerPoint(arrivee, depart, rayonSommet);

                    // Dessiner la ligne de l'arc
                    Pen pen = new Pen(arc.CouleurLigne, 3);
                    g.DrawLine(pen, depart, arriveeAjustee);
                    DessinerFleche(g, pen, depart, arriveeAjustee);
                }
                else
                {
                    // Gérer le cas où la clé est manquante
                    Console.WriteLine($"Clé manquante pour l'arc: {arc.Depart.Nom} -> {arc.Arrivee.Nom}");
                }
            }

            // Dessiner les sommets
            foreach (var station in carte.Stations)
            {
                PointF position = new PointF(station.X, station.Y);
                Brush brush = Brushes.Gray;
                g.FillEllipse(brush, position.X - rayonSommet, position.Y - rayonSommet, rayonSommet * 2, rayonSommet * 2);

                string nomAffiche = FormaterNomStation(station.Nom);
                Font font = new Font("Arial", 8);
                SizeF textSize = g.MeasureString(nomAffiche, font);
                PointF textPosition = new PointF(position.X - textSize.Width / 2, position.Y - textSize.Height / 2);
                g.DrawString(nomAffiche, font, Brushes.Black, textPosition);
            }
        }

        /// <summary>
        /// Formate le nom d'une station pour l'affichage.
        /// </summary>
        /// <param name="nom">Le nom de la station.</param>
        /// <returns>Le nom formaté de la station.</returns>
        private string FormaterNomStation(string nom)
        {
            if (nom.StartsWith("Château de "))
            {
                string resteNom = nom.Substring(11); // Enlève "Château de "
                return "Cha " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Porte de "))
            {
                string resteNom = nom.Substring(9); // Enlève "Porte de "
                return "Prt " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Porte d'"))
            {
                string resteNom = nom.Substring(8); // Enlève "Porte d'"
                return "Prt " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Porte des "))
            {
                string resteNom = nom.Substring(10); // Enlève "Porte des "
                return "Prt " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Porte "))
            {
                string resteNom = nom.Substring(6); // Enlève "Porte "
                return "Prt " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Pont "))
            {
                string resteNom = nom.Substring(5); // Enlève "Pont "
                return "Pnt " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Place de "))
            {
                string resteNom = nom.Substring(9); // Enlève "Place de "
                return "Plc " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Place des "))
            {
                string resteNom = nom.Substring(10); // Enlève "Place des "
                return "Plc " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Place d'"))
            {
                string resteNom = nom.Substring(8); // Enlève "Place d'"
                return "Plc " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Place "))
            {
                string resteNom = nom.Substring(6); // Enlève "Place "
                return "Plc " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Rue des "))
            {
                string resteNom = nom.Substring(8); // Enlève "Rue des "
                return "R " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Rue du "))
            {
                string resteNom = nom.Substring(7); // Enlève "Rue du "
                return "R " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Rue de la "))
            {
                string resteNom = nom.Substring(10); // Enlève "Rue de la "
                return "R " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Rue "))
            {
                string resteNom = nom.Substring(4); // Enlève "Rue "
                return "R " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Gare de "))
            {
                string resteNom = nom.Substring(8); // Enlève "Gare de "
                return "Gare " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Gare du "))
            {
                string resteNom = nom.Substring(8); // Enlève "Gare du "
                return "Gare " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Les "))
            {
                string resteNom = nom.Substring(4); // Enlève "Les "
                return "Les " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("La "))
            {
                string resteNom = nom.Substring(3); // Enlève "La "
                return "La " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Le "))
            {
                string resteNom = nom.Substring(3); // Enlève "Le "
                return "Le " + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            if (nom.StartsWith("Saint-"))
            {
                string resteNom = nom.Substring(6); // Enlève "Saint-"
                return "St-" + resteNom.Substring(0, Math.Min(7, resteNom.Length));
            }
            return nom.Substring(0, Math.Min(7, nom.Length));
        }



        /// <summary>
        /// Dessine une flèche entre deux points.
        /// </summary>
        /// <param name="g">L'objet Graphics utilisé pour dessiner.</param>
        /// <param name="pen">Le stylo utilisé pour dessiner la flèche.</param>
        /// <param name="depart">Le point de départ de la flèche.</param>
        /// <param name="arrivee">Le point d'arrivée de la flèche.</param>
        private void DessinerFleche(Graphics g, Pen pen, PointF depart, PointF arrivee)
        {
            double angle = Math.Atan2(arrivee.Y - depart.Y, arrivee.X - depart.X);
            double flecheSize = 10;

            PointF p1 = new PointF(
                (float)(arrivee.X - flecheSize * Math.Cos(angle - Math.PI / 6)),
                (float)(arrivee.Y - flecheSize * Math.Sin(angle - Math.PI / 6))
            );

            PointF p2 = new PointF(
                (float)(arrivee.X - flecheSize * Math.Cos(angle + Math.PI / 6)),
                (float)(arrivee.Y - flecheSize * Math.Sin(angle + Math.PI / 6))
            );

            g.DrawLine(pen, arrivee, p1);
            g.DrawLine(pen, arrivee, p2);
        }

        /// <summary>
        /// Ajuste le point d'arrivée d'un arc pour dessiner une flèche.
        /// </summary>
        /// <param name="arrivee">Le point d'arrivée initial.</param>
        /// <param name="depart">Le point de départ.</param>
        /// <param name="distance">La distance à reculer.</param>
        /// <returns>Le nouveau point d'arrivée ajusté.</returns>
        private PointF ReculerPoint(PointF arrivee, PointF depart, float distance)
        {
            double angle = Math.Atan2(arrivee.Y - depart.Y, arrivee.X - depart.X);
            return new PointF(
                (float)(arrivee.X - distance * Math.Cos(angle)),
                (float)(arrivee.Y - distance * Math.Sin(angle))
            );
        }
    }
}