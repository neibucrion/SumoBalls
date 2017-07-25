using UnityEngine;
using System.Collections;

public class DirectionAndroid : MonoBehaviour {

	public float amount;
	public bool joueur;

	// Update is called once per frame
	void Update () {
		if (joueur)
			bougeTouche();
	}

	void bougeTouche()
	{
		Touch myTouch = Input.GetTouch(0);
		Vector3 direction = new Vector3(myTouch.deltaPosition.x, 0.0f, myTouch.deltaPosition.y);
		direction = direction * amount * myTouch.deltaTime;
		GetComponent<Rigidbody>().AddForce(direction, ForceMode.VelocityChange);
	}
}
