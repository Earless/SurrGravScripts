using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class MenuFunctions : MonoBehaviour {

	public GameObject Main_Menu;
	public GameObject Multiplayer_Menu;
	public GameObject Server_Menu;
	public GameObject Client_Menu;
	public GameObject Account_Menu;
	public GameObject Stats_Menu;
	public GameObject Control_Menu;
	public GameObject Settings_Menu;
	public GameObject Credits_Menu;
	public GameObject CantMulti_Text;

	public Text currentUname;
	public Text statsUname;
	public Toggle chooseModus;
	public Toggle chooseJump;
	public Toggle Music_button;
	public Toggle chooseLoginModus;
	public Text Music_text;
	public Text chooseModusLabelServer;
	public Text chooseModusLabelClient;
	public Text chooseJumpLabelServer;
	public Text chooseJumpLabelClient;
	public Text chooseLoginLabel;
	public Text robotsText;
	public Text pointsText;
	public Text SensitivityText;
	public Text clientRobotsText;
	public Text clientPointsText;
	public Text errorMessage;
	public InputField login;
	public InputField reg1;
	public InputField reg2;
	public Slider robotsSlider;
	public Slider pointsSlider;
	public Slider sensitivitySlider;
	public Button Multiplayer_Button;
	public Text Multiplayer_Text;
	public Text TotalPlayed;
	public Text MostPlayed;
	public Text MostWon;
	public Text AmountPlayed;
	public Text AmountWon;
	public Text WLRatio;

	public bool isLoggedIn = false;

	public AccountManagement AccManager;
	public NW_Server serverStuff;

	public AudioClip menu_click_sound;

	private bool setClientMenu = true;
	private bool canTabL = false;
	private bool canTabR1 = false;
	private bool canTabR2 = false;
	private bool canLogin = false;
	private bool canReg = false;

	void Start ()
	{
		if (BasicFunctions.firstStart)
		{
			BasicFunctions.ForkModus = true;
		}

		Screen.showCursor = true;
		Screen.lockCursor = false;

		using (StreamReader sread = new StreamReader("Settings.txt"))
		{
			sread.ReadLine();
			BasicFunctions.Sensitivity = float.Parse(sread.ReadLine());
			Debug.Log("Sens: " + BasicFunctions.Sensitivity);
			sread.Close();
		}
	}

	public void goToMultiplayer ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		if (AccountManagement.loggedIn)
		{
			BasicFunctions.ForkModus = true;
			BasicFunctions.JumpModus = true;
			Main_Menu.SetActive(false);
			Multiplayer_Menu.SetActive(true);
			NW_Server.showServers = true;
		}
	}

	public void goToSingleplayer ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		BasicFunctions.playOffline = true;
		BasicFunctions.ForkModus = false;
		BasicFunctions.firstStart = false;
		Application.LoadLevel("Main_Game");
	}

	public void goToServer ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		serverStuff.startServer();
		Multiplayer_Menu.SetActive(false);
		Server_Menu.SetActive(true);
	}

	public void goToClient ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		Multiplayer_Menu.SetActive(false);
		Client_Menu.SetActive(true);
	}

	public void goToAccount ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5f);
		Main_Menu.SetActive(false);
		Account_Menu.SetActive(true);
		if (BasicFunctions.activeAccount != null)
			currentUname.text = BasicFunctions.activeAccount.Name;
		if (BasicFunctions.loginServer)
		{
			chooseLoginModus.isOn = true;
		}
		else
		{
			chooseLoginModus.isOn = false;
		}
	}

	public void goToStats ()
	{
		string urlGlobal = "http://drproject.twi.tudelft.nl:8082/UnityGlobalInfo";
		WWW www = new WWW (urlGlobal);
		StartCoroutine (WaitForGlobalStats (www));

		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.1F);
		if (BasicFunctions.activeAccount != null){

		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		if (BasicFunctions.activeAccount != null)
			statsUname.text = BasicFunctions.activeAccount.Name + ":";
			string urlAccount = "http://drproject.twi.tudelft.nl:8082/UnityAccountInfo?playerName="+BasicFunctions.activeAccount.Name;
			WWW wwwAccount = new WWW(urlAccount);
			StartCoroutine(WaitForAccountStats(wwwAccount));		
		}
		Main_Menu.SetActive(false);
		Stats_Menu.SetActive(true);
	}

	public void goToSettings ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		Main_Menu.SetActive(false);
		Settings_Menu.SetActive(true);
		if (BasicFunctions.MusicOn)
		{
			Music_button.isOn = true;
		}
		else
		{
			Music_button.isOn = false;
		}
		sensitivitySlider.value = BasicFunctions.Sensitivity;
		SensitivityText.text = "" + sensitivitySlider.value;
	}

	public void goToControls ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		Settings_Menu.SetActive(false);
		Control_Menu.SetActive(true);
	}

	public void goToCredits ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		Settings_Menu.SetActive(false);
		Credits_Menu.SetActive(true);
	}

	public void Refresh ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		serverStuff.refreshHost();
	}

	public void ChooseModus ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		if (Network.isServer)
		{
			if (chooseModus.isOn)
			{
				chooseModusLabelServer.text = "Fork";
				networkView.RPC("SetGameMode", RPCMode.AllBuffered, true);
				networkView.RPC("SetGameModeText", RPCMode.OthersBuffered, true);
			}
			else
			{
				chooseModusLabelServer.text = "RailGun";
				networkView.RPC("SetGameMode", RPCMode.AllBuffered, false);
				networkView.RPC("SetGameModeText", RPCMode.OthersBuffered, false);
			}
		}
	}

	public void ChooseJump ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		if (Network.isServer)
		{
			if (chooseJump.isOn)
			{
				chooseJumpLabelServer.text = "Jump";
				networkView.RPC("SetJumpMode", RPCMode.AllBuffered, true);
				networkView.RPC("SetJumpModeText", RPCMode.OthersBuffered, true);
			}
			else
			{
				chooseJumpLabelServer.text = "No-Jump";
				networkView.RPC("SetJumpMode", RPCMode.AllBuffered, false);
				networkView.RPC("SetJumpModeText", RPCMode.OthersBuffered, false);
			}
		}
	}

	public void Music_On_OFF ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		if (Music_button.isOn)
		{
			Music_text.text = "ON";
			BasicFunctions.MusicOn = true;
		}
		else
		{
			Music_text.text = "OFF";
			BasicFunctions.MusicOn = false;
		}
	}

	public void ChooseLoginMode ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		if (chooseLoginModus.isOn)
		{
			chooseLoginLabel.text = "Webserver";
			BasicFunctions.loginServer = true;
		}
		else if (!chooseLoginModus.isOn)
		{
			chooseLoginLabel.text = "Local";
			BasicFunctions.loginServer = false;
		}
	}

	public void StartGame ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		serverStuff.startGame();
	}

	public void Disconnect ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		if (Network.isServer)
		{
			serverStuff.closeServer();
			NW_Server.showServers = true;
			Server_Menu.SetActive(false);
			Multiplayer_Menu.SetActive(true);
		}
		if (Network.isClient)
		{
			serverStuff.closeClient();
			NW_Server.showServers = true;
			Client_Menu.SetActive(false);
			Multiplayer_Menu.SetActive(true);
		}
	}

	public void Back ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		errorMessage.gameObject.SetActive(false);
		Account_Menu.SetActive(false);
		Multiplayer_Menu.SetActive(false);
		Stats_Menu.SetActive(false);
		Settings_Menu.SetActive(false);
		NW_Server.showServers = false;
		Main_Menu.SetActive(true);
	}

	public void BackSettings ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		Control_Menu.SetActive(false);
		Credits_Menu.SetActive(false);
		Settings_Menu.SetActive(true);
	}

	public void QuitGame ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		Application.Quit ();
	}

	public void Register ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		AccManager.registerAccount();
	}
	
	public void Login ()
	{
		AudioSource.PlayClipAtPoint(menu_click_sound, transform.position, 0.5F);
		AccManager.loginAccount();
	}

	public void setRobots (float robots)
	{
		robotsText.text = "" + robots;
		networkView.RPC("SetMaxRobots", RPCMode.AllBuffered, (int) robots);
		networkView.RPC("SetRobotsText", RPCMode.OthersBuffered, (int) robots);
	}

	public void setPoints (float points)
	{
		pointsText.text = "" + points;
		networkView.RPC("SetMaxPoints", RPCMode.AllBuffered, (int) points);
		networkView.RPC("SetPointsText", RPCMode.OthersBuffered, (int) points);
	}

	public void setMouseSensitivity (float points)
	{
		SensitivityText.text = "" + points;
		BasicFunctions.Sensitivity = points;

		using (StreamWriter swrite = new StreamWriter ("Settings.txt", false))
		{
			string sens = points.ToString();
			swrite.WriteLine ("Sensitivity");
			swrite.WriteLine (sens);
			swrite.Close ();
		}
	}

	public void setTabL ()
	{
		canTabL = true;
		canTabR1 = false;
		canTabR2 = false;
		canLogin = true;
		canReg = false;
	}

	public void setTabR1 ()
	{
		canTabR1 = true;
		canTabL = false;
		canTabR2 = false;
		canReg = true;
		canLogin = false;
	}

	public void setTabR2 ()
	{
		canTabR2 = true;
		canTabL = false;
		canTabR1 = false;
		canReg = true;
		canLogin = false;
	}

	void Update ()
	{
		if (!BasicFunctions.playOffline && serverStuff.connected)
		{
			if (Network.isClient && setClientMenu)
			{
				Multiplayer_Menu.SetActive(false);
				Client_Menu.SetActive(true);
				setClientMenu = false;
			}
		}

		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (canTabL)
			{
				login.Select ();
			}
			else if (canTabR1)
			{
				reg1.Select ();
			}
			else if (canTabR2)
			{
				reg2.Select ();
			}
		}
		else if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
		{
			if (canLogin)
			{
				Login();
			}
			else if (canReg)
			{
				Register();
			}
		}
		if (isLoggedIn)
		{
			isLoggedIn = false;
			Account_Menu.SetActive(false);
			Main_Menu.SetActive(true);
		}
		if (AccountManagement.loggedIn)
		{
			CantMulti_Text.SetActive(false);
			Multiplayer_Button.interactable = true;
			Multiplayer_Text.color = new Color(0.118f, 1, 0.729f, 1);
		}
	}

	[RPC]
	public void SetGameMode (bool fork)
	{
		if (fork)
		{
			BasicFunctions.ForkModus = true;
		}
		else
		{
			BasicFunctions.ForkModus = false;
		}
	}

	[RPC]
	public void SetGameModeText (bool fork)
	{
		if (fork)
		{
			chooseModusLabelClient.text = "Fork";
		}
		else
		{
			chooseModusLabelClient.text = "RailGun";
		}
	}

	[RPC]
	public void SetJumpMode (bool jump)
	{
		if (jump)
		{
			BasicFunctions.JumpModus = true;
		}
		else
		{
			BasicFunctions.JumpModus = false;
		}
	}
	
	[RPC]
	public void SetJumpModeText (bool jump)
	{
		if (jump)
		{
			chooseJumpLabelClient.text = "Off";
		}
		else
		{
			chooseJumpLabelClient.text = "On";
		}
	}

	[RPC]
	public void SetMaxRobots (int robots)
	{
		BasicFunctions.maxRobots = robots;
	}

	[RPC]
	public void SetMaxPoints (int points)
	{
		BasicFunctions.maxPoints = points;
	}

	[RPC]
	public void SetRobotsText (int robots)
	{
		clientRobotsText.text = "" + robots;
	}

	[RPC]
	public void SetPointsText (int points)
	{
		clientPointsText.text = "" + points;
	}

	IEnumerator WaitForGlobalStats(WWW www){
		if (www.error == null)
		{
			yield return www;
			string[] global_data = www.text.Split(',');
			TotalPlayed.text = global_data[0];
			MostPlayed.text = global_data[1] + " (" + global_data[2] + ")";
			MostWon.text = global_data[3] + " (" + global_data[4] + ")";
		}
	}

	IEnumerator WaitForAccountStats(WWW www){
		if (www.error == null)
		{
			yield return www;
			string[] local_data = www.text.Split(',');
			AmountPlayed.text = local_data[0];
			AmountWon.text = local_data[1];
			if (local_data[2].StartsWith("P") || local_data[2].Length <= 5)
				WLRatio.text = local_data[2];
			else
				WLRatio.text = local_data[2].Substring(0, 5);
		}
	}
}
