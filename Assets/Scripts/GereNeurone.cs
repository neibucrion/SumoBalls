using UnityEngine;
using NeuralNetwork;
using ChargeurCSV;
using System.Collections.Generic;

public class SouvenirCollision
{
    public double[] entreePrecedente;
    public Vector3 positionJoueur;
    public Vector3 directionJoueur;
    public Vector3 positionIA;
    public Vector3 directionIA;
}

public class SouvenirEntree
{
    public double[] entree;
    public float timecode;
}

public class GereNeurone : MonoBehaviour
{
    public float rayonArene;

    //Neural Network Variables
    public double MinimumError;
    public bool reseauPrecalcule = false;
    public bool trainingFinales = false;
    public bool trainingAttente = false;
    public bool trainingCollisions = false;
    public float dureeMemoire;
    public float attenteMax;
    public float facteurImmobilite;
    public float angleOffensif;

    private const TrainingType TrType = TrainingType.MinimumError;
    private ChargeurTableaux chargeur;

    private static List<DataSet> dataSets;
    [HideInInspector]
    public double[] derniereEntree = { };
    private IAJeu iaEnCours;
    private static NeuralNet net;
    private Verbateur verba;   

    // Use this for initialization
    public void Start()
    {
        net = new NeuralNet(8, 6, 4);
        chargeur = new ChargeurTableaux();
        dataSets = new List<DataSet>();        
        creeVerbateur();
    }

    private void creeVerbateur()
    {
        verba = new Verbateur(net);
        if (reseauPrecalcule)
        {
            chargeReseau();
        }
        else
        {
            entraineReseau();
        }
    }

    void chargeReseau()
    {
        List<List<double>> sauvegarde = chargeur.chargeTableauDouble("sauvegardeReseau");
        verba.chargeReseau(sauvegarde);
    }

    void entraineReseau()
    {
        List<List<double>> donneesDepart = chargeur.chargeTableauDouble("datas");
        foreach (List<double> liste in donneesDepart)
        {
            double[] inputs = { liste[0], liste[1], liste[2], liste[3], liste[4], liste[5], liste[6], liste[7] };
            double[] desired = { liste[8], liste[9], liste[10], liste[11] };
            dataSets.Add(new DataSet(inputs, desired));
            net.Train(dataSets, MinimumError);
        }
    }

    public double[] interrogeReseau( GameObject ia, GameObject adversaire)
    {
        Direction directionAdversaire = adversaire.GetComponent<Direction>();
        double[] posAdversaire = {convertitValeurPosition(adversaire.transform.position.x),
                                  convertitValeurPosition(adversaire.transform.position.z)};
        double[] dirAdversaire = {convertitDecimal(directionAdversaire.direction.x),
                                  convertitDecimal(directionAdversaire.direction.z) };

        Direction directionIA = ia.GetComponent<Direction>();
        double[] posIA = {convertitValeurPosition(ia.transform.position.x),
                          convertitValeurPosition(ia.transform.position.z) };
        double[] dirIA = {convertitDecimal(directionIA.direction.x),
                          convertitDecimal(directionIA.direction.z) };

        double[] inputs = { posAdversaire[0], posAdversaire[1], dirAdversaire[0], dirAdversaire[1], posIA[0], posIA[1], dirIA[0], dirIA[1] };
        derniereEntree = inputs;
        double[] result = net.Compute(derniereEntree);
        return result;
    }    

    private double convertitValeurPosition(float valeur)
    {
        float calcul = convertitDecimal(valeur / (rayonArene * 2.0f));
        calcul = Mathf.Clamp(calcul, 0.0f, 1.0f);
        double valFinale = calcul;
        return valFinale;
    }

    private float convertitDecimal(float valInitiale)
    {
        float valFinale = (valInitiale + 1.0f) / 2.0f;
        return valFinale;
    }

    public void metAJourReseau(bool victoire, IAJeu ia)
    {
        iaEnCours = ia;
        if (trainingFinales)
            apprendDerniereEntree(victoire);
        if (trainingAttente)
            apprendAttentes();
        if (trainingCollisions)
            analyseCollisions();
        iaEnCours.entrees = new List<SouvenirEntree>();
    }

    private void apprendDerniereEntree(bool victoire)
    {
        double[] desired = { 0.1d, 0.1d, 0.1d, 0.9d };
        if (victoire)
        {
            desired[0] = 0.9d;
            desired[3] = 0.1d;
        }
        dataSets.Add(new DataSet(iaEnCours.entrees[0].entree, desired));
        net.Train(dataSets, MinimumError);
    }

    private void apprendAttentes()
    {
        double[] desired = { 0.1d, 0.1d, 0.9d, 0.1d };
        foreach (double[] attente in iaEnCours.attentes)
        {
            dataSets.Add(new DataSet(attente, desired));
            net.Train(dataSets, MinimumError);
        }
        iaEnCours.attentes = new List<double[]>();
    }

    private void analyseCollisions()
    {
        foreach (SouvenirCollision souvenir in iaEnCours.collisions)
        {
            double[] desired = trouveDesir(souvenir);
            dataSets.Add(new DataSet(souvenir.entreePrecedente, desired));
            net.Train(dataSets, MinimumError);
        }
        iaEnCours.collisions = new List<SouvenirCollision>();
    }

    private double[] trouveDesir(SouvenirCollision collision)
    {
        double[] retour = { 0.9d, 0.1d, 0.1d, 0.1d };
        Vector3 difference = collision.positionJoueur - collision.positionIA;
        float angle = Vector3.Angle(collision.directionIA, difference);
        if (Mathf.Abs(angle) > angleOffensif)
        {
            retour[0] = 0.1d;
            retour[1] = 0.9d;
        }
        return retour;
    }

    public void sauvegardeGenerale()
    {
        ecritData();
        sauveReseau();
    }

    public void ecritData()
    {
        try
        {
            string dataToDump = transformeDataSets();
            System.IO.File.WriteAllText("dumpData.csv", dataToDump);
        }
        catch (System.Exception e)
        {
            System.Console.WriteLine("{0}\n", e.Message);
        }
    }

    private string transformeDataSets()
    {
        string dataFinale = "";
        foreach (DataSet ds in dataSets)
        {
            dataFinale = ajouteData(dataFinale, ds.Values, ",");
            string finligne = "\n";
            if (dataSets.IndexOf(ds) == dataSets.Count - 1)
            {
                finligne = "";
            }
            dataFinale = ajouteData(dataFinale, ds.Targets, finligne);
        }
        return dataFinale;
    }

    private string ajouteData(string dataActuelle, double[] tableDouble, string finLigne)
    {
        int i = 0;
        foreach (double value in tableDouble)
        {
            i++;
            dataActuelle += value.ToString();
            if (i < tableDouble.Length)
            {
                dataActuelle += ",";
            }
            else
            {
                dataActuelle += finLigne;
            }
        }
        return dataActuelle;
    }

    public void sauveReseau()
    {
        verba.sauvegardeReseau();
    }
}
