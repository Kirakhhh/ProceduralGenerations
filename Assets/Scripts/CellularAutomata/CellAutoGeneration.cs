using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CellAutoGeneration : MonoBehaviour { 

    public int widthMap;
    public int heightMap;
    public int seed;
    public bool randomSeed;

    [Range(0, 100)] 
    public int fillPercent;
    public int smoothCount;

    private int[,] map;

    public void GenerateMapOnMesh()
    {
        map = new int[widthMap, heightMap];
        RandomFillMap();

        for (int i = 0; i < smoothCount; i++)
            SmoothMap();

        CellAutoMeshGenerator mesh = GetComponent<CellAutoMeshGenerator>();
        mesh.CreateMesh(map);
    }

    void RandomFillMap()
    {
        if (randomSeed)
            seed = (int)Time.time;

        System.Random random = new System.Random(seed);

        for (int x = 0; x < widthMap; x++)
            for (int y = 0; y < heightMap; y++)
                if (x == 0 || x == widthMap - 1 || y == 0 || y == heightMap - 1)
                    map[x, y] = 1;
                else
                    map[x, y] = (random.Next(0, 100) < fillPercent) ? 1 : 0;

    }

    void SmoothMap()
    {
        for (int x = 0; x < widthMap; x++)
            for (int y = 0; y < heightMap; y++)
            {
                int neighbourWallCount = GetCountOfNeighboorWall(x, y);

                if (neighbourWallCount > 4)
                    map[x, y] = 1;
                else if (neighbourWallCount < 4)
                    map[x, y] = 0;
            }
    }

    int GetCountOfNeighboorWall(int checkPointX, int checkPointY)
    {
        int neighboorWallCount = 0;
        // Окрестность Мура 1 порядка
        for (int neighborX = checkPointX - 1; neighborX <= checkPointX + 1; neighborX++)
            for (int neighborY = checkPointY - 1; neighborY <= checkPointY + 1; neighborY++)
                if (neighborX >= 0 && neighborX < widthMap && neighborY >= 0 && neighborY < heightMap)
                {
                    if (neighborX != checkPointX || neighborY != checkPointY)
                        neighboorWallCount += map[neighborX, neighborY];
                }
                else
                    neighboorWallCount++;
        return neighboorWallCount;
    }


    //private void OnDrawGizmos()
    //{
    //    if (map != null)
    //    {
    //        for (int x = 0; x < widthMap; x++)
    //        {
    //            for (int y = 0; y < heightMap; y++)
    //            {
    //                Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
    //                Vector3 pos = new Vector3(-widthMap * 0.5f + x + 0.5f, -heightMap * 0.5f + y + 0.5f, 0);
    //                Gizmos.DrawCube(pos, Vector3.one);
    //            }
    //        }
    //    }
    //}
}
