using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintTerrain : MonoBehaviour
{
    public float GrassLevel;
    public float MountainLevel;
    public float SnowLevel;

    private Dictionary<int, int> TextureStartingHeight = new Dictionary<int, int>();

    public Terrain Terrain;

    public void StartPaint()
    {
        TerrainData terrainData = Terrain.terrainData;
        TextureStartingHeight.Clear();
        TextureStartingHeight.Add(0, 0);
        TextureStartingHeight.Add(1, (int)(GrassLevel * terrainData.size.y));
        TextureStartingHeight.Add(2, (int)(MountainLevel * terrainData.size.y));
        TextureStartingHeight.Add(3, (int)(SnowLevel * terrainData.size.y));
        int countOfTextureLevels = TextureStartingHeight.Count;

        float[,,] splatMapData = new float[terrainData.alphamapWidth,
                                           terrainData.alphamapHeight,
                                           terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float terrainHeight = terrainData.GetHeight(y, x);

                float[] splat = new float[countOfTextureLevels];

                for (int i = 0; i < countOfTextureLevels; i++)
                {
                    if (i == countOfTextureLevels - 1 && terrainHeight >= TextureStartingHeight[i])
                    {
                        splat[i] = 1;
                    }
                    else if (terrainHeight >= TextureStartingHeight[i] &&
                        terrainHeight < TextureStartingHeight[i + 1])
                    {
                        splat[i] = 1;
                    }
                }

                for (int j = 0; j < countOfTextureLevels; j++)
                {
                    splatMapData[x, y, j] = splat[j];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatMapData);
    }
}
