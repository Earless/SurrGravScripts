using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class playerController : MonoBehaviour
{
	#region [init look around]
	public float speed;
	public float jumpSpeed = 8.0F;
	//private Vector3 Current_Global_Force;
	public Camera_Control Main_Camera;
	private float speed_multiplier = 1f;
	private Vector3 TruePosition;
	private Quaternion TrueRotation;
	public float Gravity_Shift_Time = 10f;

	private Quaternion before_shift;
	private Quaternion after_shift;
	private float before_shift_cam;
	private float after_shift_cam;
	private Vector3 before_shift_vel;

	private float Gravity_Shift_Counter;
	private Animator anim;

	public enum RotationAxes
	{
		MouseXAndY = 0,
		MouseX = 1,
		MouseY = 2 }
	;
	public RotationAxes axes = RotationAxes.MouseX;
	//public float sensitivityX = 15F;
	//public float sensitivityY = 15F;
	public float minimumX = -360F;
	public float maximumX = 360F;
	public float minimumY = -60F;
	public float maximumY = 60F;
	float rotationY = 0F;
	#endregion

	#region [init other]

	public GameObject Wounded_Panel;
	public float Gravity_Strength = 10f;
	public Vector3 Initial_Gravity_Direction;
	public Vector3 Gravity_Direction;
	public Rigidbody Kill_Bullet;
	public float Bullet_Speed = 15f;
	public GameObject playerPrefab;
	public GameObject LightningLine;
	public GameObject KillLine;
	public Account activeAccount; //= BasicFunctions.activeAccount;
	public int playerNumber;
	public AudioClip kill_shot_sound;
	//public AudioClip gravity_shot_sound;
	public AudioClip jump_sound;
	public static bool dontDestroy;
	public bool isAlive = true;
	public bool endGame = false;
	private NW_Spawning spawnScript;
	private bool spawnChosen = false;
	private Referee_script referee;
	private int vertexsize;
	public int VerticesPerUnit;
	public float Gibrange;
	private float multiplier;
	public float time2death = 0f;
	public float Sphere_collider_radius = 0.6f;
	private float JumpTime;
	public GameObject ScoreScreen;
	public AudioClip boundary_death_sound;
	public AudioClip respawn_sound;
	public AudioClip getting_hit_sound;
	private bool quitting;
	private ScoreScreen scoreScreen;
	public bool showWP = false;
	private float WoundedTimer = 0.2f;
	public GameObject Leven1;
	public GameObject Leven2;
	public GameObject Leven3;
	//Color change
	private float ColorChangeLength = 0.3f;
	private float ColorChangeTime;
	private bool ColorRed;
	private bool isWalking = true;
	private bool jumpMode = BasicFunctions.JumpModus;
	public Gun gunscript;

	public AudioClip endfork_sound;
	public AudioClip endrailgun_sound;
	public AudioClip shot_death_sound;

	#endregion

	void Start ()
	{
		Wounded_Panel = GameObject.FindGameObjectWithTag("WPanel");
		quitting = false;
		if (networkView.isMine || BasicFunctions.playOffline) {
			Wounded_Panel.SetActive(false);
			if (!BasicFunctions.playOffline) {
				activeAccount.Number = playerNumber;
				if (!referee)
				{
					referee = GameObject.FindGameObjectWithTag("Referee_Tag").GetComponent<Referee_script>();
				}
				if (!Wounded_Panel)
				{
					Wounded_Panel = GameObject.FindGameObjectWithTag("WPanel");
				}
				Leven1 = GameObject.FindGameObjectWithTag("Leven1");
				Leven2 = GameObject.FindGameObjectWithTag("Leven2");
				Leven3 = GameObject.FindGameObjectWithTag("Leven3");
			}
			Screen.lockCursor = true;
			rigidbody.freezeRotation = true;
			Gravity_Direction = Initial_Gravity_Direction;
			spawnScript = GameObject.FindGameObjectWithTag ("SpawnTag").GetComponent<NW_Spawning> ();
			JumpTime = Time.time;

			if(!BasicFunctions.playOffline)
				networkView.RPC ("Loop_toch_naar_de_tering", RPCMode.All, "Walk");
			//anim.SetBool("Walk", true);
		}
	}

	[RPC]
	public void Loop_toch_naar_de_tering (string animatietype)
	{
		if(!anim)
			anim = GetComponent<Animator> ();

		if (animatietype == "Stab") 
		{
			anim.SetBool ("Stab", true);
		}
		else if (animatietype == "Walk")
		{
			anim.SetBool ("Walk", true);
		}
		else if (animatietype == "Jump")
		{
			anim.SetBool ("Jump", true);
		}
		else if (animatietype == "NoStab") 
		{
			anim.SetBool ("Stab", false);
		}
		else if (animatietype == "NoWalk")
		{
			anim.SetBool ("Walk", false);
		}
		else if (animatietype == "NoJump")
		{
			anim.SetBool ("Jump", false);
		}
		else
		{
			Debug.Log ("ERROR in animation. playercontroller.cs");
		}
	}

	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting) {
			TruePosition = transform.position;
			TrueRotation = transform.rotation;
			stream.Serialize (ref TruePosition);
			stream.Serialize (ref TrueRotation);
		} else {
			stream.Serialize (ref TruePosition);
			stream.Serialize (ref TrueRotation);
		}
	}

	public void Switch_Gravity (Vector3 new_Gravity, Vector3 hitpoint)
	{
		if (networkView.isMine || BasicFunctions.playOffline) {
			/*if (new_Gravity != Gravity_Direction) {
								before_shift = transform.rotation;
								Gravity_Direction = new_Gravity;
								Vector3 New_Player_Forward_tmp = BasicFunctions.ProjectVectorOnPlane (Gravity_Direction, transform.forward);
								after_shift = Quaternion.LookRotation (New_Player_Forward_tmp, Gravity_Direction * -1f);
								Gravity_Shift_Counter = Gravity_Shift_Time;
						}*/

			if (new_Gravity != Gravity_Direction) {
				before_shift = transform.rotation;
				before_shift_cam = Main_Camera.rotationY;
				before_shift_vel = rigidbody.velocity;
				if (new_Gravity == Gravity_Direction * -1f) {
					//180 Graden draaien heeft aparte behandeling nodig
					after_shift = Quaternion.LookRotation (transform.forward, new_Gravity * -1f);
					after_shift_cam = before_shift_cam*-1f;
					//Debug.Log("180 graden draai");
				} else {
					//berekenen wat voor extra hoek erij moet.
					Vector3 player2point = hitpoint - transform.position;
					//berekenen wat voor extra hoek erij moet.
					Vector3 extra_tmp = BasicFunctions.ProjectVectorOnPlane (new_Gravity, BasicFunctions.ProjectVectorOnPlane (Gravity_Direction, player2point));
					//Debug.Log(extra_tmp);
					after_shift = Quaternion.LookRotation (Gravity_Direction * -1f + extra_tmp, new_Gravity * -1f);
					after_shift_cam = 0f;
				}
				Gravity_Shift_Counter = Gravity_Shift_Time;
				Gravity_Direction = new_Gravity;
			}

		}
	}

	public void Fire_Grav_Bullet (Vector3 pos1, Vector3 pos2)
	{
		networkView.RPC ("fireGravityLaser", RPCMode.Others, pos1, pos2, activeAccount.Number);
	}

	public void Fire_Kill_Bullet (Vector3 pos1, Vector3 pos2, int shooter)
	{
		networkView.RPC ("fireKillLaser", RPCMode.Others, pos1, pos2, shooter);
	}

	public void setScreenTimer (int shooter)
	{
		if (networkView.isMine)
		{
			if (!scoreScreen)
			{
				scoreScreen = GameObject.FindGameObjectWithTag("ScoreScreen").GetComponent<ScoreScreen>();
			}
			scoreScreen.shooter = shooter;
			scoreScreen.time2show = time2death;
		}
	}

	public void setKillTimer (int target)
	{
		if (networkView.isMine)
		{
			if (!scoreScreen)
			{
				scoreScreen = GameObject.FindGameObjectWithTag("ScoreScreen").GetComponent<ScoreScreen>();
			}
			scoreScreen.target = target;
			scoreScreen.showKill = 2f;
		}
	}

	public void setEndScreenTimer (int winner)
	{
		if (networkView.isMine)
		{
			if (!scoreScreen)
			{
				scoreScreen = GameObject.FindGameObjectWithTag("ScoreScreen").GetComponent<ScoreScreen>();
			}
			//scoreScreen.time2show = 1000f;
			scoreScreen.winner = winner;
			scoreScreen.showScoreScreen();
		}
	}

	public void PlayGetHit ()
	{
		if (networkView.isMine)
			AudioSource.PlayClipAtPoint (getting_hit_sound, transform.position);
	}

	public void PlayDead ()
	{
		if (networkView.isMine)
			AudioSource.PlayClipAtPoint (boundary_death_sound, transform.position, 2.0f);
	}

	public void PlayShotDead ()
	{
		if (networkView.isMine)
			AudioSource.PlayClipAtPoint (shot_death_sound, transform.position, 2.0f);
	}

	public void PlayEndGameSound ()
	{
		if (BasicFunctions.ForkModus) 
		{
			AudioSource.PlayClipAtPoint (endfork_sound, transform.position);
		}
		else
		{
			AudioSource.PlayClipAtPoint (endrailgun_sound, transform.position);
		}
	}

	/* Color change */
	public void hitColorRed()
	{
		//moet op de renderer van de circle van de player prefab worden toegepast
		//networkView.RPC("hitColorRedRPC", RPCMode.All);
		if (GetComponentInChildren<SkinnedMeshRenderer>())
			GetComponentInChildren<SkinnedMeshRenderer>().renderer.material.color = new Color (0.8f,0f,0f, 1.0f);
		ColorRed = true;
		ColorChangeTime = Time.time + ColorChangeLength;
	}

	public void hitColorRegular()
	{
		//moet op de renderer van de circle van de player prefab worden toegepast
		//networkView.RPC("hitColorRegularRPC", RPCMode.All);
		if (GetComponentInChildren<SkinnedMeshRenderer>())
			GetComponentInChildren<SkinnedMeshRenderer>().renderer.material.color = new Color (0.8f,0.8f,0.8f, 1.0f);
		ColorRed = false;
	}

	[RPC]
	public void hitColorRedRPC ()
	{
		if (GetComponentInChildren<SkinnedMeshRenderer>())
			GetComponentInChildren<SkinnedMeshRenderer>().renderer.material.color = new Color (0.8f,0f,0f, 1.0f);
		ColorRed = true;
		ColorChangeTime = Time.time + ColorChangeLength;
	}

	[RPC]
	public void hitColorRegularRPC ()
	{
		if (GetComponentInChildren<SkinnedMeshRenderer>())
			GetComponentInChildren<SkinnedMeshRenderer>().renderer.material.color = new Color (0.8f,0.8f,0.8f, 1.0f);
		ColorRed = false;
	}

	[RPC]
	void fireGravityLaser (Vector3 pos1, Vector3 pos2, int number)
	{
		LineRenderer LightningLineCurrent = (LineRenderer)Instantiate (LightningLine.GetComponent<LineRenderer> ());

		//new
		float Distance = Mathf.Sqrt ((pos1 - pos2).sqrMagnitude);
		float floatvertexsize = VerticesPerUnit * Distance;
		vertexsize = (int)floatvertexsize;
		if (vertexsize > 30000) {
			vertexsize = 30000;
		}
		LightningLineCurrent.SetVertexCount (vertexsize);
		LightningLineCurrent.SetPosition (vertexsize - 1, pos1 + new Vector3 (0.01f, -0.01f, 0.01f));
		for (int i=1; i<(vertexsize-1); i++) {
			float multiplier = ((i * 1.0f) / (vertexsize - 1));
			LightningLineCurrent.SetPosition (i, (multiplier * (pos1 - pos2)) + pos2 + new Vector3 (Random.Range (-Gibrange, Gibrange), Random.Range (-Gibrange, Gibrange), Random.Range (-Gibrange, Gibrange)));
		}
		LightningLineCurrent.SetPosition (0, pos2);
		//\new

	}

	[RPC]
	void fireKillLaser (Vector3 pos1, Vector3 pos2, int Pnumber)
	{
		LineRenderer KillLineCurrent = (LineRenderer)Instantiate (KillLine.GetComponent<LineRenderer> ());
		KillLineCurrent.GetComponent<Gravity_trace_script> ().shooterNumber = Pnumber;
		//KillLineCurrent.SetPosition(1, pos1);
		//KillLineCurrent.SetPosition(0, pos2);

		//new
		float Distance = Mathf.Sqrt ((pos1 - pos2).sqrMagnitude);
		float floatvertexsize = VerticesPerUnit * Distance;
		vertexsize = (int)floatvertexsize;
		if (vertexsize > 30000) {
			vertexsize = 30000;
		}
		KillLineCurrent.SetVertexCount (vertexsize);
		KillLineCurrent.SetPosition (vertexsize - 1, pos1);
		for (int i=1; i<(vertexsize-1); i++) {
			float multiplier = ((i * 1.0f) / (vertexsize - 1));
			KillLineCurrent.SetPosition (i, (multiplier * (pos1 - pos2)) + pos2 + new Vector3 (Random.Range (-Gibrange, Gibrange), Random.Range (-Gibrange, Gibrange), Random.Range (-Gibrange, Gibrange)));
		}
		KillLineCurrent.SetPosition (0, pos2);

	}

	[RPC]
	void hitByBullet (int shooter, int target)
	{
		Debug.Log ("Target: " + target + ", Shooter: " + shooter + ", ActiveNumber: " + activeAccount.Number);
	}

	void Update ()
	{
		if (ColorRed && Time.time > ColorChangeTime)
		{
			hitColorRegular();
		}

		if (showWP)
		{
			Wounded_Panel.SetActive(true);
			WoundedTimer -= Time.deltaTime;
			if (WoundedTimer <= 0)
			{
				Wounded_Panel.SetActive(false);
				WoundedTimer = 0.2f;
				showWP = false;
			}
		}

		if (Input.GetKeyDown (KeyCode.Escape) && quitting == false) {
			quitting = true;

			if (!spawnScript) {
				spawnScript = GameObject.FindGameObjectWithTag ("SpawnTag").GetComponent<NW_Spawning> ();
			}

			if(BasicFunctions.playOffline){
				Application.LoadLevel ("Menu_New");
			}
			else if (Network.isServer && networkView.isMine)
			{
				Debug.Log("ISSERVER");
				dontDestroy = true;
				string gamemode;
				if (!endGame)
				{
					if (BasicFunctions.ForkModus)
					{
						gamemode = "FORK";
					}
					else
					{
						gamemode = "RAILGUN";
					}
					if (BasicFunctions.loginServer)
					{
						endGame = true;
						string url = "http://drproject.twi.tudelft.nl:8082/GameRegister?Server=" + BasicFunctions.activeAccount.Name + "&Finished=0" + "&Gamemode=" + gamemode;
						WWW www = new WWW (url);
						StartCoroutine (WaitForGameLog (www));
					}
					else
					{
						spawnScript.closeServer (false);
					}
				}
				else
				{
					spawnScript.closeServer (true);
				}
			}
			else if (Network.isClient || networkView.isMine)
			{
				Debug.Log("ISGEENSERVER");
				if (spawnScript.serverHasQuit)
				{
					spawnScript.closeClient (true, true);
				}
				else if (endGame)
				{
					spawnScript.closeClient (false, true);
				}
				else
				{
					spawnScript.closeClient (false, false);
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.C))
		{
			Screen.lockCursor = true;
			Screen.showCursor = false;
		}
	}

	void FixedUpdate ()
	{
		if (networkView.isMine || BasicFunctions.playOffline) {


			if (Gravity_Shift_Counter > 1f) {
				Gravity_Shift_Counter--;
				transform.rotation = Quaternion.Lerp (after_shift, before_shift, Gravity_Shift_Counter / Gravity_Shift_Time);
				Main_Camera.rotationY = Mathf.Lerp(after_shift_cam, before_shift_cam, Gravity_Shift_Counter / Gravity_Shift_Time);
				rigidbody.velocity = before_shift_vel;
			} else if (Gravity_Shift_Counter == 1f) {
				Gravity_Shift_Counter = 0f;
				transform.rotation = after_shift;
				Main_Camera.rotationY = after_shift_cam;
				rigidbody.velocity = new Vector3 (0f, 0f, 0f);
			} else {

				#region [look around]
				if (axes == RotationAxes.MouseXAndY) {
					float rotationX = transform.localEulerAngles.y + Input.GetAxis ("Mouse X") * BasicFunctions.Sensitivity;

					rotationY += Input.GetAxis ("Mouse Y") * BasicFunctions.Sensitivity;
					rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);

					transform.localEulerAngles = new Vector3 (-rotationY, rotationX, 0);
				} else if (axes == RotationAxes.MouseX) {
					transform.Rotate (0, Input.GetAxis ("Mouse X") * BasicFunctions.Sensitivity, 0);
				} else {
					rotationY += Input.GetAxis ("Mouse Y") * BasicFunctions.Sensitivity;
					rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);

					transform.localEulerAngles = new Vector3 (-rotationY, transform.localEulerAngles.y, 0);
				}
			}
			#endregion

			transform.TransformDirection (Vector3.forward);
			//DIT STUK BEWEEGT DE SPELER ALLEEN ALS IE NIET DOOD IS
			if (isAlive && !endGame) {
				speed_multiplier = Mathf.Lerp (speed_multiplier, 1f, Time.fixedDeltaTime * 2f);
				/*Collider[] hitColliders = Physics.OverlapSphere (transform.position + Gravity_Direction * 0.6f, Sphere_collider_radius);

				for (int i=0; i<hitColliders.Length; i++) {
					if (hitColliders [i].tag == "level") {
						Can_Jump = true;
					}
				}*/

				//HIER WORDT DE VELOCITY BEREKEND
				Vector3 New_Velocity = new Vector3 (0f, 0f, 0f);
				//New_Velocity = Vector3.Scale(rigidbody.velocity, Gravity_Direction);
				New_Velocity = Vector3.Lerp (Vector3.Scale (rigidbody.velocity, Vector3.Scale (Gravity_Direction, Gravity_Direction)), Gravity_Direction * Gravity_Strength, Time.fixedDeltaTime * 3f);
				New_Velocity += transform.forward * speed * speed_multiplier * Input.GetAxis ("Vertical");
				New_Velocity += Vector3.Cross (transform.up, transform.forward) * speed * speed_multiplier * Input.GetAxis ("Horizontal");

				if(!BasicFunctions.playOffline){
					if (Input.GetAxis ("Horizontal")==0 && Input.GetAxis ("Vertical")==0) {
						if (isWalking)
						{
							networkView.RPC ("Loop_toch_naar_de_tering", RPCMode.All, "NoWalk");
							isWalking = false;
						}
					} else {
						if (!isWalking)
						{
							networkView.RPC ("Loop_toch_naar_de_tering", RPCMode.All, "Walk");
							isWalking = true;
						}
					}
				}

				rigidbody.velocity = New_Velocity;

				if (Input.GetKeyDown("space")){
					if(isAlive && jumpMode && !endGame && (JumpTime + 0.35f) < Time.time) {
					Collider[] hitColliders = Physics.OverlapSphere (transform.position + Gravity_Direction * 0.6f, Sphere_collider_radius);
					
						for (int i=0; i<hitColliders.Length; i++) {
							if (hitColliders [i].tag == "level") {
								rigidbody.velocity += (Gravity_Direction * jumpSpeed * -1f);
								AudioSource.PlayClipAtPoint (jump_sound, transform.position, 2.0f);
								JumpTime = Time.time;
								if(!BasicFunctions.playOffline)
								{
									if (BasicFunctions.amountPlayers > 1)
									{
										if (!isWalking)
										{
											networkView.RPC ("Loop_toch_naar_de_tering", RPCMode.All, "Walk");
											isWalking = true;
										}
									}
								}
								break;
							}
						}
					}
				} 
			} else {
				//DIT STUK IS DE SPELER ALS IE DOOD IS
				rigidbody.velocity = new Vector3 (0f, 0f, 0f);
				if (!isAlive && !endGame) {
					time2death -= Time.fixedDeltaTime;
					if (time2death <= 3f) {
						if (!spawnChosen) {
							Vector3 randomSpawnPoint;
							if (!BasicFunctions.playOffline)
							{
								int index = Random.Range (0, spawnScript.respawnLocations.Count - 1); //Take random integer
								randomSpawnPoint = spawnScript.respawnLocations [index];
							}
							else
							{
								int index = Random.Range (0, spawnScript.spawnLocations.Count - 1); //Take random integer
								randomSpawnPoint = spawnScript.spawnLocations [index];
							}
							transform.position = randomSpawnPoint;//new Vector3(-1f, -1f, -1f);
							spawnChosen = true;
						}
						if (time2death <= 0f) {
							if (!BasicFunctions.playOffline)
								networkView.RPC ("PlayerRespawn", RPCMode.All, transform.position);
							else
							{
								AudioSource.PlayClipAtPoint (respawn_sound, transform.position);
								isAlive = true;
								gameObject.GetComponent<CapsuleCollider>().enabled = true;
							}
							spawnChosen = false;
						}
					}
				} else {
				}
			}
		} else {
			//Control of other player
			transform.position = Vector3.Lerp (transform.position, TruePosition, Time.fixedDeltaTime * 5f);
			transform.rotation = Quaternion.Lerp (transform.rotation, TrueRotation, Time.fixedDeltaTime * 5f);
		}
	}

	[RPC]
	void PlayerRespawn (Vector3 SpawnPOsition)
	{
		AudioSource.PlayClipAtPoint (respawn_sound, transform.position);
		isAlive = true;
		transform.position = SpawnPOsition;
		if (!networkView.isMine)
		{
			gameObject.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
		}
		gameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
		gameObject.GetComponent<CapsuleCollider> ().enabled = true;
		if (networkView.isMine)
		{
			if (!Leven1)
			{
				Leven1 = GameObject.FindGameObjectWithTag("Leven1");
			}
			if (!Leven2)
			{
				Leven2 = GameObject.FindGameObjectWithTag("Leven2");
			}
			if (!Leven3)
			{
				Leven3 = GameObject.FindGameObjectWithTag("Leven3");
			}
			Leven1.SetActive(true);
			Leven2.SetActive(true);
			Leven3.SetActive(true);
		}
	}

	[RPC]
	void WalkAnim (int player, bool set)
	{
		//referee.players [player - 1].anim.SetBool ("Walk", set);
		//anim.SetBool ("Walk", set);
		Debug.Log ("dit mag niet aangeroepen worden! zoek in playercontroller naar RPC");
		//Debug.Log(anim.GetBool("Walk"));
	}

	[RPC]
	void JumpAnim (int player, bool set)
	{
		Debug.Log ("dit mag niet aangeroepen worden! zoek in playercontroller naar RPC");
	}

	void OnCollisionStay (Collision collisionInfo)
	{
		if (networkView.isMine || BasicFunctions.playOffline) {
			if (collisionInfo.gameObject.tag == "DeathBoundary") {
				AudioSource.PlayClipAtPoint (boundary_death_sound, transform.position, 2.0f);
				if (!BasicFunctions.playOffline)
				{
					if (!referee) {
						referee = (GameObject.FindGameObjectsWithTag ("Referee_Tag")) [0].GetComponent<Referee_script> ();
					}
					if (this.isAlive) {
						referee.fragged (BasicFunctions.activeAccount.Number);
					}
				}
				else
				{
					if (this.isAlive)
					{
						this.isAlive = false;
						time2death = 1.5f;
						gameObject.GetComponent<CapsuleCollider>().enabled = false;
					}
				}
			}
		}
	}

	IEnumerator WaitForGameLog (WWW www)
	{
		yield return www;

		if (www.error == null) {
			if (www.text.Equals ("Succesfully Registered Game")) {
				Debug.Log ("Succesfully logged game");
			} else {
				Debug.Log ("Failed to log game");
			}
		}

		for(int i=0;i<BasicFunctions.startingAccounts.Count-1;i++){
			string urlParticipant = "http://drproject.twi.tudelft.nl:8082/ParticipantsRegister?SERVER="+BasicFunctions.activeAccount.Name + "&PLAYER="+BasicFunctions.startingAccounts[i];
			WWW www2 = new WWW(urlParticipant);
			StartCoroutine (WaitForParticipantRegister(www2));
		}

		string finalurlparticipant = "http://drproject.twi.tudelft.nl:8082/ParticipantsRegister?SERVER="+BasicFunctions.activeAccount.Name + "&PLAYER="+BasicFunctions.startingAccounts[BasicFunctions.startingAccounts.Count-1];
		WWW www3 = new WWW(finalurlparticipant);
		yield return StartCoroutine (WaitForParticipantRegister(www3));
		spawnScript.closeServer (false);
	}

	IEnumerator WaitForParticipantRegister (WWW www)
	{
		yield return www;
		
		if (www.error == null) {
			if (www.text.Equals ("succesfully logged participant")) {
				Debug.Log ("Succesfully logged participant");
			} else {
				Debug.Log ("Failed to log participant");
			}
		}
		//spawnScript.closeServerInGame ();
	}
}