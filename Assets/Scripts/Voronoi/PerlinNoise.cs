using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class PerlinNoise : MonoBehaviour
{

    public static NoiseHeightMap GenerateNoiseMap(int mapSize, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, float heightMultiplier, AnimationCurve heightCurve)
    {
        float[,] noiseMap = new float[mapSize, mapSize];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
            scale = 0.0001f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfSize = mapSize / 2f;

        for (int y = 0; y < mapSize; y++)
            for (int x = 0; x < mapSize; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfSize) / scale * frequency + octaveOffsets[i].x * frequency;
                    float sampleY = (y - halfSize) / scale * frequency + octaveOffsets[i].y * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }

        for (int y = 0; y < mapSize; y++)
            for (int x = 0; x < mapSize; x++)
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);


        float[,] fallofMap = GenerateFalloffMap(mapSize);

        AnimationCurve threadSafeHeightCurve = new AnimationCurve(heightCurve.keys);

        maxNoiseHeight = float.MinValue;
        minNoiseHeight = float.MaxValue;

        for (int y = 0; y < mapSize; y++)
            for (int x = 0; x < mapSize; x++)
            {
                noiseMap[x, y] -= fallofMap[x, y];

                noiseMap[x, y] *= threadSafeHeightCurve.Evaluate(noiseMap[x, y]) *  heightMultiplier;

                if (noiseMap[x, y] > maxNoiseHeight)
                    maxNoiseHeight = noiseMap[x, y];
                else if (noiseMap[x, y] < minNoiseHeight)
                    minNoiseHeight = noiseMap[x, y];
            }

        return new NoiseHeightMap(noiseMap, minNoiseHeight, maxNoiseHeight);
    }

    public static float[,] GenerateFalloffMap(int mapSize)
    {
        float[,] map = new float[mapSize, mapSize];

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                float x = i / (float)mapSize * 2 - 1;
                float y = j / (float)mapSize * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    static float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}

public struct NoiseHeightMap
{
    public float[,] map;
    public float min;
    public float max;

    public NoiseHeightMap(float[,] map, float min, float max)
    {
        this.map = map;
        this.min = min;
        this.max = max;
    }
}
