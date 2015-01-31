using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
public class SpawnBlock : MonoBehaviour {

	public GameObject buildingBlock;
	public GameObject Settings;
	public float checkRadius;

	private float xScaling;
	private float yScaling;
	private float zScaling;
	private bool maxOneDirectionOfChange;
	private int spawn;


	void startUp(int sp){
		if (sp>0){
			spawn = sp-1;
			createSecondarySpawn();

		}
	}



	void createSecondarySpawn(){

		LevelCreator settings = Settings.GetComponent<LevelCreator>();


		Vector3 targetPosition;
		int iterations = 0;
		Vector3 add;
		int checkResult;

		List<int> xList = new List<int>();
		xList.Add (-1);xList.Add (1);
		List<int> yList = new List<int>();
		yList.Add (-1);yList.Add (1);
		List<int> zList = new List<int>();
		zList.Add (-1);zList.Add (1);

		float positionchange1 = Mathf.CeilToInt (Random.value*100);
		int positionchange2 = 0;
		float xInterval = (100/3);
		float yInterval = (100/3);
		float zInterval = (100/3);
		
		
		if(settings.xScaling > 1 && xList.Count>0){
			xInterval = ((100/3)*settings.xScaling);
			yInterval = (100-xInterval)/2;
			zInterval = yInterval;
		}
		else if(settings.yScaling > 1 && yList.Count>0){
			yInterval = ((100/3)*settings.yScaling);
			xInterval = (100-yInterval)/2;
			zInterval = xInterval;
		}
		else{
			zInterval = ((100/3)*settings.zScaling);
			xInterval = (100-zInterval)/2;
			yInterval = xInterval;
		}
		
		do{
			if(true){



				int temprandom = 0;
				int direction = 0;


				if(yList.Count == 0|| xList.Count == 0 || zList.Count == 0)
					Debug.Log ("klopt niet");

				if(positionchange1<xInterval && xList.Count>0){
					temprandom = Mathf.CeilToInt(Random.value*xList.Count);
					positionchange2 = 1;
					direction = xList[temprandom-1];
					xList.RemoveAt(temprandom-1);
				}
				else if(positionchange1<(xInterval + yInterval) && yList.Count>0){
					temprandom = Mathf.CeilToInt(Random.value*yList.Count);
					positionchange2 = 2;
					direction = yList[temprandom-1];
					yList.RemoveAt(temprandom-1);
				}
				else if(positionchange1<(xInterval + yInterval + zInterval) && zList.Count>0){
					temprandom = Mathf.CeilToInt(Random.value*zList.Count);
					positionchange2 = 3;
					direction = zList[temprandom-1];
					zList.RemoveAt(temprandom-1);
				}



				add = new Vector3(0,0,0);


				if(positionchange2 == 1)
					add.x = direction;
				else if(positionchange2 == 2)
					add.y = direction;
				else if(positionchange2 == 3)
					add.z = direction;




			}
			
			else{
				add = new Vector3(Mathf.RoundToInt((Random.value*2) - 1),Mathf.RoundToInt((Random.value*2) - 1),Mathf.RoundToInt((Random.value*2) - 1));
			}

			targetPosition = this.transform.position + add;
			checkResult = Physics.OverlapSphere( targetPosition, checkRadius ).Length;
			iterations++;			
			
		}while (checkResult>0 && iterations<50);


		GameObject go = (GameObject)Instantiate (buildingBlock, targetPosition, Quaternion.identity);
		go.SendMessage ("startUp", spawn);
	}
}
*/
