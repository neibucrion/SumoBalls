using UnityEngine;
using System.Collections.Generic;

namespace ChargeurCSV
{
    public class ChargeurTableaux
    {
        enum TypeTableau {DOUBLE, INT, STRING};
        private List<List<double>> tableDouble;
        private List<List<int>> tableInt;
        private List<List<string>> tableString;
        private TypeTableau typeValeur;

        public ChargeurTableaux()
        {

        }

        public List<List<string>> chargeTableauString(string nomFichier)
        {
            typeValeur = TypeTableau.STRING;
            tableString = new List<List<string>>();
            chargeTableau(nomFichier);
            return tableString;
        }

        public List<List<int>> chargeTableauInt(string nomFichier)
        {
            typeValeur = TypeTableau.INT;
            tableInt = new List<List<int>>();
            chargeTableau(nomFichier);
            return tableInt;
        }

        public List<List<double>> chargeTableauDouble(string nomFichier)
        {
            typeValeur = TypeTableau.DOUBLE;
            tableDouble = new List<List<double>>();
            chargeTableau(nomFichier);
            return tableDouble;
        }

        void chargeTableau(string nomFichier)
        {
            try
            {
                TextAsset texteFichier =  Resources.Load(nomFichier) as TextAsset;
                string texteFinal = texteFichier.text;
                string[] lignes = texteFinal.Split('\n');
                foreach (string ligne in lignes)
                {
                    string lignePropre = nettoieLigne(ligne);                
                    litLigne(lignePropre);
                }
            }
            catch(System.Exception e)
            {
                System.Console.WriteLine("{0}\n", e.Message);
            }        
        }

        string nettoieLigne(string ligneSale)
        {
            string retour = ligneSale;
            string charFinal = ligneSale.Substring(ligneSale.Length - 1, 1);
            if (charFinal == "\n" || charFinal == "\r")
            {
                retour = ligneSale.Substring(0, ligneSale.Length - 1);
            }
            return retour;
        }

        void litLigne(string ligne)
        {
            string[] entries = ligne.Split(',');
            if (entries.Length > 0)
            {
                if (typeValeur == TypeTableau.INT)
                {
                    enregistreLigneTableauInt(entries);
                }
                else if (typeValeur == TypeTableau.STRING)
                {
                    enregistreLigneTableauStr(entries);
                }
                else
                {
                    enregistreLigneTableauDouble(entries);
                }
            }
        }

        void enregistreLigneTableauStr(string[] valeurs)
        {
            List<string> ligne = new List<string>();
            foreach (string valeur in valeurs)
            {
                ligne.Add(valeur);
            }        
            tableString.Add(ligne);
        }

        void enregistreLigneTableauInt(string[] valeurs)
        {
            List<int> ligne = new List<int>();
            foreach (string valeur in valeurs)
            {
                ligne.Add(int.Parse(valeur));
            }
            tableInt.Add(ligne);
        }

        void enregistreLigneTableauDouble(string[] valeurs)
        {
            List<double> ligne = new List<double>();
            foreach (string valeur in valeurs)
            {
                ligne.Add(double.Parse(valeur));
            }
            tableDouble.Add(ligne);
        }
    }


}


