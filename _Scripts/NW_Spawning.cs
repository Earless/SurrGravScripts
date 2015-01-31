using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class NW_Spawning : MonoBehaviour {

	public GameObject playerForkPrefab;
	public GameObject playerRailPrefab;
	public GameObject refereePrefab;
	public GameObject serverQuitText;
	
	public Texture Player1F;
	public Texture Player2F;
	public Texture Player3F;
	public Texture Player4F;
	public Texture Player1R;
	public Texture Player2R;
	public Texture Player3R;
	public Texture Player4R;

	public Copy_LevelCreator levelCreator;

	public List<Vector3> spawnLocations;
	public List<Vector3> respawnLocations;

	public bool serverHasQuit = false;

	private GameObject player;
	private Vector3 randomSpawnPoint;
	private bool playOffline;
	private bool canSpawn;
	private bool canInitLevel = true;
	private int amountSpawnPoints = 20;
	private Referee_script refScript;
	private ScoreScreen scoreScreen;

	void Awake ()
	{
		if (!BasicFunctions.playOffline)
		{
			if (Network.isServer)
			{
				Network.Instantiate (refereePrefab, new Vector3(0,0,0), Quaternion.identity, 0); //Spawn referee
			}
		}
	}

	void Start ()
	{
		spawnLocations = new List<Vector3> ();
		respawnLocations = new List<Vector3> ();
		canSpawn = true;
	}
	
	public void spawnPlayer ()
	{
		int index = Random.Range (0, spawnLocations.Count-1); //Take random integer
		randomSpawnPoint = spawnLocations[index]; //Pick random spawnpoint (because of random int)

		if (BasicFunctions.playOffline)
		{
			Object.Instantiate (playerRailPrefab, randomSpawnPoint, Quaternion.identity);
		}
		else
		{
			GameObject playerN;
			if (BasicFunctions.ForkModus)
			{
				playerN = Network.Instantiate (playerForkPrefab, randomSpawnPoint, Quaternion.identity, 0) as GameObject; //Instantiate player on the spawn point
			}
			else
			{
				playerN = Network.Instantiate (playerRailPrefab, randomSpawnPoint, Quaternion.identity, 0) as GameObject; //Instantiate player on the spawn point
			}
			player = playerN;
			networkView.RPC("removeSpawnPoint", RPCMode.All, index);
			networkView.RPC("setNumbers", RPCMode.All, playerN.networkView.viewID, BasicFunctions.activeAccount.Name, BasicFunctions.activeAccount.Word, BasicFunctions.activeAccount.Number);
		}
	}
	/* Leave game gracefully as client
	 */ 
	public void closeClient (bool serverQuit, bool endGame)
	{
		if (!endGame)
		{
			networkView.RPC("changeEntry", RPCMode.Others, BasicFunctions.activeAccount.Number);
		}
		if (!serverQuit)
		{
			networkView.RPC("makePlayerInvis", RPCMode.All, player.networkView.viewID);
			BasicFunctions.amountPlayers = 0;
			BasicFunctions.activeAccounts.Clear ();
			BasicFunctions.accountNumbers.Clear ();
			Network.Disconnect();
		}
		playerController.dontDestroy = true;
		Application.LoadLevel("Menu_New");
	}
	/* Close the server gracefully
	 */
	public void closeServer (bool gameEnded)
	{
		if (BasicFunctions.amountPlayers > 1)
		{
			networkView.RPC("clearAccountsInGame", RPCMode.All);
			networkView.RPC("quitGame", RPCMode.Others, gameEnded);
			MasterServer.UnregisterHost();
			Network.Disconnect(); 
			Application.LoadLevel("Menu_New");
		}
		else
		{
			BasicFunctions.amountPlayers = 0;
			BasicFunctions.activeAccounts.Clear();
			BasicFunctions.startingAccounts.Clear();
			BasicFunctions.accountNumbers.Clear();
			MasterServer.UnregisterHost();
			Network.Disconnect(); 
			Application.LoadLevel("Menu_New");
		}
	}
	/* Fill spawnpositionvector
	 */
	[RPC]
	public void addSpawnPoints (Vector3 spawnPos)
	{
		spawnLocations.Add (spawnPos);
		respawnLocations.Add (spawnPos);
	}

	[RPC]
	public void removeSpawnPoint (int index)
	{
		spawnLocations.RemoveAt(index);
	}
	/* Assign number to a player
	 */
	[RPC]
	public void setNumbers (NetworkViewID player, string Uname, string Pword, int Number)
	{
		NetworkView playerN = NetworkView.Find(player);
		playerN.GetComponent<playerController>().activeAccount = new Account(Uname, Pword);
		playerN.GetComponent<playerController>().playerNumber = Number;
		if (BasicFunctions.ForkModus)
		{
			switch (Number)
			{
			case 1:
				playerN.transform.Find ("Circle").renderer.material.SetTexture("_MainTex", Player1F);
				break;
			case 2: 
				playerN.transform.Find ("Circle").renderer.material.SetTexture("_MainTex", Player2F);
				break;
			case 3: 
				playerN.transform.Find ("Circle").renderer.material.SetTexture("_MainTex", Player3F);
				break;
			case 4: 
				playerN.transform.Find ("Circle").renderer.material.SetTexture("_MainTex", Player4F);
				break;
			default : 
				Debug.Log("DIKKE TERROR ERROR");
				break;
			}
		}
		else
		{
			switch (Number)
			{
			case 1:
				playerN.transform.Find ("Circle").renderer.material.SetTexture("_MainTex", Player1R);
				break;
			case 2: 
				playerN.transform.Find ("Circle").renderer.material.SetTexture("_MainTex", Player2R);
				break;
			case 3: 
				playerN.transform.Find ("Circle").renderer.material.SetTexture("_MainTex", Player3R);
				break;
			case 4: 
				playerN.transform.Find ("Circle").renderer.material.SetTexture("_MainTex", Player4R);
				break;
			default : 
				Debug.Log("DIKKE TERROR ERROR");
				break;
			}
		}
	}

	[RPC]
	public void quitGame (bool gameEnded)
	{
		serverHasQuit = true;
		if (!gameEnded)
		{
			serverQuitText.SetActive(true);
			if (!refScript)
			{
				refScript = (GameObject.FindGameObjectsWithTag("Referee_Tag"))[0].GetComponent<Referee_script>();
			}
			refScript.EndGameQuit();
		}
	}

	[RPC]
	void clearAccountsInGame ()
	{
		BasicFunctions.amountPlayers = 0;
		BasicFunctions.activeAccounts.Clear();
		BasicFunctions.startingAccounts.Clear();
		BasicFunctions.accountNumbers.Clear();
	}

	[RPC]
	void changeEntry (int Number)
	{
		if (!scoreScreen)
		{
			scoreScreen = GameObject.FindGameObjectWithTag("ScoreScreen").GetComponent<ScoreScreen>();
		}
		scoreScreen.changeEntry(Number);
	}
	/* Set the amount of players currently connected
	 */
	[RPC]
	void setAmountPlayers (bool up)
	{
		if (up)
			BasicFunctions.amountPlayers += 1;
		else 
			BasicFunctions.amountPlayers -= 1;
	}

	[RPC]
	void makePlayerInvis (NetworkViewID ID)
	{
		NetworkView quitterView = NetworkView.Find (ID);
		GameObject quitter = quitterView.gameObject;
		Destroy(quitter);
	}

	void Update ()
	{
		if (levelCreator.gridinitialised && canInitLevel)
		{
			for (int i = 0; i < amountSpawnPoints; i++)
			{
				Vector3 spawn = levelCreator.getSpawn();//new Vector3 (width, height, depth);
				if (Network.isServer)
				{
					networkView.RPC("addSpawnPoints", RPCMode.All, spawn);
				}
				else if (BasicFunctions.playOffline)
				{
					spawnLocations.Add (spawn);
				}
			}
			
			if (BasicFunctions.playOffline)
			{
				spawnPlayer();
			}
			canInitLevel = false;
		}


		if(!BasicFunctions.playOffline && canSpawn)
		{
			if (GameObject.FindGameObjectsWithTag("Player").Length == (BasicFunctions.activeAccount.Number-1))
			{
				spawnPlayer();
				canSpawn = false;
			}
		}
	}
}