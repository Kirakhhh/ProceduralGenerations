using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingSquaresGrid
{
	public Square[,] squares;

	public MarchingSquaresGrid(int[,] map)
	{
		int nodeCountX = map.GetLength(0);
		int nodeCountY = map.GetLength(1);

		Node[,] Nodes = new Node[nodeCountX, nodeCountY];

		for (int x = 0; x < nodeCountX; x++)
			for (int y = 0; y < nodeCountY; y++)
				Nodes[x, y] = new Node(new Vector3(x, y, 0), map[x, y] == 1);

		squares = new Square[nodeCountX - 1, nodeCountY - 1];
		for (int x = 0; x < nodeCountX - 1; x++)
			for (int y = 0; y < nodeCountY - 1; y++)
				squares[x, y] = new Square(Nodes[x, y + 1], Nodes[x + 1, y + 1], Nodes[x + 1, y], Nodes[x, y]);

	}
}

public class Square
{
	public Node topLeft, topRight, bottomRight, bottomLeft;
	public Node middleTop, middleRight, middleBottom, middleLeft;
	public int configuration;

	public Square(Node _topLeft, Node _topRight, Node _bottomRight, Node _bottomLeft)
	{
		topLeft = _topLeft;
		topRight = _topRight;
		bottomRight = _bottomRight;
		bottomLeft = _bottomLeft;

		middleTop = new Node(topLeft.position + Vector3.right * 0.5f, false);
		middleRight = new Node(bottomRight.position + Vector3.up * 0.5f, false);
		middleBottom = new Node(bottomLeft.position + Vector3.right * 0.5f, false);
		middleLeft = new Node(bottomLeft.position + Vector3.up * 0.5f, false);

		if (topLeft.isWall)
			configuration += 8;
		if (topRight.isWall)
			configuration += 4;
		if (bottomRight.isWall)
			configuration += 2;
		if (bottomLeft.isWall)
			configuration += 1;
	}
}

public class Node
{
	public Vector3 position;
	public int vertexIndex = -1;
	public bool isWall;
	public Node(Vector3 _pos, bool _isWall)
	{
		position = _pos;
		isWall = _isWall;
	}
}