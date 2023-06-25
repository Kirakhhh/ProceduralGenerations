using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static UnityEngine.Mesh;

public class VoronoiMapGenerator : MonoBehaviour
{

    [Header("Settings")]
    public int randomSeed = 0;
    public int mapSize = 200;
    public int distBetweenPoints = 10;
    public int relaxIterations = 1;
    public int textureSize = 512;
    public bool drawNodeBoundries;
    public bool drawDelauneyTriangles;
    public bool drawNodeCenters;
    public List<MapNodeTypeColor> colours;

    [Header("Perlin Noise Settings")]
    public float noiseScale = 50;
    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = .6f;
    public float lacunarity = 2;
    public Vector2 offset;
    public float heightMultiplier;
    public AnimationCurve heightCurve;

    [Header("Mesh Attributes")]
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;


    public void GenerateMap()
    {
        var points = GeneratePoints();

        var voronoiMap = new Delaunay.Voronoi(points, null, new Rect(0, 0, mapSize, mapSize), relaxIterations);

        var noiseHeightMap = PerlinNoise.GenerateNoiseMap(mapSize, randomSeed, noiseScale, octaves, persistance, lacunarity, offset, heightMultiplier, heightCurve);

        var mapGraph = new GraphGenerator(voronoiMap, noiseHeightMap);

        LandSelector.SelectLandForGraph(mapGraph);

        var meshData = MeshGenerator.GenerateMesh(mapGraph, noiseHeightMap, mapSize);
        UpdateMesh(meshData);

        var texture = TextureGenerator.GenerateTexture(mapGraph, mapSize, textureSize, colours, drawNodeBoundries, drawDelauneyTriangles, drawNodeCenters);
        UpdateTexture(texture);
    }

    private List<Vector2> GeneratePoints()
    {
        List<Vector2> points = null;
        var poisson = new PoissonDiscSampler(mapSize, mapSize, distBetweenPoints, randomSeed);
        points = poisson.Samples().ToList();

        return points;
    }

    public void UpdateMesh(MeshData meshData)
    {
        var mesh = new Mesh
        {
            vertices = meshData.vertices.ToArray(),
            triangles = meshData.indices.ToArray(),
            uv = meshData.uvs
        };
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    private void UpdateTexture(Texture2D texture)
    {
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

}

[Serializable]
public class MapNodeTypeColor
{
    public GraphGenerator.MapNodeType type;
    public UnityEngine.Color color;
}