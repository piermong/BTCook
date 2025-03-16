using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BTCook;
using MySql.Data.MySqlClient;

namespace BTCook
{
    class Station
    {
        public string ID { get; set; }
        public string Nom { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Commune { get; set; }
        public List<string> Lignes { get; set; }


        public Station(string id, string nom, double lat, double lon, string commune)
        {
            ID = id;
            Nom = nom;
            Latitude = lat;
            Longitude = lon;
            Commune = commune;
            Lignes = new List<string> ();
        }

        public void ChargerLignes(MySqlConnection conn)
        {
            try
            {
                string query = "SELECT IDligne FROM Passe_par WHERE Nom = @Nom";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nom", Nom);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string ligne = reader.GetString("IDligne");
                            if (!Lignes.Contains(ligne)) // Évite les doublons
                            {
                                Lignes.Add(ligne);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des lignes pour la station {Nom} : {ex.Message}");
            }
        }
    }
}
