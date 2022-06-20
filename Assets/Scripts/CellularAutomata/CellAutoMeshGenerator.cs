using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellAutoMeshGenerator : MonoBehaviour
{
	private MarchingSquaresGrid marchingSquaresGrid;
	private List<Vector3> vertices;
	private List<int> triangles;

	public void CreateMesh(int[,] map)
	{
		marchingSquaresGrid = new MarchingSquaresGrid(map);

		vertices = new List<Vector3>();
		triangles = new List<int>();

		for (int x = 0; x < marchingSquaresGrid.squares.GetLength(0); x++)
			for (int y = 0; y < marchingSquaresGrid.squares.GetLength(1); y++)
				CheckIsolanesOnSquare(marchingSquaresGrid.squares[x, y]);

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

    }

    void CheckIsolanesOnSquare(Square square)
	{
		switch (square.configuration)
		{
			case 0:
				break;

			// 1 вершина положительна:
			case 1:
				MeshFromPoints(square.middleBottom, square.bottomLeft, square.middleLeft);
				break;
			case 2:
				MeshFromPoints(square.middleRight, square.bottomRight, square.middleBottom);
				break;
			case 4:
				MeshFromPoints(square.middleTop, square.topRight, square.middleRight);
				break;
			case 8:
				MeshFromPoints(square.topLeft, square.middleTop, square.middleLeft);
				break;

			// 2:
			case 3:
				MeshFromPoints(square.middleRight, square.bottomRight, square.bottomLeft, square.middleLeft);
				break;
			case 6:
				MeshFromPoints(square.middleTop, square.topRight, square.bottomRight, square.middleBottom);
				break;
			case 9:
				MeshFromPoints(square.topLeft, square.middleTop, square.middleBottom, square.bottomLeft);
				break;
			case 12:
				MeshFromPoints(square.topLeft, square.topRight, square.middleRight, square.middleLeft);
				break;
			case 5:
				MeshFromPoints(square.middleTop, square.topRight, square.middleRight, square.middleBottom, square.bottomLeft, square.middleLeft);
				break;
			case 10:
				MeshFromPoints(square.topLeft, square.middleTop, square.middleRight, square.bottomRight, square.middleBottom, square.middleLeft);
				break;

			// 3:
			case 7:
				MeshFromPoints(square.middleTop, square.topRight, square.bottomRight, square.bottomLeft, square.middleLeft);
				break;
			case 11:
				MeshFromPoints(square.topLeft, square.middleTop, square.middleRight, square.bottomRight, square.bottomLeft);
				break;
			case 13:
				MeshFromPoints(square.topLeft, square.topRight, square.middleRight, square.middleBottom, square.bottomLeft);
				break;
			case 14:
				MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.middleBottom, square.middleLeft);
				break;

			// 4:
			case 15:
				MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
				break;
		}

	}

	void MeshFromPoints(params Node[] points)
	{
		AddVerticesToMesh(points);

		if (points.Length >= 3)
			AddTriangleToMesh(points[0], points[1], points[2]);
		if (points.Length >= 4)
			AddTriangleToMesh(points[0], points[2], points[3]);
		if (points.Length >= 5)
			AddTriangleToMesh(points[0], points[3], points[4]);
		if (points.Length >= 6)
			AddTriangleToMesh(points[0], points[4], points[5]);

	}

	void AddVerticesToMesh(Node[] points)
	{
		for (int i = 0; i < points.Length; i++)
			if (points[i].vertexIndex == -1)
			{
				points[i].vertexIndex = vertices.Count;
				vertices.Add(points[i].position);
			}
	}

	void AddTriangleToMesh(Node a, Node b, Node c)
	{
		triangles.Add(a.vertexIndex);
		triangles.Add(b.vertexIndex);
		triangles.Add(c.vertexIndex);
	}

  //  void OnDrawGizmos()
  //  {
		//if (marchingSquaresGrid != null)
  //          for (int x = 0; x < marchingSquaresGrid.squares.GetLength(0); x++)
  //              for (int y = 0; y < marchingSquaresGrid.squares.GetLength(1); y++)
  //              {
  //                  Gizmos.color = (marchingSquaresGrid.squares[x, y].topLeft.isWall) ? Color.black : Color.white;
  //                  Gizmos.DrawCube(marchingSquaresGrid.squares[x, y].topLeft.position, Vector3.one * .4f);
  //                  Gizmos.color = (marchingSquaresGrid.squares[x, y].topRight.isWall) ? Color.black : Color.white;
  //                  Gizmos.DrawCube(marchingSquaresGrid.squares[x, y].topRight.position, Vector3.one * .4f);
  //                  Gizmos.color = (marchingSquaresGrid.squares[x, y].bottomRight.isWall) ? Color.black : Color.white;
  //                  Gizmos.DrawCube(marchingSquaresGrid.squares[x, y].bottomRight.position, Vector3.one * .4f);
  //                  Gizmos.color = (marchingSquaresGrid.squares[x, y].bottomLeft.isWall) ? Color.black : Color.white;
  //                  Gizmos.DrawCube(marchingSquaresGrid.squares[x, y].bottomLeft.position, Vector3.one * .4f);
  //                  Gizmos.color = Color.grey;
  //                  Gizmos.DrawCube(marchingSquaresGrid.squares[x, y].middleTop.position, Vector3.one * .15f);
  //                  Gizmos.DrawCube(marchingSquaresGrid.squares[x, y].middleTop.position, Vector3.one * .15f);
  //                  Gizmos.DrawCube(marchingSquaresGrid.squares[x, y].middleTop.position, Vector3.one * .15f);
  //                  Gizmos.DrawCube(marchingSquaresGrid.squares[x, y].middleTop.position, Vector3.one * .15f);
  //              }
  //  }
}
