﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BTCook;

public class Arc
{
    public Station Depart { get; set; }
    public Station Arrivee { get; set; }
    public string Ligne { get; set; }
    public double Distance { get; set; }
    public double Temps { get; set; }
    public Color CouleurLigne { get; set; } // Ajout de la couleur

    /// <summary>
    /// Initialise une nouvelle instance de la classe Arc.
    /// </summary>
    /// <param name="depart">La station de départ.</param>
    /// <param name="arrivee">La station d'arrivée.</param>
    /// <param name="temps">Le temps de trajet entre les deux stations.</param>
    /// <param name="ligne">La ligne de métro.</param>
    public Arc(Station depart, Station arrivee, double temps, string ligne)
    {
        Depart = depart;
        Arrivee = arrivee;
        Ligne = ligne;
        Color couleurLigne = ObtenirCouleurLigne(Ligne);
        Distance = CalculerDistance(depart, arrivee);
        Temps = temps;
        CouleurLigne = ObtenirCouleurLigne(Ligne);
    }

    /// <summary>
    /// Obtient la couleur associée à une ligne de métro.
    /// </summary>
    /// <param name="ligne">Le numéro de la ligne de métro.</param>
    /// <returns>La couleur de la ligne.</returns>
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

    /// <summary>
    /// Calcule la distance entre deux stations en utilisant la formule de Haversine.
    /// </summary>
    /// <param name="s1">La première station.</param>
    /// <param name="s2">La deuxième station.</param>
    /// <returns>La distance en kilomètres.</returns>
    private double CalculerDistance(Station s1, Station s2)
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
