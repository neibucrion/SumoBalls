using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Arbitre : MonoBehaviour {

	public GameObject joueurBleu;
	public GameObject joueurRouge;
	public Text texteBleu;
	public Text texteRouge;

    private GameObject[] joueurs;
	private int scoreBleu = 0;
	private int scoreRouge = 0;
    private bool autoEntraine = true;

    void Start()
	{
        joueurs = GameObject.FindGameObjectsWithTag("joueur");
		texteRouge.transform.position = new Vector3(100.0f, Screen.height - 50.0f, 0.0f);
		texteBleu.transform.position = new Vector3(Screen.width -100.0f, Screen.height - 50.0f, 0.0f);
        trouveAutoEntraine();
	}

    void trouveAutoEntraine()
    {
        autoEntraine = true;
        foreach (GameObject joueur in joueurs)
        {
            IAJeu iaJoueur = joueur.GetComponent<IAJeu>();
            if (!iaJoueur.IAActive)
            {
                autoEntraine = false;
                break;
            }
        }
        autoActiveJoueurs();
    }

    void autoActiveJoueurs()
    {
        foreach (GameObject joueur in joueurs)
        {
            Direction dirJoueur = joueur.GetComponent<Direction>();
            dirJoueur.actif = autoEntraine;
        }
    }

	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.R))
			resetScore();

		if (Input.GetKey(KeyCode.Escape))
        {
            GereNeurone neurone = GetComponent<GereNeurone>();
            neurone.sauvegardeGenerale();
            Application.Quit();
        }			
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject == joueurBleu)
			faitGagner(joueurRouge);
		else if (other.gameObject == joueurRouge)
			faitGagner(joueurBleu);
	}

	void faitGagner(GameObject joueur)
	{
        initialiseJoueurs();
		if (joueur == joueurBleu)
		{
			++scoreBleu;
			texteBleu.text = "Blue : " + scoreBleu;
		}
		else
		{
			++scoreRouge;
			texteRouge.text = "Red : " + scoreRouge;
        }
        ajusteIA(joueur);	
	}

    void ajusteIA(GameObject gagnant)
    {
        GereNeurone neurone = GetComponent<GereNeurone>();
        foreach (GameObject joueur in joueurs)
        {
            bool victoire = false;
            if (joueur == gagnant)
                victoire = true;
            IAJeu iaJoueur = joueur.GetComponent<IAJeu>();
            if (iaJoueur.IAActive)
            {
                neurone.metAJourReseau(victoire, iaJoueur);
            }
        }
        neurone.sauvegardeGenerale();
        autoActiveJoueurs();
    }

    void resetScore()
	{
		scoreBleu = 0;
		texteBleu.text = "Blue : 0";
		scoreRouge = 0;
		texteRouge.text = "Red : 0";
	}    

    void initialiseJoueurs()
    {
        initialiseUnJoueur(joueurBleu);
        initialiseUnJoueur(joueurRouge);
    }

    void initialiseUnJoueur(GameObject joueur)
    {
        Rigidbody rigid = joueur.GetComponent<Rigidbody>();
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        Direction dirJoueur = joueur.GetComponent<Direction>();
        rigid.MovePosition(dirJoueur.positionInitiale);
        dirJoueur.actif = false;
    }
}
