using UnityEngine;
using System.Collections;

public class Direction : MonoBehaviour {

	public float patate;    
	public string nomHorizontal;
	public string nomVertical;
    public bool torque = true;

    [HideInInspector]
    public bool actif = false;

    [HideInInspector]
    public Vector3 positionInitiale;
    [HideInInspector]
    public Vector3 direction;

    private IAJeu iajoueur;

    void Start()
    {
        positionInitiale = transform.position;
        iajoueur = GetComponent<IAJeu>();
    }

    // Update is called once per frame
    void Update () {        
		if (iajoueur.IAActive)
        {            
            direction = Vector3.zero;
            if (!actif)
                actif = iajoueur.adversaireActif();
            else
                direction = iajoueur.trouveDirection();
        }
        else
        {
            directionManuelle();
        }
        calculeDirection();
    }

    void directionManuelle()
    {
        float dirX = Input.GetAxis(nomHorizontal);
        float dirZ = Input.GetAxis(nomVertical);
        direction = new Vector3(dirX, 0.0f, dirZ);
        if (direction.magnitude > 0.25f)
        {
            actif = true;
        }
        direction = iajoueur.ajusteDirectionSticky();
    }

    void calculeDirection()
    {        
        direction = Vector3.ClampMagnitude(direction, 1.0f);
        Vector3 mouvement = direction * patate * Time.deltaTime;
        Rigidbody rigid = GetComponent<Rigidbody>();
        if (torque)
        {
            rigid.AddTorque(mouvement.z, 0.0f, -mouvement.x);
        }
        else
        {
            rigid.AddForce(mouvement);
        }
    }
}
