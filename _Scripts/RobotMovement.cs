using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotMovement : MonoBehaviour {
	public Texture frame1;
	public Texture frame2;
	public Texture frame3;
	public Texture frame4;
	public Texture frame5;
	public Texture frame6;

	public float frames_per_image = 3;
	private float frame_counter;
	private float current_frame = 1;

	private Pathfinder pathfind;
	private RobotScript robotscript;

	private List<Node> path;

	private Vector3 start;
	private Vector3 end = new Vector3(10,10,10);
	private float length;
	private float startTime;
	public float speed;
	public float rotSpeed;
	private int target;

	public bool reset = false;
	public bool arrived = false;
	public bool moving = false;
	public bool rotating = false;
	public bool rotatingcompleted = true;
	public bool movinginitiate = false;
	public bool rotatinginit = false;
	public bool destroyTarget = false;
	public List<Vector3> vectorPath = new List<Vector3>();

	private float dx;
	private float dy;
	private float dz;
	private float timer;

	void Start()
	{
		frame_counter = frames_per_image;
		reset = false;
		pathfind = this.GetComponent<Pathfinder>();
		robotscript = this.GetComponent<RobotScript>();
		rotatingcompleted = true;
		movinginitiate = false;
		rotatinginit = false;
		destroyTarget = false;
		timer = 0;
	}

	void FixedUpdate()
	{
		if(Network.isServer)
		{
			if(reset)
			{
				path = pathfind.path;
				string pathstring = routeToString (path);
				networkView.RPC ("resetFunction",RPCMode.All,pathstring);
			}
		}

		if(!rotatingcompleted)
		{
			dx = end.x - start.x;
			dy = end.y - start.y;
			dz = end.z - start.z;
			
			if(Mathf.Abs(dx)>0 || Mathf.Abs(dy)>0 || Mathf.Abs (dz)>0)
			{
				rotating = true;
				rotatinginit = true;
				moving = false;	
			}		
		}

		if(movinginitiate)
		{
			startTime = Time.time;
			movinginitiate = false;
		}

		if(moving)
		{
			float distCovered = (Time.time - startTime) * speed;
			float fracJourney = distCovered / length;

			if (vectorPath.Count>2)
			{
				this.transform.position = Vector3.Lerp(start, end, fracJourney);
			}

			if (target>=(vectorPath.Count-2) && fracJourney>0.90)
			{
				destroyTarget = true;
				moving = false;
				rotating = false;
				moving = false;
				rotatingcompleted = true;
			}

			if (fracJourney>0.90 && target<(vectorPath.Count-2))
			{
				selectNext ();
				rotatingcompleted = false;
			}
		}

		if (rotating == true && moving == false)
		{
			if (rotatinginit == true)
			{

			}
			Quaternion tolerp = Quaternion.LookRotation(new Vector3(dx*90,dy*90,dz*90),Vector3.up);
			this.transform.rotation = Quaternion.Slerp (this.transform.rotation,tolerp,Time.deltaTime*rotSpeed);
			if (Quaternion.Angle (this.transform.rotation,tolerp)<20)
			{
				moving = true;
				rotating = true;
				rotatingcompleted = true;
				movinginitiate = true;

			}
		}

		if (rotating == true && moving == true)
		{
			if (rotatinginit == true)
			{
				
			}
			Quaternion tolerp = Quaternion.LookRotation(new Vector3(dx*90,dy*90,dz*90),Vector3.up);
			this.transform.rotation = Quaternion.Slerp (this.transform.rotation,tolerp,Time.deltaTime*rotSpeed);
			if (Quaternion.Angle (this.transform.rotation,tolerp)<1)
			{
				moving = true;
				rotating = false;
				rotatingcompleted = true;
				
			}
		}

		if(destroyTarget == true)
		{
			if (frame_counter == 0)
			{
				frame_counter = frames_per_image;
				if (current_frame == 1)
				{
					transform.GetChild(0).transform.renderer.material.mainTexture = frame1;
					current_frame = 2;
				}
				else if (current_frame == 2)
				{
					transform.GetChild(0).transform.renderer.material.mainTexture = frame2;
					current_frame = 3;
				}
				else if (current_frame == 3)
				{
					transform.GetChild(0).transform.renderer.material.mainTexture = frame3;
					current_frame = 4;
				}
				else if (current_frame == 4)
				{
					transform.GetChild(0).transform.renderer.material.mainTexture = frame4;
					current_frame = 5;
				}
				else if (current_frame == 5)
				{
					transform.GetChild(0).transform.renderer.material.mainTexture = frame5;
					current_frame = 6;
				}
				else
				{
					transform.GetChild(0).transform.renderer.material.mainTexture = frame6;
					current_frame = 1;
				}
			}
			else
				frame_counter--;

			Vector3 relativePos = vectorPath[vectorPath.Count-1] - transform.position;
			Quaternion tolerp = Quaternion.LookRotation(relativePos,Vector3.up);
			this.transform.rotation = Quaternion.Slerp (this.transform.rotation,tolerp,Time.deltaTime*rotSpeed);
			if (Quaternion.Angle (this.transform.rotation,tolerp)<5 && Network.isServer)
			{
				timer += Time.deltaTime;
				if (timer>4)
				{
					robotscript.target.SendMessage("Kill");	
					timer = 0;
					destroyTarget = false;
					moving = false;
					reset = false;
					rotatingcompleted = true;
					rotating = false;
				}
			}
		}
	}


	void selectNext()
	{
		target++;
		start = vectorPath[target-1];
		end = vectorPath[target];
		movinginitiate = true;
	}

	[RPC]
	void resetFunction(string inpath)
	{
		if(Network.isServer)
		{
			pathfind.tracedBack = false;
		}
		moving = true;
		reset = true;
		rotatingcompleted = false;
		destroyTarget = false;

		vectorPath = routeParser (inpath);
		start = vectorPath[0];
		end = vectorPath[1];
		target = 1;
		reset = false;
		length = Vector3.Distance(start, end);
	}

	string routeToString(List<Node> inpath)
	{
		string temp = System.String.Empty;
		for( int i=path.Count-1;i>=0;i--)
		{
			temp = temp + inpath[i].xPosition + "," + inpath[i].yPosition + "," + inpath[i].zPosition + ";";
		}
		return temp;
	}

	List<Vector3> routeParser(string instring)
	{
		vectorPath.Clear ();
		string[] positions = instring.Split(';');
		string[] coords;
		Vector3 tempCoord;
		for (int i=0;i<positions.Length-1;i++)
		{
			coords = positions[i].Split(',');
			tempCoord = new Vector3(float.Parse(coords[0]),float.Parse(coords[1]),float.Parse (coords[2]));
			vectorPath.Add(tempCoord);
		}
		return vectorPath;
	}
}
