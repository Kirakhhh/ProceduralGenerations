using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangulation
{
    public Dictionary<int, Vector3> mainRoomsCoords = new Dictionary<int, Vector3>();
    public List<Tuple<int, int>> pointsOfTriangulationVectors = new List<Tuple<int, int>>();
    public List<double> lengthOfVectors = new List<double>();

    public Triangulation(Dictionary<int, Vector3> coords)
    {
        this.mainRoomsCoords = coords;
        List<int> keyList = new List<int>(this.mainRoomsCoords.Keys);

        int firstPoint;
        int secondPoint;
        double lengthOfVector;
        for (int i = 0; i < keyList.Count-1; i++)
        {
            firstPoint = keyList[i];
            for (int j = i+1; j < keyList.Count; j++)
            {
                secondPoint = keyList[j];
                pointsOfTriangulationVectors.Add(Tuple.Create(firstPoint, secondPoint));
                lengthOfVector = Math.Sqrt((mainRoomsCoords[secondPoint].x- mainRoomsCoords[firstPoint].x) * (mainRoomsCoords[secondPoint].x - mainRoomsCoords[firstPoint].x) + (mainRoomsCoords[secondPoint].y - mainRoomsCoords[firstPoint].y) * (mainRoomsCoords[secondPoint].y - mainRoomsCoords[firstPoint].y));

                lengthOfVectors.Add(lengthOfVector);
            }
           
        }

        sortVectorsbyLength();

        formationOfTriangulation();
    }

    private void sortVectorsbyLength()
    {
        bool check = true;
        int rightBorder = lengthOfVectors.Count;
        while (check)
        {
            check = false;  
            for (int i = 0; i+1 < rightBorder; i++)
            {
                //Debug.Log("длина 1: " + lengthOfVectors[i] + " длина 2: " + lengthOfVectors[i + 1]);
                if (lengthOfVectors[i] > lengthOfVectors[i + 1])
                {
                    Swap(lengthOfVectors, i, i + 1);
                    Swap(pointsOfTriangulationVectors, i, i + 1);
                    //Debug.Log(" свап длина 1: " + lengthOfVectors[i] + " длина 2: " + lengthOfVectors[i + 1]);
                    check = true;
                }
            }
            rightBorder--;
        }
        //Debug.Log("конец сортировки");
    }

    private static void Swap<T>(IList<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }

    private void formationOfTriangulation()
    {
        int A, B, C, D;

        for (int i = 0; i + 1 < pointsOfTriangulationVectors.Count; i++)
        {
            A = pointsOfTriangulationVectors[i].Item1;
            B = pointsOfTriangulationVectors[i].Item2;
            for (int j = i+1; j < pointsOfTriangulationVectors.Count; j++)
            {
                C = pointsOfTriangulationVectors[j].Item1;
                D = pointsOfTriangulationVectors[j].Item2;
                if (A != C && B != C && A != D && B != D)
                {
                    if (сheckCrossing(A, B, C, D))
                    {
                        //Debug.Log("удаленная связь 1: " + pointsOfTriangulationVectors[j]);
                        pointsOfTriangulationVectors.RemoveAt(j);
                        lengthOfVectors.RemoveAt(j);
                        j--;
                    }
                }
            } 
        }
    }

    private bool сheckCrossing(int A, int B, int C, int D)
    {
        double x1 = mainRoomsCoords[A].x, y1 = mainRoomsCoords[A].y;
        double x2 = mainRoomsCoords[B].x, y2 = mainRoomsCoords[B].y;
        double x3 = mainRoomsCoords[C].x, y3 = mainRoomsCoords[C].y;
        double x4 = mainRoomsCoords[D].x, y4 = mainRoomsCoords[D].y;

        double denominator = (y4 - y3) * (x1 - x2) - (x4 - x3) * (y1 - y2);
        if (denominator == 0)
        {
            if ((x1 * y2 - x2 * y1) * (x4 - x3) - (x3 * y4 - x4 * y3) * (x2 - x1) == 0 && (x1 * y2 - x2 * y1) * (y4 - y3) - (x3 * y4 - x4 * y3) * (y2 - y1) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            double numerator_a = (x4 - x2) * (y4 - y3) - (x4 - x3) * (y4 - y2);
            double numerator_b = (x1 - x2) * (y4 - y2) - (x4 - x2) * (y1 - y2);
            double Ua = numerator_a / denominator;
            double Ub = numerator_b / denominator;
            if (Ua >= 0 && Ua <= 1 && Ub >= 0 && Ub <= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
