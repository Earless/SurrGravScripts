using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

/* Class: Account
 * Stelt een account voor met een Username, Passowrd en een Playernumber
 */
public class Account {

	private string Username;
	private string Password;
	private int playerNumb;
	//private int gamePoints;
	//private Color teamColor;

	/* Creeert een Account met een username en password (number wordt op 0 gezet)
	 */
	public Account (string Uname, string Pword)
	{
		Username = Uname;
		Password = Pword;
		playerNumb = 0;
	}
	/* Getter/Setter voor de username
	 */
	public string Name
	{
		get { return Username; }
		set { Username = value; }
	}
	/* Getter/Setter voor de password
	 */
	public string Word
	{
		get { return Password; }
		set { Password = value; }
	}
	/* Getter/Setter voor de playernumber
	 */
	public int Number
	{
		get { return playerNumb; }
		set { playerNumb = value; }
	}
	/* Getter/Setter voor de gamePoints
	 */
	//public int Points
	//{
	//	get { return gamePoints; }
	//	set { gamePoints = value; }
	//}

	public string toString ()
	{
		return Username + ", " + Number;
	}
	/* Checkt of 2 accounts gelijk zijn
	 */
	public bool equals (Account acc)
	{
		bool res = false;
		if (this.Username.Equals(acc.Username) && this.Password.Equals(acc.Password))
		{
			res = true;
		}

		return res;
	}
	/* Lees een Account in met sread
	 */
	public static Account readAccount (StreamReader sread)
	{
		string Uname = sread.ReadLine();
		string Pword = sread.ReadLine();
		//Debug.Log("INLEZEN: " + Uname + "; " + Pword);
		Account newAccount = new Account (Uname, Pword);
		return newAccount;
	}
	/* Schrijf een Account weg met swrite
	 */
	public static void writeAccount (Account acc, StreamWriter swrite)
	{
		string Uname = acc.Username;
		string Pword = acc.Password;
		swrite.WriteLine (Uname);
		swrite.WriteLine (Pword);
	}
}
