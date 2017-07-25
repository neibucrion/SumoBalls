using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ArbitreQuatre : MonoBehaviour {

	public GameObject joueurBleu;
	public GameObject joueurRouge;
    public GameObject joueurVert;
    public GameObject joueurJaune;
    public Text texteBleu;
	public Text texteRouge;
    public Text texteVert;
    public Text texteJaune;

    private int scoreBleu = 0;
	private int scoreRouge = 0;
    private int scoreVert = 0;
    private int scoreJaune = 0;

    void Start()
	{
		texteRouge.transform.position = new Vector3(150.0f, Screen.height - 50.0f, 0.0f);
		texteBleu.transform.position = new Vector3(Screen.width -50.0f, Screen.height - 50.0f, 0.0f);
        texteVert.transform.position = new Vector3(150.0f, 50.0f, 0.0f);
        texteJaune.transform.position = new Vector3(Screen.width - 50.0f, 50.0f, 0.0f);
    }

	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.R))
			resetScore();

		if (Input.GetKey(KeyCode.Escape))
			Application.Quit();
	}

	void OnTriggerEnter(Collider other)
    {
        GameObject joueur = other.gameObject;
        initialiseUnJoueur(joueur);
        GameObject[] joueurs = GameObject.FindGameObjectsWithTag("joueur");
        foreach (GameObject joueur2 in joueurs)
        {
            if (joueur2 != joueur)
            {
                faitGagner(joueur2);
                initialiseUnJoueur(joueur2);
            }
        }
    }

	void faitGagner(GameObject joueur)
	{
		if (joueur == joueurBleu)
		{
			++scoreBleu;
			texteBleu.text = "Blue : " + scoreBleu;
		}
		else if (joueur == joueurRouge)
		{
			++scoreRouge;
			texteRouge.text = "Red : " + scoreRouge;
		}
        else if (joueur == joueurVert)
        {
            ++scoreVert;
            texteVert.text = "Green : " + scoreVert;
        }
        else
        {
            ++scoreJaune;
            texteJaune.text = "Yellow : " + scoreJaune;
        }
	}

	void resetScore()
	{
		scoreBleu = 0;
		texteBleu.text = "Blue : 0";
		scoreRouge = 0;
		texteRouge.text = "Red : 0";
        scoreVert = 0;
        texteVert.text = "Green : 0";
        scoreJaune = 0;
        texteJaune.text = "Yellow : 0";
    }

    void initialiseUnJoueur(GameObject joueur)
    {
        Rigidbody rigid = joueur.GetComponent<Rigidbody>();
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        Direction dirJoueur = joueur.GetComponent<Direction>();
        rigid.MovePosition(dirJoueur.positionInitiale); ;
    }
}
