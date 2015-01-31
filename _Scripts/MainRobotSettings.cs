using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainRobotSettings : MonoBehaviour {

	public GameObject robot;

	public Copy_LevelCreator level;

	public List<GameObject> robots;

	private int selectBlock;
	private bool hasspawned = false;

	void Start()
	{
		playerController.dontDestroy = false;
		hasspawned = false;
		level = GameObject.FindGameObjectWithTag("levelSettings").GetComponent<Copy_LevelCreator>();
	}
	
	void Update()
	{
		if (Network.isServer && !BasicFunctions.playOffline && !hasspawned && level.gridinitialised)
		{
			for (int i = 0; i < BasicFunctions.maxRobots; i++)
			{
				Network.Instantiate (robot, level.getRobotSpawn (), Quaternion.identity,0);
			}
			hasspawned = true;
		}
	}
}
