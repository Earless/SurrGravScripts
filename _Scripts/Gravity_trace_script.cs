using UnityEngine;
using System.Collections;

public class Gravity_trace_script : MonoBehaviour {

	public int shooterNumber;

	public float Time2Live = 0.5f;
	public float fadelength;
	public float fadespeed;

	private float fadetime;
	private float alphavalue;
	private float Time2Live_left;

	void Start()
	{
		Time2Live_left = Time2Live;
		fadetime = fadelength + Time.time;
		alphavalue = 1.0f;
	}

	void FixedUpdate ()
	{
		if(Time.time>fadetime)
		{
			alphavalue = alphavalue - (fadespeed*Time.deltaTime);
			renderer.material.color = new Color
				(renderer.material.color.r,renderer.material.color.g,renderer.material.color.b, alphavalue);
			if (alphavalue<=0)
			{
				fadetime = float.MaxValue;
			}
		}

		Time2Live_left -= Time.fixedDeltaTime;
		if(Time2Live_left<0)
			Destroy (gameObject);
	}
}
