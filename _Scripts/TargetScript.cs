using UnityEngine;
using System.Collections;

public class TargetScript : MonoBehaviour {

	public void OnTriggerEnter () {
		Debug.Log("Target Reached");
		Vector3 newCol = new Vector3 (0, 0, 1);
		networkView.RPC ("setColor", RPCMode.AllBuffered, newCol);
	}

	[RPC]
	public void setColor (Vector3 newColor)
	{
		renderer.material.color = new Color (newColor.x, newColor.y, newColor.z, 1);
	}
}
