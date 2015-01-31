using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicFunctions : MonoBehaviour {

	public static bool playOffline;
	public static bool firstStart = true;
	public static bool ForkModus = true;
	public static bool JumpModus = true;
	public static bool loginServer;
	public static bool MusicOn = true;
	public static int amountPlayers;
	public static int maxPoints = 10;
	public static int maxRobots = 3;
	public static float Sensitivity = 10f;
	public static Account activeAccount;
	public static List<string> activeAccounts = new List<string>();
	public static List<string> startingAccounts = new List<string>();
	public static List<int> accountNumbers = new List<int>();

	void Awake ()
	{
		DontDestroyOnLoad(this);
	}

	public static Vector3 ProjectVectorOnPlane(Vector3 Normal, Vector3 Vector){
		//De projectie is de oorspronkelijke vector minus de component loodrecht op de plane en dus aan de normal
		return Vector - (Vector3.Dot(Vector, Normal) * Normal);
	}

}
