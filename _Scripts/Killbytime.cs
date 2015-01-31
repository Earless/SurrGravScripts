using UnityEngine;
using System.Collections;

public class Killbytime : MonoBehaviour {

	public float Time2Live = 0.5f;
	private float Time2Live_left;

	void Start ()
	{
		Time2Live_left = Time2Live;
	}

	void Update ()
	{
		Time2Live_left -= Time.fixedDeltaTime;
		if (Time2Live_left < 0)
			Destroy (gameObject);
	}
}
