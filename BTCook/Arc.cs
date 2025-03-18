using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BTCook;

public class Arc<T>
{
    public Station<T> Depart { get; set; }
    public Station<T> Arrivee { get; set; }
    public string Ligne { get; set; }
    public double Distance { get; set; }
    public double Temps { get; set; }
    public Color CouleurLigne { get; set; } // Ajout de la couleur


    public Arc(Station<T> depart, Station<T> arrivee, double temps)
    {
        Depart = depart;
        Arrivee = arrivee;
        string ligne = "";
        if (depart.IDstation.Count() == 4) ligne = depart.IDstation.Substring(0, 2);
        else if (depart.IDstation.Contains("bis")) ligne = depart.IDstation[0] + "bis";
        else ligne = depart.IDstation[0].ToString();
        Color couleurLigne = ObtenirCouleurLigne(ligne);
        Ligne = ligne;
        Distance = CalculerDistance(depart, arrivee);
        Temps = temps;
        CouleurLigne = ObtenirCouleurLigne(ligne);
    }


    public static Color ObtenirCouleurLigne(string ligne)
    {
        switch (ligne)
        {
            case "1": return Color.Yellow;
            case "2": return Color.Blue;
            case "3": return Color.Olive;
            case "3bis": return Color.SkyBlue;
            case "4": return Color.DarkMagenta;
            case "5": return Color.OrangeRed;
            case "6": return Color.PaleGreen;
            case "7": return Color.Pink;
            case "7bis": return Color.Chartreuse;
            case "8": return Color.Violet;
            case "9": return Color.YellowGreen;
            case "10": return Color.Orange;
            case "11": return Color.SaddleBrown;
            case "12": return Color.DarkGreen;
            case "13": return Color.DeepSkyBlue;
            case "14": return Color.MediumPurple;
            default: return Color.Gray; // Couleur par défaut si la ligne est inconnue
        }
    }



    private double CalculerDistance(Station<T> s1, Station<T> s2)
    {
        const double R = 6371; // Rayon de la Terre en km

        double lat1 = s1.Latitude * Math.PI / 180;
        double lon1 = s1.Longitude * Math.PI / 180;
        double lat2 = s2.Latitude * Math.PI / 180;
        double lon2 = s2.Longitude * Math.PI / 180;

        double deltaLat = lat2 - lat1;
        double deltaLon = lon2 - lon1;

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // Distance en kilomètres

    }

}
