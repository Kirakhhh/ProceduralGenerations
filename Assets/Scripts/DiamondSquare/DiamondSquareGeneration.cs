using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiamondSquareGeneration : MonoBehaviour
{
    public Terrain Terrain;
    public PaintTerrain paintTerrain;

    public int heightMapResolutionPower;
    public float roughness;
    public int smoothCount;
    public int seed;
    public bool randomSeed;
    public int approximation;
    public int offsetX;
    public int offsetY;
    public float waterLevel;
    public bool useOnlyOffsetXY;

    private int _seed;
    private int _heightMapResolutionPower;
    private float _roughness;
    private int _smoothCount;
    private int _approximation;
    private float _waterLevel;

    private int heightMapResolution;
    private int visibleHeightMapResolution;
    private int sizeMap;
    private float outsideHeight = 0f;
    private float[,] bigMap;
    private float[,] visibleMap;

    public void GenerateMapOnTerrain()
    {

        if (useOnlyOffsetXY)
            CalculateDiamondSquareOnMap();
        else
        {
            if (randomSeed)
            {
                _seed = (int)Time.time;
                seed = _seed;
            }   
            else
                _seed = seed;

            _heightMapResolutionPower=heightMapResolutionPower;
            _roughness = roughness;
            _smoothCount = smoothCount;
            _approximation = approximation;
            _waterLevel = waterLevel;

            GenerateMap();
        }

        Terrain.terrainData.SetHeights(0, 0, visibleMap);

        paintTerrain.StartPaint();

        var waterObj = GameObject.Find("Water");
        waterObj.transform.position = new Vector3(waterObj.transform.position.x, _waterLevel * Terrain.terrainData.size.y, waterObj.transform.position.z);
    }

    public void GenerateMap()
    {
        heightMapResolution = (int)Mathf.Pow(2, _heightMapResolutionPower) + 1;
        visibleHeightMapResolution = (int)Mathf.Pow(2, _heightMapResolutionPower - _approximation) + 1;
        sizeMap = heightMapResolution - 1;
        bigMap = new float[heightMapResolution, heightMapResolution];
        visibleMap = new float[visibleHeightMapResolution, visibleHeightMapResolution];

        SetBaseHeights();
        SetHeightsInCorners();
        CalculateDiamondSquareOnMap();
    }
    
    private void SetBaseHeights()
    {
        for (int i = 0; i < heightMapResolution; i++)
            for (int j = 0; j < heightMapResolution; j++)
                bigMap[i, j] = float.MinValue;
    }

    private void SetHeightsInCorners()
    {
        System.Random prng = new System.Random(_seed);
        UnityEngine.Random.InitState(_seed);
        bigMap[0, 0] = (float)prng.NextDouble();
        bigMap[0, sizeMap] = (float)prng.NextDouble();
        bigMap[sizeMap, 0] = (float)prng.NextDouble();
        bigMap[sizeMap, sizeMap] = (float)prng.NextDouble();
    }

    private void CalculateDiamondSquareOnMap()
    {
        for (int i = 0; i < visibleHeightMapResolution; i++)
            for (int j = 0; j < visibleHeightMapResolution; j++)
            {
                bigMap[i + offsetX, j + offsetY] = GetHeightInPoint(i + offsetX, j + offsetY);
                visibleMap[i, j] = bigMap[i + offsetX, j + offsetY];
            }

        PostProcess();
    }

    private float GetHeightInPoint(int x, int y)
    {
        if (x < 0 || x > sizeMap || y < 0 || y > sizeMap)
            return outsideHeight;

        if (bigMap[x, y] != float.MinValue)
            return bigMap[x, y];

        var phigureSize = 1;

        while (((x & phigureSize) == 0) && ((y & phigureSize) == 0))
            phigureSize <<= 1;

        if (((x & phigureSize) != 0) && ((y & phigureSize) != 0))
            return CalcSquare(x, y, phigureSize * 2);
        else
            return CalcDiamond(x, y, phigureSize * 2);
    }
    private float CalcSquare(int x, int y, int lengthOfSide)
    {
        int halfSize = lengthOfSide / 2;

        bigMap[x, y] = AddRandomDisplacement((GetHeightInPoint(x - halfSize, y - halfSize) +
                     GetHeightInPoint(x + halfSize, y - halfSize) +
                     GetHeightInPoint(x + halfSize, y + halfSize) +
                     GetHeightInPoint(x - halfSize, y + halfSize)) / 4,
                            _roughness, lengthOfSide, x, y);

        return bigMap[x, y];
    }

    private float CalcDiamond(int x, int y, int lengthOfDiagonal)
    {
        int halfSize = lengthOfDiagonal / 2;

        bigMap[x, y] = AddRandomDisplacement((GetHeightInPoint(x, y - halfSize) +
                     GetHeightInPoint(x + halfSize, y) +
                     GetHeightInPoint(x, y + halfSize) +
                     GetHeightInPoint(x - halfSize, y)) / 4,
                            _roughness, lengthOfDiagonal, x, y);

        return bigMap[x, y];
    }

    private float AddRandomDisplacement(float val, float roughness, int sizeOfPhigure, int x, int y)
    {
        System.Random prng = new System.Random((x + heightMapResolution * y) * (x + heightMapResolution * y) * _seed);
        float rnd = (float)prng.NextDouble() - 0.5f;
        float height = (float)(val + rnd * ((float)sizeOfPhigure / sizeMap) * roughness);

        if(height > 1)
        {

        }
        return height;
    }

    private void PostProcess()
    {
        //возведение высот в квадрат
        for (int i = 0; i < visibleHeightMapResolution; i++)
            for (int j = 0; j < visibleHeightMapResolution; j++)
            {
                float square = Mathf.Pow(visibleMap[i, j], 2);
                visibleMap[i, j] = square;
            }

        //сглаживание
        for (int s = 0; s < _smoothCount; s++)
        {
            float[,] copyMap = new float[visibleHeightMapResolution, visibleHeightMapResolution];
            Array.Copy(visibleMap, 0, copyMap, 0, visibleMap.Length);
            for (int i = 0; i < visibleHeightMapResolution; i++)
                for (var j = 0; j < visibleHeightMapResolution; j++)
                {
                    float point1 = (i != 0 && j != 0) ? visibleMap[i - 1, j - 1] : 0;
                    float point2 = (i != 0) ? visibleMap[i - 1, j] : 0;
                    float point3 = (i != 0 && j + 1 != visibleHeightMapResolution) ? visibleMap[i - 1, j + 1] : 0;
                    float point4 = (j != 0) ? visibleMap[i, j - 1] : 0;
                    float point8 = (j + 1 != visibleHeightMapResolution) ? visibleMap[i, j + 1] : 0;
                    float point5 = (i + 1 != visibleHeightMapResolution && j != 0) ? visibleMap[i + 1, j - 1] : 0;
                    float point6 = (i + 1 != visibleHeightMapResolution) ? visibleMap[i + 1, j] : 0;
                    float point7 = (i + 1 != visibleHeightMapResolution && j + 1 != visibleHeightMapResolution) ? visibleMap[i + 1, j + 1] : 0;
                    List<float> neighborsHeights = new List<float> { point1, point2, point3, point4, point5, point6, point7, point8 };
                    copyMap[i, j] = neighborsHeights.Sum() / neighborsHeights.Count();
                }
            visibleMap = copyMap;
        }

        //нормализация
        for (var i = 0; i < visibleHeightMapResolution; i++)
            for (var j = 0; j < visibleHeightMapResolution; j++)
                visibleMap[i, j] = Mathf.InverseLerp(0, 100, visibleMap[i, j]);
    }
}
