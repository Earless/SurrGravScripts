using UnityEngine;
using System.Collections;

public class Wapen : MonoBehaviour {

	public Referee_script referee;
	public bool KillLaser_On = false;

	private playerController playerScript;
	private bool Can_Hit = false;
	private Vector3 stab_vector = new Vector3 (0f, 0f, 0.25f);
	private Vector3 TruePosition;
	private Quaternion TrueRotation;
	private float stabbing = 0f;

	private Vector3 start_local_pos;

	public AudioClip stab_sound; 
	public AudioClip stab_someone_sound;

	void Start ()
	{
		if (BasicFunctions.ForkModus && !BasicFunctions.playOffline)
		{
			start_local_pos = transform.localPosition;
			referee = GameObject.FindGameObjectWithTag("Referee_Tag").GetComponent<Referee_script>();
			playerScript = gameObject.transform.parent.parent.GetComponent<playerController>();
		}
		else
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

	void Update()
	{
		transform.localPosition = start_local_pos+transform.localRotation*stab_vector*stabbing;
		if (networkView.isMine || BasicFunctions.playOffline) 
		{
			if (Input.GetMouseButtonDown (0) && !transform.parent.parent.GetComponent<playerController> ().endGame) 
			{
				AudioSource.PlayClipAtPoint (stab_sound, transform.position);
				collider.enabled = true;
				Can_Hit = true;
				stabbing=1f;
			}
			if (Input.GetMouseButtonUp (0) && !transform.parent.parent.GetComponent<playerController> ().endGame) 
			{
				collider.enabled = false;
				Can_Hit = false;
				stabbing=0f;
			}
		}
	}

	void FixedUpdate()
	{
		if (!networkView.isMine)
		{
			transform.position = Vector3.Lerp (transform.position, TruePosition, Time.fixedDeltaTime * 5f);
			transform.rotation = Quaternion.Lerp (transform.rotation, TrueRotation, Time.fixedDeltaTime * 5f);
		}
	}

	void OnTriggerStay (Collider hit)
	{
		if (networkView.isMine)
		{
			if(Can_Hit && hit.tag=="Player")// Input.GetKeyDown ("space"))
			{
				AudioSource.PlayClipAtPoint (stab_someone_sound, transform.position);

				if(!referee)
				{
					referee = (GameObject.FindGameObjectsWithTag("Referee_Tag"))[0].GetComponent<Referee_script>();
				}
				if (hit.gameObject.GetComponent<playerController>().isAlive && gameObject.transform.parent.parent.GetComponent<playerController>().isAlive)
				{
					referee.frag(playerScript.playerNumber, hit.gameObject.GetComponent<playerController>().playerNumber);
					Can_Hit = false;
				}
			}
		}
	}
}
