
using System.Data;
using System.Windows.Forms;
using BTCook;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Diagnostics;

namespace BTCook
{
    public partial class Form1 : Form
    {
        private Carte carte;
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

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

            carte.AfficherArcs();

            string[] lignes = File.ReadAllLines(cheminFichier, Encoding.UTF8);
            this.Paint += DessinerGraphe; // Attacher l'événement Paint

            this.WindowState = FormWindowState.Maximized; // Maximiser la fenêtre
            Console.WriteLine($"Nombre de stations : {carte.Stations.Count}");
            Console.WriteLine($"Nombre d'arcs : {carte.Arcs.Count}");
            Console.WriteLine("\n\n");
            Console.WriteLine("     Dijkstra : ");
            carte.AfficherCheminOptimal(carte.Dijkstra("106", "101"));
            Console.WriteLine("\n\n");
            Console.WriteLine("     Bellman Ford : ");
            carte.AfficherCheminOptimal(carte.BellmanFord("106", "101"));

        }

        private void DessinerGraphe(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            double minLat = double.MaxValue; // en mettant à 1000 minlat ne peut que diminuer
            double maxLat = 0; // en mettant à 0 maxlat ne peut qu'auguementer 
            double minLon = double.MaxValue ;
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

            // Dictionnaire pour stocker les positions calculées de chaque station
            //Dictionary<string, PointF> positions = new Dictionary<string, PointF>();

            // Calcul des positions à l'écran pour chaque station
            foreach (var station in carte.Stations)
            {
                // Calcul de la position X en fonction de la longitude
                station.X = (float)((station.Longitude - minLon) / (maxLon - minLon) * largeurEcran) + marge;
                // Calcul de la position Y en fonction de la latitude
                station.Y = (float)((1 - (station.Latitude - minLat) / (maxLat - minLat)) * hauteurEcran) + marge;
            }

            // Dessiner les arcs
            foreach (var arc in carte.Arcs)
            {
                // Récupérer les positions de départ et d'arrivée
                PointF depart = new PointF(arc.Depart.X, arc.Depart.Y);
                PointF arrivee = new PointF(arc.Arrivee.X, arc.Arrivee.Y);

                // Calculer l'angle de la ligne reliant le départ à l'arrivée
                double angle = Math.Atan2(arrivee.Y - depart.Y, arrivee.X - depart.X);

                // Calculer la nouvelle position d'arrivée en reculant de la distance du rayon du sommet
                PointF arriveeAjustee = new PointF(
                    (float)(arrivee.X - rayonSommet * Math.Cos(angle)),
                    (float)(arrivee.Y - rayonSommet * Math.Sin(angle))
                );

                // Dessiner la ligne de l'arc
                Pen pen = new Pen(arc.CouleurLigne, 3);
                g.DrawLine(pen, depart, arriveeAjustee);
                DessinerFleche(g, pen, depart, arriveeAjustee);
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


        private string FormaterNomStation(string nom)
        {
            if (nom.Contains("Château de "))
            {
                string resteNom = nom.Substring(11); // Enlève "Château de "
                return "Cha " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Porte de "))
            {
                string resteNom = nom.Substring(9); // Enlève "Porte de "
                return "Prt " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Porte d'"))
            {
                string resteNom = nom.Substring(8); // Enlève "Porte de "
                return "Prt " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Porte des "))
            {
                string resteNom = nom.Substring(10); // Enlève "Porte de "
                return "Prt " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Porte "))
            {
                string resteNom = nom.Substring(6); // Enlève "Porte "
                return "Prt " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Pont "))
            {
                string resteNom = nom.Substring(5); // Enlève "Pont "
                return "Pnt " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Place de "))
            {
                string resteNom = nom.Substring(9); // Enlève "Place de "
                return "Plc " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Place des "))
            {
                string resteNom = nom.Substring(10); // Enlève "Place de "
                return "Plc " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Place d'"))
            {
                string resteNom = nom.Substring(8); // Enlève "Place d'"
                return "Plc " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Place "))
            {
                string resteNom = nom.Substring(6); // Enlève "Place "
                return "Plc " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Rue des "))
            {
                string resteNom = nom.Substring(8); // Enlève "Rue des "
                return "R " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Rue du "))
            {
                string resteNom = nom.Substring(7); // Enlève "Rue du "
                return "R " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Rue de la "))
            {
                string resteNom = nom.Substring(10); // Enlève "Rue de la "
                return "R " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Rue "))
            {
                string resteNom = nom.Substring(4); // Enlève "Rue "
                return "R " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Gare de "))
            {
                string resteNom = nom.Substring(8); // Enlève "Gare de "
                return "Gar " + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }
            if (nom.Contains("Saint-"))
            {
                string resteNom = nom.Substring(6); // Enlève "Saint-"
                return "St-" + resteNom.Substring(0, Math.Min(4, resteNom.Length));
            }

            return nom.Substring(0, Math.Min(4, nom.Length));
        }


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



    }
}
