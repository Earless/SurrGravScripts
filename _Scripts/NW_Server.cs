using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class NW_Server : MonoBehaviour {

	public MenuFunctions menuFunct;
	
	public Text p1;
	public Text p2;
	public Text p3;
	public Text p4;
	public Text p1c;
	public Text p2c;
	public Text p3c;
	public Text p4c;
	public GUISkin skin;

	private GUIStyle label2;

	private string gameTypeName = "Surreal_Gravity: The Game";
	private string gameName;

	public bool connected = false;

	public static bool showServers;

	private bool ClientConn = false;
	private bool ClientUpdate = false;
	private bool refreshing = false;
	private float timer = 0.5f;
	private float refreshT = 2f;
	private float waitRefresh = 3f;
	private int serverPort;
	private int index;
	private int maxPlayers = 3;//(for now only 1) 3 //maxPlayers = # of clients
	private List<Text> serverPlayers;
	private List<Text> clientPlayers;

	private HostData[] hostD;

	void Start ()
	{
		showServers = false;
		Screen.lockCursor = false;
		serverPlayers = new List<Text>();
		clientPlayers = new List<Text>();
		serverPlayers.Add(p1);
		serverPlayers.Add(p2);
		serverPlayers.Add(p3);
		serverPlayers.Add(p4);
		clientPlayers.Add(p1c);
		clientPlayers.Add(p2c);
		clientPlayers.Add(p3c);
		clientPlayers.Add(p4c);
	}

	public void startServer ()
	{
		gameName = BasicFunctions.activeAccount.Name + "'s Server";
		serverPort = Random.Range (0, 30000); //Take random integer
		bool NAT = !Network.HavePublicAddress();
		Network.InitializeServer (4, serverPort, NAT); //Initialiseer Server; max connecties  = 4
		Network.maxConnections = maxPlayers;
		MasterServer.RegisterHost (gameTypeName, gameName, "SurrealGravity_FinalGame"); //Registreer de Server
	}

	public void closeServer ()
	{
		networkView.RPC("clearAccounts", RPCMode.All);
		showServers = false;
		clearTexts (true);
		MasterServer.UnregisterHost();
		Network.Disconnect();
	}

	public void refreshHost ()
	{
		MasterServer.RequestHostList (gameTypeName);
		refreshing = true;
	}

	public void closeClient ()
	{
		networkView.RPC("deleteUNServer", RPCMode.Server, BasicFunctions.activeAccount.Name, BasicFunctions.activeAccount.Number);
		networkView.RPC ("assignNumbers", RPCMode.Others);
		clearTexts(false);
		BasicFunctions.amountPlayers = 0;
		BasicFunctions.activeAccounts.Clear ();
		BasicFunctions.accountNumbers.Clear ();
		Network.Disconnect();
		BasicFunctions.playOffline = false;
	}

	public void startGame ()
	{
		if (Network.isServer)
		{
			Network.maxConnections = BasicFunctions.amountPlayers - 1; //Players can't connect midgame
			networkView.RPC("beginGame", RPCMode.AllBuffered);
		}
	}

	void OnServerInitialized ()
	{
		BasicFunctions.amountPlayers = 0;
		Debug.Log ("Server: " + gameName + " registered");
		ClientConn = true;
	}

	void OnConnectedToServer ()
	{
		ClientConn = true;
	}

	void OnDisconnectedFromServer ()
	{
		Debug.Log ("Server disconnected");
		hostD = null;
		menuFunct.Client_Menu.SetActive (false);
		menuFunct.Multiplayer_Menu.SetActive (true);
	}

	public void ClientIsConnected ()
	{
		if (BasicFunctions.amountPlayers == 0)
		{
			BasicFunctions.activeAccount.Number = 1;
			networkView.RPC ("setAmountPlayers", RPCMode.AllBuffered, true); //Verhoog het aantal spelers
			BasicFunctions.accountNumbers.Add(BasicFunctions.activeAccount.Number); //this.AccManager.activeAccount.Number);
			BasicFunctions.activeAccounts.Add(BasicFunctions.activeAccount.Name); //this.AccManager.activeAccount.Name); //Zet username in de lijst
			BasicFunctions.startingAccounts.Add(BasicFunctions.activeAccount.Name);
			setTextsS();
		}
		else
		{
			menuFunct.goToClient();
			BasicFunctions.activeAccount.Number = BasicFunctions.amountPlayers + 1;
			networkView.RPC ("setAmountPlayers", RPCMode.AllBuffered, true); //Verhoog het aantal spelers
			networkView.RPC("sendUNtoServer", RPCMode.Server, BasicFunctions.activeAccount.Name, BasicFunctions.activeAccount.Number); //Geef je username mee aan de Server
		}
	}

	public void clearTexts (bool server)
	{
		if (server)
		{
			p1.text = "Self";
			p2.text = "Open";
			p3.text = "Open";
			p4.text = "Open";
		}
		else
		{
			p1c.text = "Server";
			p2c.text = "Open";
			p3c.text = "Open";
			p4c.text = "Open";
		}
	}

	[RPC]
	public void beginGame ()
	{
		BasicFunctions.firstStart = false;
		BasicFunctions.playOffline = false;
		Application.LoadLevel("Main_Game");
	}

	[RPC]
	public void clearAccounts ()
	{
		BasicFunctions.amountPlayers = 0;
		BasicFunctions.activeAccounts.Clear();
		BasicFunctions.startingAccounts.Clear();
		BasicFunctions.accountNumbers.Clear();
		if (Network.isClient)
		{
			clearTexts (false);
			Network.Disconnect();
		}
	}
	/* Stuur UI data naar de Server
	 */
	[RPC]
	public void sendUNtoServer (string UN, int Number)
	{
		if (Network.isServer)
		{
			if (!BasicFunctions.activeAccounts.Contains(UN))
			{
				BasicFunctions.activeAccounts.Add(UN);
			}
			else
			{
				BasicFunctions.activeAccounts.Add (UN + "[" + Number + "]");
			}
			BasicFunctions.startingAccounts.Add(UN);
			BasicFunctions.accountNumbers.Add (Number);
			
			for(int i = 0; i < BasicFunctions.activeAccounts.Count; i++)
			{
				Account adding = new Account (BasicFunctions.activeAccounts[i], "");
				adding.Number = BasicFunctions.accountNumbers[i];
				setTextsS ();
				networkView.RPC("sendUNtoClients", RPCMode.Others, BasicFunctions.activeAccounts[i], BasicFunctions.accountNumbers[i]);
			}
		}
	}
	/* Stuur UI data naar de Clients
	 */
	[RPC]
	public void sendUNtoClients (string UN, int Number)
	{
		if (Network.isClient)
		{
			if (!BasicFunctions.activeAccounts.Contains(UN))
			{
				BasicFunctions.activeAccounts.Add(UN);
				BasicFunctions.startingAccounts.Add(UN);
				BasicFunctions.accountNumbers.Add(Number);
			}
			for (int i = 0; i < BasicFunctions.activeAccounts.Count; i++)
			{
				Debug.Log("" + i + ": " + BasicFunctions.activeAccounts[i]);
				Debug.Log("" + i + ": " + BasicFunctions.accountNumbers[i]);
			}
		
			if (BasicFunctions.activeAccounts.Count == BasicFunctions.amountPlayers)
			{
				setTextsC();
			}
		}
	}

	[RPC]
	public void deleteUNServer (string UN, int Number)
	{
		if (Network.isServer)
		{
			BasicFunctions.activeAccounts.Remove(UN);
			BasicFunctions.startingAccounts.Remove(UN);
			BasicFunctions.accountNumbers.Remove(Number);
			networkView.RPC ("setAmountPlayers", RPCMode.AllBuffered, false);
			clearTexts (true);
			setTextsS ();
			networkView.RPC("deleteUNClients", RPCMode.Others, UN, Number);
		}
	}

	[RPC]
	public void deleteUNClients (string UN, int Number)
	{
		if (Network.isClient)
		{
			BasicFunctions.activeAccounts.Remove(UN);
			BasicFunctions.startingAccounts.Remove(UN);
			BasicFunctions.accountNumbers.Remove(Number);
			clearTexts(false);
			//setTextsC();
		}
	}

	[RPC]
	public void assignNumbers ()
	{
		for (int i = 0; i < BasicFunctions.amountPlayers; i++)
		{
			if (BasicFunctions.activeAccounts[i].Equals(BasicFunctions.activeAccount.Name))
			{
				BasicFunctions.activeAccount.Number = i+1;
				setTextsC();
			}
		}
	}
	/* Zet de text op de Server
	 */
	public void setTextsS ()
	{
		for (int j = 0; j < BasicFunctions.amountPlayers; j++)
		{
			if (j == 0)
			{
				serverPlayers[j].text = "-> " + BasicFunctions.activeAccounts[j];
			}
			else
			{
				serverPlayers[j].text = BasicFunctions.activeAccounts[j];
			}
		}
	}
	/* Zet de text op de Clients
	 */
	public void setTextsC ()
	{
		for (int k = 0; k < BasicFunctions.amountPlayers; k++)
		{
			if (k == (BasicFunctions.activeAccount.Number - 1))
			{
				clientPlayers[k].text = "-> " + BasicFunctions.activeAccounts[k];
			}
			else
			{
				clientPlayers[k].text = BasicFunctions.activeAccounts[k];
			}
		}
	}

	[RPC]
	public void setAmountPlayers (bool up)
	{
		if (up)
			BasicFunctions.amountPlayers += 1;
		else 
			BasicFunctions.amountPlayers -= 1;
	}

	void Update ()
	{
		if (refreshing)
		{
			refreshT -= Time.deltaTime;
			if (refreshT <= 0)
			{
				waitRefresh = 1;
				refreshing = false;
				hostD = MasterServer.PollHostList ();
				showServers = true;
			}
		}

		if (!refreshing && NW_Server.showServers)
		{
			waitRefresh -= Time.deltaTime;
			if (waitRefresh <= 0)
			{
				refreshT = 1;
				refreshHost();
			}
		}

		if (ClientConn)
		{
			timer -= Time.deltaTime;
			if (timer <= 0)
			{
				timer = 0.5f;
				ClientConn = false;
				ClientUpdate = true;
			}
		}
		
		if (ClientUpdate)
		{
			ClientIsConnected();
			connected = true;
			ClientUpdate = false;
		}
	}

	void OnGUI ()
	{
		GUI.skin = skin;
		label2 = skin.GetStyle("label2");
		GUI.backgroundColor = new Color(0.118F, 1, 0.729F, 1);
		if (!Network.isClient && !Network.isServer)
		{
			if (showServers)
			{
				if (hostD != null)
				{
					for (int i = 0; i < hostD.Length; i++)
					{
						if (hostD[i].connectedPlayers != hostD[i].playerLimit)
						{
							if (GUI.Button(new Rect(44*Screen.width/100, Screen.height/2 + (i * 50) - 25, Screen.width*0.12f, Screen.height*0.06f), hostD[i].gameName))
							{
								Network.Connect(hostD[i]);
							}
						}
						else
						{
							GUI.Label(new Rect(46*Screen.width/100, Screen.height/2 + (i * 50) - 15, Screen.width*0.12f, Screen.height*0.06f), hostD[i].gameName, label2);
						}
						GUI.Label(new Rect(57*Screen.width/100, Screen.height/2 + (i * 50) - 25, Screen.width*0.12f, Screen.height*0.06f), (hostD[i].connectedPlayers) + "/" + (hostD[i].playerLimit)); 
					}
				}
			}
		}
	}
}
