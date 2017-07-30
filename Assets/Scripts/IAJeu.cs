using UnityEngine;
using System.Collections.Generic;

public class IAJeu : MonoBehaviour {
    public GameObject arbitre;
    public GameObject adversaire;
    public bool IAActive;
    public float angleSticky;

    private GereNeurone neurone;
    private Direction directionIA;
    private Direction directionAdversaire;
    [HideInInspector]
    public List<SouvenirEntree> entrees;
    [HideInInspector]
    public List<SouvenirCollision> collisions;
    [HideInInspector]
    public List<double[]> attentes;
    private Vector3 memoirePosition;
    private float dureeAttente;

    // Use this for initialization
    void Start ()
    {
        neurone = arbitre.GetComponent<GereNeurone>();
        directionIA = GetComponent<Direction>();
        directionAdversaire = adversaire.GetComponent<Direction>();
        memoirePosition = Vector3.zero;
        entrees = new List<SouvenirEntree>();
        collisions = new List<SouvenirCollision>();
        attentes = new List<double[]>();
    }

    public Vector3 ajusteDirectionSticky()
    {
        Vector3 dirActuelle = directionIA.direction;
        Vector3 difference = adversaire.transform.position - transform.position;
        if (Vector3.Angle(dirActuelle, difference) <= angleSticky)
            dirActuelle = difference.normalized*dirActuelle.magnitude;
        return dirActuelle;
    }

    public Vector3 trouveDirection()
    {
        Vector3 direction = new Vector3();
        double[] reponseReseau = neurone.interrogeReseau(gameObject, adversaire);
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
        retientEntrees();
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

    public bool adversaireActif()
    {
        return directionAdversaire.actif;
    }

    private void retientEntrees()
    {
        SouvenirEntree souvenir = new SouvenirEntree();
        souvenir.entree = neurone.derniereEntree;
        souvenir.timecode = Time.realtimeSinceStartup;
        entrees.Add(souvenir);
        if (Time.realtimeSinceStartup - entrees[0].timecode > neurone.dureeMemoire)
            entrees.RemoveAt(0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (neurone.trainingCollisions && collision.gameObject.tag == "joueur")
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
        if (neurone.trainingAttente)
            verifieAttente();
    }

    private void verifieAttente()
    {
        if (directionIA.actif && verifieImmobile())
        {
            dureeAttente += Time.deltaTime;
            if (dureeAttente > neurone.attenteMax)
            {
                dureeAttente = 0.0f;
                attentes.Add(neurone.derniereEntree);
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
        if (difference.magnitude < neurone.rayonArene * 2.0f * neurone.facteurImmobilite)
        {
            retour = true;
        }
        return retour;
    }
}
