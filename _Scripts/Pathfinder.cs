using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour{

	private List<Node> openList;
	private List<Node> closedList;
	private Node checkingNode = null;
	public Node startNode = null;
	public Node targetNode = null;
	public bool foundTarget = false;
	public int baseMovementCost = 10;
	public Copy_LevelCreator level;

	public List<Node> path;

	public bool tracedBack = false;

	public RobotMovement robotmovement;
	private GameObject levelcreator;
	private int[,,] grid;
	private Node[,,] nodeGrid;
	public bool findPath = false;
	public bool reset = true;

	public bool nodeGridInitialised = false;

	void Start(){
		nodeGridInitialised = false;
		if(Network.isServer)
		{
			robotmovement = this.GetComponent<RobotMovement>();

			nodeGrid = new Node[level.levelWidth,level.levelHeight,level.levelDepth];

			for(int width=0;width<level.levelWidth;width++)
			{
				for (int height=0;height<level.levelHeight;height++)
				{
					for (int depth=0;depth<level.levelDepth;depth++)
					{
						Node temp =  new Node(width,height,depth);
						nodeGrid[width,height,depth] = temp;

						if(width>0)
						{
							nodeGrid[width,height,depth].widthneg = nodeGrid[width-1,height,depth];
							nodeGrid[width-1,height,depth].widthpos = nodeGrid[width,height,depth];
						}

						if(height>0)
						{
							nodeGrid[width,height,depth].heightneg = nodeGrid[width,height-1,depth];
							nodeGrid[width,height-1,depth].heightpos = nodeGrid[width,height,depth];
						}
						if(depth>0)
						{
							nodeGrid[width,height,depth].depthneg = nodeGrid[width,height,depth-1];
							nodeGrid[width,height,depth-1].depthpos = nodeGrid[width,height,depth];
						}
					}
				}
			}
		}
		nodeGridInitialised = true;
	}

	void Update()
	{
		if (Network.isServer && nodeGridInitialised)
		{
			if (findPath)
			{
				if (Mathf.RoundToInt(this.transform.position.x)<nodeGrid.GetLength (0) && Mathf.RoundToInt(this.transform.position.y)<nodeGrid.GetLength (0) && Mathf.RoundToInt(this.transform.position.z)<nodeGrid.GetLength (0))
				{
					if (reset)
					{
						startNode = nodeGrid[Mathf.RoundToInt(this.transform.position.x),Mathf.RoundToInt(this.transform.position.y),Mathf.RoundToInt(this.transform.position.z)];
						if (openList != null && closedList != null)
						{
							for (int i=0; i<openList.Count;i++)
							{
								openList[i].parentNode = null;
							}
							for (int i=0; i<closedList.Count;i++)
							{
								closedList[i].parentNode = null;
							}
							startNode = path[1];
						}
						path = new List<Node>();
						openList = new List<Node>();
						closedList = new List<Node>();

						checkingNode = startNode;
						CalculateAllHeuristics();

						levelcreator = GameObject.FindGameObjectWithTag("levelSettings");
						level = levelcreator.GetComponent<Copy_LevelCreator>();
						grid = level.getGrid ();
						reset = false;
					}
					int j = 0;
					while (j<(1.7/Time.deltaTime))
					{
						if (foundTarget == false)
						{
							FindPath();
						}

						if (foundTarget == true)
						{
							if (tracedBack == false)
							{
								TraceBackPath ();
								reset = true;
							}
						}
						j++;
					}
				}
			}
		}
	}

	public int CalculateHeuristicValue(Node temp)
	{
		int heuristic = Mathf.Abs (targetNode.xPosition - temp.xPosition) + Mathf.Abs (targetNode.yPosition - temp.yPosition) + Mathf.Abs (targetNode.zPosition - temp.zPosition);
		return heuristic;
	}

	public void CalculateAllHeuristics()
	{
		for(int width=0;width<level.levelWidth;width++)
		{
			for (int height=0;height<level.levelHeight;height++)
			{
				for (int depth=0;depth<level.levelDepth;depth++)
				{
					nodeGrid[width,height,depth].hValue = Mathf.Abs (targetNode.xPosition - nodeGrid[width,height,depth].xPosition) + Mathf.Abs (targetNode.yPosition - nodeGrid[width,height,depth].yPosition) + Mathf.Abs (targetNode.zPosition - nodeGrid[width,height,depth].zPosition);
				}
			}
		}

	}

	private void FindPath()
	{
		if (foundTarget == false){
			if (checkingNode.widthneg != null)
				DetermineNodeValues(checkingNode,checkingNode.widthneg);
			if (checkingNode.widthpos != null)
				DetermineNodeValues(checkingNode,checkingNode.widthpos);
			if (checkingNode.heightneg != null)
				DetermineNodeValues(checkingNode,checkingNode.heightneg);
			if (checkingNode.heightpos != null)
				DetermineNodeValues(checkingNode,checkingNode.heightpos);
			if (checkingNode.depthneg != null)
				DetermineNodeValues(checkingNode,checkingNode.depthneg);
			if (checkingNode.depthpos != null)
				DetermineNodeValues(checkingNode,checkingNode.depthpos);

			deleteFromOpenList(checkingNode);
			addToClosedList(checkingNode);
			checkingNode = smallestFValueNode();
		}
	}

	private void TraceBackPath()
	{
		Node node = targetNode.parentNode;
		path.Add (targetNode);
		do{
			path.Add(node);
			node = node.parentNode;

		} while(node != null);

		robotmovement.reset = true;
		tracedBack = true;
	}

	private void DetermineNodeValues(Node currentNode, Node testing)
	{
		if (testing == null)
			return;
		if (testing == targetNode)
		{
			targetNode.parentNode = currentNode;
			foundTarget = true;
			findPath = false;
			return;
		}
		if (grid[testing.xPosition,testing.yPosition,testing.zPosition] == 1)
		{
			return;
		}

		if(closedList.Contains (testing) == false)
		{
			if(openList.Contains (testing)== true)
			{
				int newGValue = currentNode.gValue + baseMovementCost;

				if(newGValue < testing.gValue)
				{
					testing.parentNode = currentNode;
					testing.gValue = newGValue;
					testing.calculatetotalValue();
				}
			}
			else
			{
				testing.parentNode = currentNode;
				testing.gValue = currentNode.gValue + baseMovementCost;
				testing.calculatetotalValue();
				addToOpenList(testing);
			}			
		}
	}

	private void addToOpenList(Node toadd)
	{
		openList.Add(toadd);
	}

	private void addToClosedList(Node toadd)
	{
		closedList.Add(toadd);
	}

	private void deleteFromOpenList(Node todel)
	{
		openList.Remove(todel);
	}

	private void deleteFromClosedList(Node todel)
	{
		closedList.Remove (todel);
	}

	private Node smallestFValueNode()
	{
		if(openList.Count>0)
		{
			Node smallest = openList[0];
			for(int i =1;i<openList.Count;i++)
			{
				if (openList[i].totalValue < smallest.totalValue)
				{
					smallest = openList[i];
				}
			}
			return smallest;
		}
		else
		{
			return null;
		}
	}

	public void setTargetNode(Vector3 position)
	{
		targetNode = nodeGrid[Mathf.RoundToInt(position.x),Mathf.RoundToInt(position.y),Mathf.RoundToInt(position.z)];

	}

	void setFindPath(bool bo)
	{
		findPath = bo;
	}
}
