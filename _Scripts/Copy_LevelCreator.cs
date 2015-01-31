using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Copy_LevelCreator : MonoBehaviour {
	
	public GameObject buildingBlock1;
	public GameObject buildingBlock2;
	public GameObject buildingBlock3;
	public GameObject buildingBlock4;
	public GameObject MainRobotSettings;
	public int numberOfLargeSpawns;
	public int numberOfSmallSpawns;
	public int approxBlocksPerLargeStack;
	public int approxBlocksPerSmallStack;
	public int levelWidth;
	public int levelHeight;
	public int levelDepth;
	public float checkRadius;
	public float xScaling;
	public float yScaling;
	public float zScaling;
	public bool ceiling;
	public bool floor;
	public bool plusx;
	public bool negx;
	public bool plusz;
	public bool negz;
	public int smoothen;
	public int blocktypes;
	public bool gridinitialised;
	public int spawnradius;

	private int[,,] grid;
	private int[] targetPosition;
	private int seedint;
	private int ratio = 20;

	// Use this for initialization
	void Start () 
	{
		gridinitialised = false;

		if (Network.isServer || BasicFunctions.playOffline)
		{
			seedint = System.Environment.TickCount;
			Random.seed = seedint;
			grid = new int[levelWidth,levelHeight,levelDepth];
			
			//Fills the grid matrix with 1s, representing large spawns
			//createLargeSpawn() is called numberOfLargeSpawns times, each time creating a separate spawn
			for(int i = 0; i<numberOfLargeSpawns;i++)
			{
				createMainSpawn(approxBlocksPerLargeStack);
			}
			
			//fills the grid matrix with 1s, representing small spawns
			//createSmallSpawn() is called numberOfSmallSpawns times, each time creating a separate spawn
			for(int i = 0; i<numberOfSmallSpawns;i++)
			{
				createMainSpawn(approxBlocksPerSmallStack);			
			}

			for(int i=0;i<smoothen;i++){
				smoothLevel();
			}
			
			//loop through the grid matrix, drawing a building block every time a 1 is encountered
			//draw(levelWidth, levelHeight, levelDepth, grid);
			draw ();

			resetMatrix();

			gridinitialised = true;
		}
	}

	void smoothLevel(){
		int[,,] gridCopy = new int[levelWidth,levelHeight,levelDepth];
		for(int width=1;width<levelWidth-1;width++)
		{
			for (int height=1;height<levelHeight-1;height++)
			{
				for (int depth=1;depth<levelDepth-1;depth++)
				{
					int counter = 0;
					if(grid[width,height,depth]>0)
					{
						gridCopy[width,height,depth] = grid[width,height,depth];
						if(grid[width+1,height,depth]>0)
							counter++;
						if(grid[width-1,height,depth]>0)
							counter++;
						if(grid[width,height+1,depth]>0)
							counter++;
						if(grid[width,height-1,depth]>0)
							counter++;
						if(grid[width,height,depth+1]>0)
							counter++;
						if(grid[width,height,depth-1]>0)
							counter++;

						if(counter<=1)
						{
							gridCopy[width,height,depth]=0;
						}
					}
				}
			}
		}

		for(int width=1;width<levelWidth-1;width++)
		{
			for (int height=1;height<levelHeight-1;height++)
			{
				for (int depth=1;depth<levelDepth-1;depth++)
				{
					int counter = 0;
					List<int> blockt = new List<int>();
					blockt.Add (0);blockt.Add (0);blockt.Add (0);blockt.Add (0);
					if(grid[width,height,depth]==0)
					{
						if(grid[width+1,height,depth]>0)
						{
							counter += ratio;
							blockt[grid[width+1,height,depth]-1] += 1;
						}
						if(grid[width-1,height,depth]>0)
						{
							counter += ratio;
							blockt[grid[width-1,height,depth]-1] += 1;
						}
						if(grid[width,height+1,depth]>0)
						{
							counter += ratio;
							blockt[grid[width,height+1,depth]-1] += 1;
						}
						if(grid[width,height-1,depth]>0)
						{
							counter += ratio;
							blockt[grid[width,height-1,depth]-1] += 1;
						}
						if(grid[width,height,depth+1]>0)
						{
							counter += ratio;
							blockt[grid[width,height,depth+1]-1] += 1;
						}
						if(grid[width,height,depth-1]>0)
						{
							counter += ratio;
							blockt[grid[width,height,depth-1]-1] += 1;
						}
						if(grid[width+1,height+1,depth]>0)
						{
							counter++;
							blockt[grid[width+1,height+1,depth]-1] += 1;
						}
						if(grid[width+1,height-1,depth]>0)
						{
							counter++;
							blockt[grid[width+1,height-1,depth]-1] += 1;
						}
						if(grid[width-1,height+1,depth]>0)
						{
							counter++;
							blockt[grid[width-1,height+1,depth]-1] += 1;
						}
						if(grid[width-1,height-1,depth]>0)
						{
							counter++;
							blockt[grid[width-1,height-1,depth]-1] += 1;
						}
						if(grid[width+1,height,depth+1]>0)
						{
							counter++;
							blockt[grid[width+1,height,depth+1]-1] += 1;
						}
						if(grid[width+1,height,depth-1]>0)
						{
							counter++;
							blockt[grid[width+1,height,depth-1]-1] += 1;
						}
						if(grid[width-1,height,depth+1]>0)
						{
							counter++;
							blockt[grid[width-1,height,depth+1]-1] += 1;
						}
						if(grid[width-1,height,depth-1]>0)
						{
							counter++;
							blockt[grid[width-1,height,depth-1]-1] += 1;
						}
						if(grid[width,height+1,depth+1]>0)
						{
							counter++;
							blockt[grid[width,height+1,depth+1]-1] += 1;
						}
						if(grid[width,height+1,depth-1]>0)
						{
							counter++;
							blockt[grid[width,height+1,depth-1]-1] += 1;
						}
						if(grid[width,height-1,depth+1]>0)
						{
							counter++;
							blockt[grid[width,height-1,depth+1]-1] += 1;
						}
						if(grid[width,height-1,depth-1]>0)
						{
							counter++;
							blockt[grid[width,height-1,depth-1]-1] += 1;
						}

						if(counter>=60)
						{
							gridCopy[width,height,depth]=1;
							if(blockt[1]>blockt[0])
							{
								gridCopy[width,height,depth]=2;
							}
							if(blockt[2]>blockt[0] && blockt[2]>blockt[1])
							{
								gridCopy[width,height,depth]=3;

							}
							if(blockt[3]>blockt[2] && blockt[3]>blockt[1] && blockt[3]>blockt[0])
							{
								gridCopy[width,height,depth]=4;
							}
						}
					}
				}
			}
		}
		grid = gridCopy;
	}
	//function responsible for filling the grid matrix with spawns
	void createMainSpawn(int approxBlocks){
		int checkResult;
		int blocktype = Random.Range (1,blocktypes+1);
		
		//random offset for the blocks per spawn
		int delta = Random.Range (Mathf.FloorToInt (approxBlocks * -0.1f), Mathf.CeilToInt (approxBlocksPerLargeStack * 0.1f+1f));
		int Blocks = approxBlocksPerLargeStack + delta;
		
		int iterations = 0;
		do{
			//determine a random intial targetposition to start a spawn, the location is checked to see whether it already contains a 1
			targetPosition = new int[3]{Random.Range(1,levelWidth-1),Random.Range(1,levelHeight-1),Random.Range(1,levelDepth-1)};
			checkResult = grid[targetPosition[0],targetPosition[1],targetPosition[2]];
			iterations++;

		} while (checkResult>0 && iterations<50);
		
		//Make the grid at position targetposition equal to 1
		grid [targetPosition[0],targetPosition[1],targetPosition[2]] = blocktype;
		
		//Create the secondary spawns for the main spawn
		for(int i=0; i<Blocks; i++)
		{
			createSpawn(blocktype);
		}
	}

	//function responsible for creating secondary spawn
	void createSpawn(int blocktype)
	{
		//lists that contain the forward and backward direction
		List<int> xList = new List<int>();
		xList.Add (-1);xList.Add (1);
		List<int> yList = new List<int>();
		yList.Add (-1);yList.Add (1);
		List<int> zList = new List<int>();
		zList.Add (-1);zList.Add (1);

		//make sure the algorithm can not move out of the defined grid
		//added a translation to make my life easier when working with the robots
		if(targetPosition[0] <= 1){
			xList.RemoveAt (0);
		}
		if(targetPosition[1] <= 1){
			yList.RemoveAt (0);
		}
		if(targetPosition[2] <= 1){
			zList.RemoveAt (0);
		}
		if(targetPosition[0] >= levelWidth-2){
			xList.RemoveAt (1);
		}
		if(targetPosition[1] >= levelHeight-2){
			yList.RemoveAt (1);
		}
		if(targetPosition[2] >= levelDepth-2){
			zList.RemoveAt (1);
		}

		//variables used in the selection of the next direction of the building block
		float positionchange1 = Random.value;
		int positionchange2 = 0;

		int checkResult = 0;
		int iterations = 0;

		//define the probability intervals for each direction.
		float xInterval = xScaling/(xScaling + yScaling + zScaling);
		float yInterval = yScaling /(xScaling + yScaling + zScaling);
		float zInterval = zScaling /(xScaling + yScaling + zScaling);

		int direction;

		do
		{				
			int temprandom = 0;
			direction = 0;
				
			//determine which direction the next block should go
			if(positionchange1<xInterval && xList.Count>0)
			{
				temprandom = Random.Range(0,xList.Count);
				positionchange2 = 1;
				direction = xList[temprandom];
				xList.RemoveAt(temprandom);
			}
			else if(positionchange1<(xInterval + yInterval) && yList.Count>0)
			{
				temprandom = Random.Range(0,yList.Count);
				positionchange2 = 2;
				direction = yList[temprandom];
				yList.RemoveAt(temprandom);
			}
			else if(positionchange1<(xInterval + yInterval + zInterval) && zList.Count>0)
			{
				temprandom = Random.Range(0,zList.Count);
				positionchange2 = 3;
				direction = zList[temprandom];
				zList.RemoveAt(temprandom);
			}

			//if a certain direction is chosen, check if that direction is not already filled with a 1	
			if(positionchange2 == 1)
			{
				checkResult = grid[targetPosition[0]+direction,targetPosition[1],targetPosition[2]];
				if(checkResult<1)
					targetPosition[0] += direction;
			}
			else if(positionchange2 == 2)
			{
				checkResult = grid[targetPosition[0],targetPosition[1]+direction,targetPosition[2]];
				if(checkResult<1)
					targetPosition[1] += direction;
			}
			else if(positionchange2 == 3)
			{
				checkResult = grid[targetPosition[0],targetPosition[1],targetPosition[2]+direction];
				if(checkResult<1)
					targetPosition[2] += direction;
			}

			iterations++;	

		} while (checkResult>0 && iterations<50);

		//Set the grid at the new targetposition equal to 1		
		grid[targetPosition[0],targetPosition[1],targetPosition[2]] = blocktype;
	}

	//function that loops through the gread and instanciates a building block when it encounters a 1
	void draw() //int levelWidth, int levelHeight, int levelDepth, int[,,] grid)
	{
		for(int width=0;width<levelWidth;width++)
		{
			for (int height=0;height<levelHeight;height++)
			{
				for (int depth=0;depth<levelDepth;depth++)
				{
					if(grid[width,height,depth]==1)
					{
						if(BasicFunctions.playOffline)
						{
							Instantiate (buildingBlock1, new Vector3(width,height,depth), Quaternion.identity);
						}
						else{
							 Network.Instantiate (buildingBlock1, new Vector3(width,height,depth), Quaternion.identity,0);

						}
					}
					else if(grid[width,height,depth]==2)
					{
						if(BasicFunctions.playOffline)
						{
							Instantiate (buildingBlock2, new Vector3(width,height,depth), Quaternion.identity);
						}
						else{
							Network.Instantiate (buildingBlock2, new Vector3(width,height,depth), Quaternion.identity,0);

						}
					}
					else if(grid[width,height,depth]==3)
					{
						if(BasicFunctions.playOffline)
						{
							Instantiate (buildingBlock3, new Vector3(width,height,depth), Quaternion.identity);
						}
						else
						{
							Network.Instantiate (buildingBlock3, new Vector3(width,height,depth), Quaternion.identity,0);
						}
					}
					else if(grid[width,height,depth]==4)
					{
						if(BasicFunctions.playOffline)
						{
							Instantiate (buildingBlock4, new Vector3(width,height,depth), Quaternion.identity);
						}
						else
						{
							Network.Instantiate (buildingBlock4, new Vector3(width,height,depth), Quaternion.identity,0);
						}
					}
				}
			}
		}
	}

	//function that determines if a certain cube in a certain position is an edge cube
	public bool isEdge(Vector3 position)
	{
		if(grid != null)
		{
			int x = Mathf.RoundToInt(position.x);
			int y = Mathf.RoundToInt(position.y);
			int z = Mathf.RoundToInt(position.z);
			if(grid[x-1,y,z] == 1 && grid[x+1,y,z] == 1)
				return false;
			else if(grid[x,y-1,z] == 1 && grid[x,y+1,z] == 1)
				return false;
			else if(grid[x,y,z-1] == 1 && grid[x,y,z+1] == 1)
				return false;
			else
				return true;
		}
		else
			return false;
	}

	public int[,,] getGrid()
	{
		return grid;
	}
	
	public void setGrid(int x, int y, int z, int to)
	{
		this.grid[x,y,z] = to;
	}

	public void resetMatrix()
	{
		for(int width=1;width<levelWidth-1;width++)
		{
			for (int height=1;height<levelHeight-1;height++)
			{
				for (int depth=1;depth<levelDepth-1;depth++)
				{
					if(grid[width,height,depth]>1)
					{
						grid[width,height,depth]=1;
					}
				}
			}
		}
	}

	public Vector3 getSpawn()
	{
		Vector3 randomspawn;
		int checkresult;
		int x;
		int y;
		int z;
		int safecounter = 150;
		do{
			checkresult = 10;
			x = Random.Range (spawnradius+1,(levelWidth-spawnradius-1));y = Random.Range (spawnradius+2,(levelHeight-spawnradius-1));z = Random.Range (spawnradius+1,(levelDepth-spawnradius-1));
			if (grid[x,y-1,z] > 0 && grid[x,y,z] == 0)
			{
				checkresult = 0;
				for(int i=x;i<=x+spawnradius;i++)
				{
					for(int j=y;i<=y+spawnradius;i++)
					{
						for(int k=z;i<=z+spawnradius;i++)
						{
							if (grid[i,j,k]>0)
							{
								checkresult++;
							}
						}
					}
				}
			}
			//checkresult = Physics.OverlapSphere(randomspawn,2).Length;
			safecounter--;
		} while(checkresult>0 && safecounter>0);
		randomspawn = new Vector3(x,y,z);
		if(safecounter == 0)
		{
			randomspawn = new Vector3(levelWidth,levelHeight,levelDepth);
		}
		return randomspawn;
	}

	public Vector3 getRobotSpawn()
	{
		Vector3 randomspawn;
		int checkresult;
		int x;
		int y;
		int z;
		int safecounter = 150;
		do{
			checkresult = 10;
			x = Random.Range (spawnradius+2,(levelWidth-spawnradius-2));y = Random.Range (spawnradius+2,(levelHeight-spawnradius-2));z = Random.Range (spawnradius+2,(levelDepth-spawnradius-2));
			if (grid[x,y-1,z] > 0 && grid[x,y,z] == 0)
			{
				checkresult = 0;
				for(int i=x;i<=x+spawnradius;i++)
				{
					for(int j=y;i<=y+spawnradius;i++)
					{
						for(int k=z;i<=z+spawnradius;i++)
						{
							if (grid[i,j,k]>0)
							{
								checkresult++;
							}
						}
					}
				}
			}
			//checkresult = Physics.OverlapSphere(randomspawn,2).Length;
			safecounter--;
		} while(checkresult>0 && safecounter>0);
		randomspawn = new Vector3(x,y,z);
		if(safecounter == 0)
		{
			randomspawn = new Vector3(levelWidth-1,levelHeight-1,levelDepth-1);
		}
		return randomspawn;
	}
}
