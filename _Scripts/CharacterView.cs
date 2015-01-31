using UnityEngine;
using System.Collections;

public class CharacterView : MonoBehaviour {

	public string armature = "Armature";
	public string body = "Circle";

	void Start ()
	{
		if(networkView.isMine || BasicFunctions.playOffline)
		{
			Destroy(transform.Find(armature).gameObject);
			Destroy (transform.Find (body).gameObject);
		}
	}
}
