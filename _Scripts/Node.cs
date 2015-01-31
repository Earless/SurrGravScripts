using UnityEngine;
using System.Collections;

public class Node{

	public int hValue = 0;
	public int gValue = 0;
	public int totalValue = 0;
	public int xPosition;
	public int yPosition;
	public int zPosition;

	public Node parentNode = null;
	public Node widthneg = null;
	public Node widthpos = null;
	public Node heightneg = null;
	public Node heightpos = null;
	public Node depthneg = null;
	public Node depthpos = null;

	public bool canPass;

	public Node(int x, int y, int z)
	{
		parentNode = null;
		widthneg = null;
		widthpos = null;
		heightneg = null;
		heightpos = null;
		depthneg = null;
		depthpos = null;
		xPosition = x;
		yPosition = y;
		zPosition = z;
	}

	public void calculatetotalValue()
	{
		totalValue = hValue + gValue;
	}
}
