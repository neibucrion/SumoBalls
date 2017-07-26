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

public class IANeurone : MonoBehaviour {

    public GameObject adversaire;
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
    private static NeuralNet net;
    private static List<DataSet> dataSets;
    private double[] derniereEntree = { };

    Direction directionAdversaire;
    Direction directionIA;

    private List<SouvenirEntree> entrees;
    private List<SouvenirCollision> collisions;
    private List<double[]> attentes;
    private Vector3 memoirePosition;
    private float dureeAttente;

    private Verbateur verba;

    // Use this for initialization
    void Start ()
    {
        chargeur = new ChargeurTableaux();
        net = new NeuralNet(8, 6, 4);
        dataSets = new List<DataSet>();
        directionAdversaire = adversaire.GetComponent<Direction>();
        directionIA = GetComponent<Direction>();
        memoirePosition = Vector3.zero;
        entrees = new List<SouvenirEntree>();
        collisions = new List<SouvenirCollision>();
        attentes = new List<double[]>();
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
            double[] inputs = {liste[0], liste[1], liste[2], liste[3], liste[4], liste[5], liste[6], liste[7]};
            double[] desired = {liste[8], liste[9], liste[10], liste[11]};
            dataSets.Add(new DataSet(inputs, desired));
            net.Train(dataSets, MinimumError);
        }
    }

    public bool adversaireActif()
    {
        return directionAdversaire.actif;
    }

    public Vector3 trouveDirection()
    {
        Vector3 direction = new Vector3();
        double[] reponseReseau = interrogeReseau();
        if (reponseReseau[0] > reponseReseau[1] && reponseReseau[0] > reponseReseau[2] && reponseReseau[0] > reponseReseau[3])
        {
            direction = attaque();
        }
        else if (reponseReseau[1] > reponseReseau[0] && reponseReseau[1] > reponseReseau[2] && reponseReseau[1] > reponseReseau[3])
        {
            direction = esquive();
        }
        else if (reponseReseau[2] > reponseReseau[0] && reponseReseau[2] > reponseReseau[1] && reponseReseau[2] > reponseReseau[3])
        {
            direction = provoque();
        }
        else
        {
            direction = defend();
        }        
        return direction;
    }

    private Vector3 attaque()
    {
        Vector3 direction = adversaire.transform.position - transform.position;
        return direction.normalized;       
    }

    private Vector3 esquive()
    {        
        Vector3 reference = transform.position - Vector3.zero;
        Vector3 refAdversaire = adversaire.transform.position - transform.position;
        Quaternion rotationVecteur = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        Vector3 direction = rotationVecteur*refAdversaire;
        if (Vector3.Angle(direction, reference) < 90.0f)
        {
            direction = -direction;
        }
        return direction.normalized;
    }

    private Vector3 provoque()
    {
        Vector3 direction = transform.position - Vector3.zero;
        return direction.normalized;
    }

    private Vector3 defend()
    {
        Vector3 direction = Vector3.zero - transform.position;
        return direction.normalized;
    }

    private double[] interrogeReseau()
    {
        double[] posAdversaire = {convertitValeurPosition(adversaire.transform.position.x),
                                  convertitValeurPosition(adversaire.transform.position.z)};        
        double[] dirAdversaire = {convertitDecimal(directionAdversaire.direction.x),
                                  convertitDecimal(directionAdversaire.direction.z) };

        double[] posIA = {convertitValeurPosition(transform.position.x),
                          convertitValeurPosition(transform.position.z) };
        double[] dirIA = {convertitDecimal(directionIA.direction.x),
                          convertitDecimal(directionIA.direction.z) };

        double[] inputs = {posAdversaire[0], posAdversaire[1], dirAdversaire[0], dirAdversaire[1], posIA[0], posIA[1], dirIA[0], dirIA[1]};
        derniereEntree = inputs;
        retientEntrees();
        double[] result = net.Compute(derniereEntree);
        return result;
    }

    private void retientEntrees()
    {
        SouvenirEntree souvenir = new SouvenirEntree();
        souvenir.entree = derniereEntree;
        souvenir.timecode = Time.realtimeSinceStartup;
        entrees.Add(souvenir);
        if (Time.realtimeSinceStartup - entrees[0].timecode > dureeMemoire)
            entrees.RemoveAt(0);
    }

    private double convertitValeurPosition(float valeur)
    {
        float calcul = convertitDecimal(valeur / (rayonArene*2.0f));
        calcul = Mathf.Clamp(calcul, 0.0f, 1.0f);
        double valFinale = calcul;
        return valFinale;
    }

    private float convertitDecimal(float valInitiale)
    {
        float valFinale = (valInitiale + 1.0f)/2.0f;
        return valFinale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (trainingCollisions && collision.gameObject.tag == "joueur")
        {
            SouvenirCollision nouveauSouvenir = new SouvenirCollision();
            nouveauSouvenir.entreePrecedente = entrees[0].entree;
            nouveauSouvenir.positionJoueur = adversaire.transform.position;
            nouveauSouvenir.directionJoueur = directionAdversaire.direction;
            nouveauSouvenir.positionIA = transform.position;
            nouveauSouvenir.directionIA = directionIA.direction;
            collisions.Add(nouveauSouvenir);
        }
    }

    public void Update()
    {
        if (trainingAttente)
            verifieAttente();
    }

    private void verifieAttente()
    {
        if (directionIA.actif && verifieImmobile())
        {
            dureeAttente += Time.deltaTime;
            if (dureeAttente > attenteMax)
            {
                dureeAttente = 0.0f;
                attentes.Add(derniereEntree);
            }
        }
        else
        {
            memoirePosition = transform.position;
            dureeAttente = 0.0f;
        }
    }

    private bool verifieImmobile()
    {
        bool retour = false;
        Vector3 difference = transform.position - memoirePosition;
        if (difference.magnitude < rayonArene*2.0f*facteurImmobilite)
        {
            retour = true;
        }
        return retour;
    }

    public void metAJourReseau(bool victoire)
    {
        if (trainingFinales)
            apprendDerniereEntree(victoire);
        if (trainingAttente)
            apprendAttentes();
        if (trainingCollisions)
            analyseCollisions();
        entrees = new List<SouvenirEntree>();
    }

    private void apprendDerniereEntree(bool victoire)
    {
        double[] desired = { 0.1d, 0.1d, 0.1d, 0.9d };
        if (victoire)
        {
            desired[0] = 0.9d;
            desired[3] = 0.1d;
        }
        dataSets.Add(new DataSet(entrees[0].entree, desired));
        net.Train(dataSets, MinimumError);
    }

    private void apprendAttentes()
    {
        double[] desired = { 0.1d, 0.1d, 0.9d, 0.1d };
        foreach (double[] attente in attentes)
        {
            dataSets.Add(new DataSet(attente, desired));
            net.Train(dataSets, MinimumError);
        }
        attentes = new List<double[]>();
    }

    private void analyseCollisions()
    {
        foreach (SouvenirCollision souvenir in collisions)
        {
            double[] desired = trouveDesir(souvenir);
            dataSets.Add(new DataSet(souvenir.entreePrecedente, desired));
            net.Train(dataSets, MinimumError);
        }
        collisions = new List<SouvenirCollision>();
    }

    private double[] trouveDesir(SouvenirCollision collision)
    {
        double[] retour = { 0.9d, 0.1d, 0.1d, 0.1d };
        Vector3 difference = collision.positionIA - collision.positionJoueur;
        float angle = Vector3.Angle(collision.directionJoueur, difference);
        if (angle < angleOffensif)
        {
            retour[0] = 0.1d;
            retour[1] = 0.9d;
        }
        return retour;
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
            if (dataSets.IndexOf(ds) == dataSets.Count -1)
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
