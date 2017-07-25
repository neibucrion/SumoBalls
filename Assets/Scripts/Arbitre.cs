using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Arbitre : MonoBehaviour {

	public GameObject joueurBleu;
	public GameObject joueurRouge;
	public Text texteBleu;
	public Text texteRouge;

	private int scoreBleu = 0;
	private int scoreRouge = 0;

	void Start()
	{
		texteRouge.transform.position = new Vector3(100.0f, Screen.height - 50.0f, 0.0f);
		texteBleu.transform.position = new Vector3(Screen.width -100.0f, Screen.height - 50.0f, 0.0f);
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.R))
			resetScore();

		if (Input.GetKey(KeyCode.Escape))
			Application.Quit();
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
            ajusteIA(false);
		}
		else
		{
			++scoreRouge;
			texteRouge.text = "Red : " + scoreRouge;
            ajusteIA(true);
        }		
	}

    void ajusteIA(bool victoire)
    {
        Direction dirRouge = joueurRouge.GetComponent<Direction>();
        if (dirRouge.ia)
        {
            IANeurone IA = joueurRouge.GetComponent<IANeurone>();
            IA.apprendDerniereEntree(victoire);
        }
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
    }
}
