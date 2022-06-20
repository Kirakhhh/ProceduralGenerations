using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MinSpanTree
{
    private Dictionary<int, Vector3> mainRoomsCoords = new Dictionary<int, Vector3>();
    public List<Tuple<int, int>> pointsOfTriangulationVectors = new List<Tuple<int, int>>();
    private List<double> lengthOfTriangulationVectors = new List<double>();

    private List<int> indexOfNeighborVectors = new List<int>();
    private List<int> usedPoints = new List<int>();
    public List<Tuple<int, int>> pointsOfMinSpanTreeVectors = new List<Tuple<int, int>>();
    //private List<double> lengthOfMinSpanTreeVectors = new List<double>();
    
    public MinSpanTree(Triangulation Triangulation)
    {
        this.mainRoomsCoords = Triangulation.mainRoomsCoords;
        this.pointsOfTriangulationVectors = Triangulation.pointsOfTriangulationVectors;
        this.lengthOfTriangulationVectors = Triangulation.lengthOfVectors;
        List<int> points = new List<int>(this.mainRoomsCoords.Keys);

        int indexStartPoint = Random.Range(0, points.Count);
        //Debug.Log(points[indexStartPoint]);
        usedPoints.Add(points[indexStartPoint]);
        points.RemoveAt(indexStartPoint);

        while (points.Count > 0)
        {
            //Debug.Log("check");
            for (int i = 0; i < pointsOfTriangulationVectors.Count; i++)
            {
                int point1 = pointsOfTriangulationVectors[i].Item1;
                int point2 = pointsOfTriangulationVectors[i].Item2;
                if (usedPoints.IndexOf(point1) != -1 && usedPoints.IndexOf(point2) == -1 || usedPoints.IndexOf(point1) == -1 && usedPoints.IndexOf(point2) != -1)
                {
                    indexOfNeighborVectors.Add(i);
                }
            }
            double minLength = 1000;
            int indexOfMinVector = 0;
            for (int i = 0; i < indexOfNeighborVectors.Count; i++)
            {
                if (lengthOfTriangulationVectors[indexOfNeighborVectors[i]] < minLength)
                {
                    minLength = lengthOfTriangulationVectors[indexOfNeighborVectors[i]];
                    indexOfMinVector = indexOfNeighborVectors[i];
                    //Debug.Log("inside:"+indexOfMinVector);
                }
            }
            //Debug.Log(indexOfMinVector);
            if (usedPoints.IndexOf(pointsOfTriangulationVectors[indexOfMinVector].Item1) == -1)
            {
                //Debug.Log(pointsOfTriangulationVectors[indexOfMinVector].Item1);
                usedPoints.Add(pointsOfTriangulationVectors[indexOfMinVector].Item1);
                points.Remove(pointsOfTriangulationVectors[indexOfMinVector].Item1);
            }
            else 
            {
                //Debug.Log(pointsOfTriangulationVectors[indexOfMinVector].Item2);
                usedPoints.Add(pointsOfTriangulationVectors[indexOfMinVector].Item2);
                points.Remove(pointsOfTriangulationVectors[indexOfMinVector].Item2);
            }
            
            pointsOfMinSpanTreeVectors.Add(pointsOfTriangulationVectors[indexOfMinVector]);
            pointsOfTriangulationVectors.RemoveAt(indexOfMinVector);
            lengthOfTriangulationVectors.RemoveAt(indexOfMinVector);
            indexOfNeighborVectors.Clear();
        }
    }
}
