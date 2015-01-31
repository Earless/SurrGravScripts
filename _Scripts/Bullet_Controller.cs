using UnityEngine;
using System.Collections;

public class Bullet_Controller : MonoBehaviour {

	public bool GravityBullet = false;
	public int shooterNumber;
	public float Time2Live = 2;

	private float Time2Live_left;

	void Start()
	{
		Time2Live_left = Time2Live;
	}

	void FixedUpdate (){
		Time2Live_left -= Time.fixedDeltaTime;
		if(Time2Live_left<0)
			Destroy (gameObject);
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.tag.Equals ("level"))
		{
			Destroy (gameObject);
		}
	}
}
