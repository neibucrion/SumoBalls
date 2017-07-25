using UnityEngine;
using System.Collections;
using NeuralNetwork;
using ChargeurCSV;
using UnityEngine.UI;
using System.Collections.Generic;

public class IANeurone : MonoBehaviour {

    public GameObject adversaire;
    public double rayonArene;

    //Neural Network Variables
    public double MinimumError;

    private const TrainingType TrType = TrainingType.MinimumError;
    private ChargeurTableaux chargeur;
    private static NeuralNet net;
    private static List<DataSet> dataSets;
    private double[] derniereEntree = { };

    Direction directionAdversaire;
    Direction directionIA;

    // Use this for initialization
    void Start ()
    {
        chargeur = new ChargeurTableaux();
        net = new NeuralNet(8, 8, 4);
        dataSets = new List<DataSet>();
        directionAdversaire = adversaire.GetComponent<Direction>();
        directionIA = GetComponent<Direction>();
        entraineReseau();
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
        Vector3 direction = transform.position - adversaire.transform.position;
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
        double[] result = net.Compute(derniereEntree);
        return result;
    }

    private double convertitValeurPosition(float valeur)
    {
        double valFinale = convertitDecimal(valeur / rayonArene);
        return valFinale;
    }

    private double convertitDecimal(double valInitiale)
    {
        double valFinale = (valInitiale + 1.0d)/2.0d;
        return valFinale;
    }

    public void apprendDerniereEntree(bool victoire)
    {
        double[] desired = { 0.1d, 0.1d, 0.1d, 0.9d };
        if (victoire)
        {
            desired[0] = 0.9d;
            desired[3] = 0.1d;
        }
        dataSets.Add(new DataSet(derniereEntree, desired));
        net.Train(dataSets, MinimumError);
    }
}
