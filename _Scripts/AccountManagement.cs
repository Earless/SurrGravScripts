using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AccountManagement : MonoBehaviour {
	public InputField registerU;
	public InputField registerP;
	public InputField registerPC;
	public InputField loginU;
	public InputField loginP;
	public Text currentUName;
	public NW_Server networkServer;
	public MenuFunctions menuF;
	
	public static bool loggedIn;
	
	private AccountList list_of_accounts;
	private Account log_acc;
	
	void Start ()
	{
		list_of_accounts = new AccountList ();
		log_acc = new Account ("", "");
		if (BasicFunctions.firstStart)
		{
			BasicFunctions.loginServer = true;
		}
		if (BasicFunctions.firstStart)
		{
			loggedIn = false;
		}
		
		using (StreamReader sread = new StreamReader("Accounts.txt"))
		{
			list_of_accounts = AccountList.readAccounts(sread); //Reading in all the Accounts
			sread.Close ();
		}
	}
	/* Lees de username en password in
	 */
	public void registerAccount ()
	{
		string username = registerU.text.ToString();
		string password = registerP.text.ToString();
		string passwordC = registerPC.text.ToString();
		registerAccount (username, password, passwordC);
	}
	/* Lokaal registreren van account; dit doet alleen de Server
	 */
	public void registerAccount (string Uname, string Pword, string PwordC)
	{
		if (Pword.Equals(PwordC) && Pword.Length>0 && PwordC.Length > 0)
		{
			if (BasicFunctions.loginServer)
			{
				if(Uname == ""){
					Debug.Log("empty name");
					menuF.errorMessage.gameObject.SetActive(true);
					menuF.errorMessage.text = "Username can not be empty";
				}
				else{
					string url = "http://drproject.twi.tudelft.nl:8082/Register?playerName="+Uname+"&playerPassword="+Pword;
					WWW www = new WWW(url);
					StartCoroutine(WaitForRegistration(www));
				}
			}
			else
			{
				Account reg_acc = new Account (Uname, Pword);
				if (Uname == " ")
				{
					menuF.errorMessage.gameObject.SetActive(true);
					menuF.errorMessage.text = "Username can not be empty";
				}
				else if (!list_of_accounts.containsUsername(reg_acc))
				{
					
					list_of_accounts.addAccount(reg_acc);
					using (StreamWriter swrite = new StreamWriter ("Accounts.txt", true))
					{
						Account.writeAccount (reg_acc, swrite);
						swrite.Close ();
					}
					menuF.errorMessage.gameObject.SetActive(true);
					menuF.errorMessage.text = "Account is created";;
				}
				else
				{
					menuF.errorMessage.gameObject.SetActive(true);
					menuF.errorMessage.text = "Failed to register";
				}
			}
		}
		else
		{
			menuF.errorMessage.gameObject.SetActive(true);
			menuF.errorMessage.text = "Passwords must match and not be empty";
		}
		registerU.text = "";
		registerP.text = "";
		registerPC.text = "";
	}
	/* Lees de username, het password en het team in
	 */
	public void loginAccount ()
	{
		string username = loginU.text.ToString();
		string password = loginP.text.ToString();
		loginAccount (username, password);
	}
	/* Login op een account en geef de username en het gekozen team mee aan de Server
	 */
	public void loginAccount (string Uname, string Pword) //,string Team)
	{
		log_acc.Name = Uname;
		log_acc.Word = Pword;
	
		if (BasicFunctions.loginServer)
		{
			string url = "http://drproject.twi.tudelft.nl:8082/Authenticate?playerName="+Uname+"&playerPassword="+Pword;
			WWW www = new WWW(url);
			StartCoroutine(WaitForAuthorization(www));
		}
		else
		{
			using (StreamReader slread = new StreamReader("Accounts.txt"))
			{
				list_of_accounts = AccountList.readAccounts(slread);
				slread.Close ();
			}
			
			int i = 0;
			while (i < list_of_accounts.sizeList)
			{
				Account acc2 = list_of_accounts.indexOf(i);
				if (log_acc.equals(acc2))
				{
					Debug.Log("Account: " + this.log_acc.Name + "; " + this.log_acc.Word);
					break;
				}
				i++;
			}

			if (i != this.list_of_accounts.sizeList) //Dus account bestaat en password is correct
			{
				loginU.text = "";
				loginP.text = "";
				BasicFunctions.activeAccount = new Account(log_acc.Name, log_acc.Word);
				currentUName.text = BasicFunctions.activeAccount.Name;
				menuF.errorMessage.gameObject.SetActive(false);
				loggedIn = true;
				menuF.isLoggedIn = true;
			}
			else
			{
				menuF.errorMessage.gameObject.SetActive(true);
				menuF.errorMessage.text = "Login info incorrect";
			}
		}
	}

	IEnumerator WaitForRegistration(WWW www)
	{
		yield return www;

		if (www.error == null){
			if(www.text.Equals ("Succesfully Registered")){
				menuF.errorMessage.gameObject.SetActive(true);
				menuF.errorMessage.text = "Account is created";
			}
			else if(www.text.Equals ("Can not create empty game account")){
				menuF.errorMessage.gameObject.SetActive(true);
				menuF.errorMessage.text = "Username can not be empty";
			}
			else{
				menuF.errorMessage.gameObject.SetActive(true);
				menuF.errorMessage.text = "Failed to register, username is taken";
			}
		}
		else
		{
			menuF.errorMessage.gameObject.SetActive(true);
			menuF.errorMessage.text = "Can't connect to Webserver\nTry registering locally";
		}

		registerU.text = "";
		registerP.text = "";
		registerPC.text = "";
	}

	IEnumerator WaitForAuthorization(WWW www)
	{
		yield return www;
		
		// check for errors
		if (www.error == null)
		{
			if(www.text.Equals("Succesfully Authorized")){
				BasicFunctions.activeAccount = new Account(log_acc.Name, log_acc.Word);
				currentUName.text = BasicFunctions.activeAccount.Name;
				menuF.errorMessage.gameObject.SetActive(false);
				loggedIn = true;
				menuF.isLoggedIn = true;
			}
			else{
				menuF.errorMessage.gameObject.SetActive(true);
				menuF.errorMessage.text = "Login info incorrect";
			}
		} else {
			Debug.Log("WWW Error: "+ www.error);
			menuF.errorMessage.gameObject.SetActive(true);
			menuF.errorMessage.text = "Can't connect to Webserver\nTry logging in locally";
		}  
		loginU.text = "";
		loginP.text = "";
	}
}