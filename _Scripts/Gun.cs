using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Gun : MonoBehaviour {

	private Vector3 TruePosition;
	private Quaternion TrueRotation;

	void Start ()
	{
		if (BasicFunctions.playOffline)
		{
			this.enabled = false;
		}
	}

	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting)
		{
			TruePosition = transform.position;
			TrueRotation = transform.rotation;
			stream.Serialize (ref TruePosition);
			stream.Serialize (ref TrueRotation);
		}
		else
		{
			stream.Serialize (ref TruePosition);
			stream.Serialize (ref TrueRotation);
		}
	}

	void FixedUpdate ()
	{
		if (!networkView.isMine)
		{
			transform.rotation = Quaternion.Lerp (transform.rotation, TrueRotation, Time.fixedDeltaTime * 5f);
		}
	}
}
