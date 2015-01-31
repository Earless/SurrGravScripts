using UnityEngine;
using System.Collections;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)

[AddComponentMenu("Camera-Control/Mouse Look")]
public class Camera_Control : MonoBehaviour {
	
	#region [init for look around]
	public GameObject LightningLine;
	public GameObject KillLine;

	public Texture2D crosshairImage;
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;
	
	public float minimumX = -360F;
	public float maximumX = 360F;
	
	public float minimumY = -60F;
	public float maximumY = 60F;

	public bool can_rotate = true;
	public float rotationY = 0F;
	#endregion
	#region [init other]
	public playerController player;
	public Referee_script referee;

	public GameObject playercam;

	public Rigidbody Gravity_Bullet;

	public Transform GunTransform;

	public AudioClip bullet_hit_sound;

	public int VerticesPerUnit;

	public float reloadTime = 0f;
	public float Bullet_Speed = 5f;
	public float Gravity_Switch_Timer= 0f;
	public float Gibrange;
	
	public AudioClip kill_shot_sound;
	public AudioClip gravity_shot_sound;

	private Account activeAccount = BasicFunctions.activeAccount;

	private int vertexsize;

	private float multiplier;
	#endregion
	
	void Start()
	{
		if (networkView.isMine || BasicFunctions.playOffline)
		{
			transform.rotation = player.transform.rotation;
		}
		else
		{
			GetComponent<Camera_Control>().enabled = false;
			GetComponent<AudioListener>().enabled = false;
			GetComponent<Camera>().enabled = false;
		}
	}
	
	void Fire_Gravity_Bullet()
	{
		if (networkView.isMine || BasicFunctions.playOffline)
		{
			AudioSource.PlayClipAtPoint(gravity_shot_sound, transform.position);
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width/2f, Screen.height/2f));
			if (Physics.Raycast(ray, out hit)) {
				LineRenderer LightningLineCurrent = (LineRenderer)Instantiate(LightningLine.GetComponent<LineRenderer>());
				float Distance = Mathf.Sqrt((GunTransform.position - hit.point).sqrMagnitude);
				float floatvertexsize = VerticesPerUnit * Distance;
				vertexsize = (int) floatvertexsize;
				if ( vertexsize > 30000 ){
					vertexsize = 30000;
				}
				LightningLineCurrent.SetVertexCount(vertexsize);
				LightningLineCurrent.SetPosition(vertexsize-1, GunTransform.position+new Vector3(0.01f,-0.01f,0.01f));
				for(int i=1; i<(vertexsize-1) ;i++)
				{
					float multiplier = ((i*1.0f)/(vertexsize-1));
					LightningLineCurrent.SetPosition(i, (multiplier*(GunTransform.position - hit.point)) + hit.point + new Vector3 (Random.Range(-Gibrange, Gibrange),Random.Range(-Gibrange, Gibrange),Random.Range(-Gibrange, Gibrange)));			
				}
				LightningLineCurrent.SetPosition(0, hit.point);
				
				if (!BasicFunctions.playOffline)
					player.Fire_Grav_Bullet(GunTransform.position+new Vector3(0.01f,-0.01f,0.01f),hit.point);
				if(hit.collider.tag=="level"){
					player.Switch_Gravity(hit.normal*-1f, hit.point);
				}
			}
		}
	}

	void Fire_Kill_Laser()
	{
		if (networkView.isMine || BasicFunctions.playOffline)
		{
			AudioSource.PlayClipAtPoint(kill_shot_sound, transform.position);
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width/2f, Screen.height/2f));
			if (Physics.Raycast(ray, out hit)) {
				LineRenderer KillLineCurrent = (LineRenderer)Instantiate(KillLine.GetComponent<LineRenderer>());

				float Distance = Mathf.Sqrt((GunTransform.position - hit.point).sqrMagnitude);
				float floatvertexsize = VerticesPerUnit * Distance;
				vertexsize = (int) floatvertexsize;
				if ( vertexsize > 30000 ){
					vertexsize = 30000;
				}
				KillLineCurrent.SetVertexCount(vertexsize);
				KillLineCurrent.SetPosition(vertexsize-1, GunTransform.position+new Vector3(0.01f,-0.01f,0.01f));
				for(int i=1; i<(vertexsize-1) ;i++)
				{
					float multiplier = ((i*1.0f)/(vertexsize-1));
					KillLineCurrent.SetPosition(i, (multiplier*(GunTransform.position - hit.point)) + hit.point + new Vector3 (Random.Range(-Gibrange, Gibrange),Random.Range(-Gibrange, Gibrange),Random.Range(-Gibrange, Gibrange)));			
				}
				KillLineCurrent.SetPosition(0, hit.point);
	
				if (!BasicFunctions.playOffline)
				{
					int shootNumber = activeAccount.Number;
					KillLineCurrent.GetComponent<Gravity_trace_script>().shooterNumber = shootNumber;
					player.Fire_Kill_Bullet(GunTransform.position+new Vector3(0.01f,-0.01f,0.01f),hit.point, shootNumber);
				}
				if(hit.collider.tag=="Player")
				{
					AudioSource.PlayClipAtPoint (bullet_hit_sound, transform.position);
					if(!referee){
						referee = (GameObject.FindGameObjectsWithTag("Referee_Tag"))[0].GetComponent<Referee_script>();
					}
					referee.frag(KillLineCurrent.GetComponent<Gravity_trace_script>().shooterNumber, hit.collider.gameObject.GetComponent<playerController>().playerNumber);
				}
			}
		}
	}

	void Update ()
	{
		if (networkView.isMine || BasicFunctions.playOffline)
		{
			if(Gravity_Switch_Timer>0f)
			{
				transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
			}else
			{
				#region [look around]
				if (axes == RotationAxes.MouseXAndY)
				{
					float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
					
					rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
					rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
					
					transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
				}
				else if (axes == RotationAxes.MouseX)
				{
					transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
				}
				else
				{
					rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
					rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
					
					transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
				}
			}
			#endregion
			
			transform.position = player.transform.position;
			
			if (Input.GetMouseButtonDown(1) && player.isAlive && !player.endGame) {
				Fire_Gravity_Bullet();
			}

			if (Input.GetMouseButtonDown(0) && player.isAlive && !player.endGame && !BasicFunctions.ForkModus) {
				Fire_Kill_Laser();
			}
		}
	}
	
	void OnGUI()
	{
		if (networkView.isMine || BasicFunctions.playOffline)
		{
			float xMin = (Screen.width / 2) - (crosshairImage.width / 2);
			float yMin = (Screen.height / 2) - (crosshairImage.height / 2);
			GUI.DrawTexture(new Rect(xMin, yMin, crosshairImage.width, crosshairImage.height), crosshairImage);
		}
	}
}