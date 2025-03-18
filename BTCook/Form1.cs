
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
        }

        private void DessinerGraphe(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            // Normalisation des coordonnées pour afficher le graphe correctement
            double minLat = double.MaxValue, maxLat = double.MinValue;
            double minLon = double.MaxValue, maxLon = double.MinValue;

            foreach (var station in carte.Stations)
            {
                if (station.Latitude < minLat) minLat = station.Latitude;
                if (station.Latitude > maxLat) maxLat = station.Latitude;
                if (station.Longitude < minLon) minLon = station.Longitude;
                if (station.Longitude > maxLon) maxLon = station.Longitude;
            }


            int marge = 50;
            int largeurEcran = this.ClientSize.Width - 2 * marge;
            int hauteurEcran = this.ClientSize.Height - 2 * marge;
            int rayonSommet = 10; // Rayon des sommets

            Dictionary<string, PointF> positions = new Dictionary<string, PointF>();

            foreach (var station in carte.Stations)
            {
                float x = (float)((station.Longitude - minLon) / (maxLon - minLon) * largeurEcran) + marge;
                float y = (float)((1 - (station.Latitude - minLat) / (maxLat - minLat)) * hauteurEcran) + marge;
                positions[station.IDstation] = new PointF(x, y);
            }
            // Dessiner les arcs
            foreach (var arc in carte.Arcs)
            {
                PointF depart = positions[arc.Depart.IDstation];
                PointF arrivee = positions[arc.Arrivee.IDstation];

                // Calculer un nouveau point d'arrivée avant d'atteindre le sommet
                PointF arriveeAjustee = ReculerPoint(arrivee, depart, rayonSommet);

                Pen pen = new Pen(arc.CouleurLigne, 3);
                g.DrawLine(pen, depart, arriveeAjustee);

                // Dessiner la flèche sur le point ajusté
                DessinerFleche(g, pen, depart, arriveeAjustee);
            }

            /// Dessiner les sommets
            foreach (var station in carte.Stations)
            {
                PointF position = positions[station.IDstation];
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

        // Méthode pour ajuster le point d'arrivée d'un arc
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
