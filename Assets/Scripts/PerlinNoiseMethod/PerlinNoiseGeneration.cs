using UnityEngine;

public class PerlinNoiseGeneration : MonoBehaviour
{
	public enum DrawMode { NoiseMap, Terrain };
	public DrawMode drawMode;

	public Renderer textureRender;
	public Terrain Terrain;
	public PaintTerrain paintTerrain;

	public int heightMapResolutionPower;
	public float noiseScale;
	
	public int octaves;
	[Range(0, 1)]
	public float persistence;
	public float lacunarity;

	public int seed;
	public Vector2 offset;
	public float waterLevel;

	public void GenerateMap()
	{
		int heightMapResolution = (int)Mathf.Pow(2, heightMapResolutionPower) + 1;
		float[,] noiseHeightMap = GenerateNoiseMap(heightMapResolution, seed, noiseScale, octaves, persistence, lacunarity, offset);

		if (drawMode == DrawMode.NoiseMap)
        {
			Texture2D texture = TextureFromHeightMap(noiseHeightMap);
			textureRender.sharedMaterial.mainTexture = texture;
			textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
		}
			
		if (drawMode == DrawMode.Terrain)
        {
			Terrain.terrainData.SetHeights(0, 0, noiseHeightMap);

			paintTerrain.StartPaint();

			var waterObj = GameObject.Find("Water");
			waterObj.transform.position = new Vector3(waterObj.transform.position.x, waterLevel * Terrain.terrainData.size.y, waterObj.transform.position.z);

		}
    }

	public float[,] GenerateNoiseMap(int mapSize, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
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

		float halfSize = mapSize / 2f; // для приближения в центр шума

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
				noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]); //приведение к числам от 0 до 1, т.е. нормализация

		return noiseMap;
	}

	public static Texture2D TextureFromHeightMap(float[,] heightMap)
	{
		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(1);

		Color[] colorMap = new Color[width * height];
		for (int y = 0; y < height; y++)
			for (int x = 0; x < width; x++)
				colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);

		Texture2D texture = new Texture2D(width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels(colorMap);
		texture.Apply();
		return texture;
	}
	void OnValidate()
	{
		if (lacunarity < 1)
			lacunarity = 1;
		if (octaves < 0)
			octaves = 0;
	}
}
