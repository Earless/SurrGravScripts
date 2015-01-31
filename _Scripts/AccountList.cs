using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

/* Class: Accountlist
 * Bevat een lijst met alle Accounts
 */
public class AccountList {

	private List<Account> Acclist;
	/* Maak een nieuwe lijst
	 */
	public AccountList ()
	{
		Acclist = new List<Account> ();
	}
	/* Returnt het aantal Accounts in de lijst
	 */
	public int sizeList
	{
		get { return Acclist.Count; }
	}
	/* Returnt de index van i
	 */
	public Account indexOf (int i)
	{
		return Acclist[i];
	}
	/* Voeg een Account toe aan de lijst
	 */
	public void addAccount (Account acc)
	{
		if (!Acclist.Contains(acc))
		{
			Acclist.Add(acc);
		}
	}
	/* Check of de lijst Account acc bevat
	 */
	public bool containsUsername (Account acc)
	{
		bool res = false;
		string uname = acc.Name;
		int i = 0;
		while (i < Acclist.Count && res == false)
		{
			if (Acclist[i].Name.Equals(uname))
			{
				res = true;
			}
			i++;
		}

		return res;
	}
	/* Lees alle Accounts in met sread en voeg ze toe aan de lijst
	 */
	public static AccountList readAccounts (StreamReader sread)
	{
		AccountList al = new AccountList ();
		int i = 0;
		while (sread.Peek() != -1)
		{
			Account acc = Account.readAccount(sread);
			al.addAccount(acc);
			i++;
		}
		sread.Close();
		return al;
	}

}
