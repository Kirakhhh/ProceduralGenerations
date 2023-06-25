using System.Collections.Generic;
using UnityEngine;


public static class TextureGenerator 
{
    private static Material drawingMaterial;

    private static void CreateDrawingMaterial()
    {
        if (!drawingMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            drawingMaterial = new Material(shader);
            drawingMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            drawingMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            drawingMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            drawingMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            drawingMaterial.SetInt("_ZWrite", 0);
        }
    }

    public static Texture2D GenerateTexture(GraphGenerator map, int meshSize, int textureSize, List<MapNodeTypeColor> colours, bool drawBoundries, bool drawTriangles, bool drawCenters)
    {
        CreateDrawingMaterial();
        var texture = RenderGLToTexture(map, textureSize, meshSize, drawingMaterial, colours, drawBoundries, drawTriangles, drawCenters);

        return texture;
    }

    private static Texture2D RenderGLToTexture(GraphGenerator map, int textureSize, int meshSize, Material material, List<MapNodeTypeColor> colours, bool drawBoundries, bool drawTriangles, bool drawCenters)
    {
        var renderTexture = CreateRenderTexture(textureSize, Color.white);

        // render GL immediately to the active render texture //
        DrawToRenderTexture(map, material, textureSize, meshSize, colours, drawBoundries, drawTriangles, drawCenters);

        return CreateTextureFromRenderTexture(textureSize, renderTexture);
    }

    private static Texture2D CreateTextureFromRenderTexture(int textureSize, RenderTexture renderTexture)
    {
        // read the active RenderTexture into a new Texture2D //
        Texture2D newTexture = new Texture2D(textureSize, textureSize);
        newTexture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);

        // apply pixels and compress //
        bool applyMipsmaps = false;
        newTexture.Apply(applyMipsmaps);
        bool highQuality = true;
        newTexture.Compress(highQuality);

        // clean up after the party //
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);

        // return the goods //
        return newTexture;
    }

    private static RenderTexture CreateRenderTexture(int textureSize, Color color)
    {
        // get a temporary RenderTexture //
        RenderTexture renderTexture = RenderTexture.GetTemporary(textureSize, textureSize);

        // set the RenderTexture as global target (that means GL too)
        RenderTexture.active = renderTexture;

        // clear GL //
        GL.Clear(false, true, color);
        GL.sRGBWrite = false;

        return renderTexture;
    }

    private static void DrawToRenderTexture(GraphGenerator map, Material material, int textureSize, int meshSize, List<MapNodeTypeColor> colours, bool drawBoundries, bool drawTriangles, bool drawCenters)
    {
        material.SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, meshSize, 0, meshSize);
        GL.Viewport(new Rect(0, 0, textureSize, textureSize));

        var coloursDictionary = new Dictionary<GraphGenerator.MapNodeType, Color>();
        foreach (var colour in colours)
        {
            if (!coloursDictionary.ContainsKey(colour.type)) coloursDictionary.Add(colour.type, colour.color);
        }

        /*
        var coloursDictionary = new Dictionary<MapGraph.MapNodeType, Color>
        {
            { MapGraph.MapNodeType.Beach, new Color(210f / 255f, 180f / 255f, 124f / 255f) },
            { MapGraph.MapNodeType.Grass, new Color(109f / 255f, 154f / 255f, 102f / 255f) },
            { MapGraph.MapNodeType.FreshWater, new Color(48f / 255f, 104f / 255f, 153f / 255f) },
            { MapGraph.MapNodeType.SaltWater, new Color(68f / 255f, 68f / 255f, 120f / 255f) },
            { MapGraph.MapNodeType.Mountain, new Color(162f / 255f, 99f / 255f, 68f / 255f) },
            { MapGraph.MapNodeType.Snow, new Color(248f / 255f, 248f / 255f, 248f / 255f) },
            { MapGraph.MapNodeType.City, Color.gray },
            { MapGraph.MapNodeType.Error, Color.red }
        };
        */

        DrawNodeTypes(map, coloursDictionary);

        if (drawCenters) DrawCenterPoints(map, Color.red);
        if (drawBoundries) DrawEdges(map, Color.black);
        //DrawRivers(map, 2, coloursDictionary[GraphGeneration.MapNodeType.FreshWater]);
        if (drawTriangles) DrawDelauneyEdges(map, Color.red);

        GL.PopMatrix();
    }

    private static void DrawEdges(GraphGenerator map, Color color)
    {
        GL.Begin(GL.LINES);
        GL.Color(color);

        foreach (var edge in map.edges)
        {
            var start = edge.GetStartPosition();
            var end = edge.GetEndPosition();

            GL.Vertex3(start.x, start.z, 0);
            GL.Vertex3(end.x, end.z, 0);
        }

        GL.End();
    }

    private static void DrawDelauneyEdges(GraphGenerator map, Color color)
    {
        GL.Begin(GL.LINES);
        GL.Color(color);

        foreach (var edge in map.edges)
        {
            if (edge.neighbor != null)
            {
                var start = edge.node.centerPoint;
                var end = edge.neighbor.node.centerPoint;

                GL.Vertex3(start.x, start.z, 0);
                GL.Vertex3(end.x, end.z, 0);
            }
        }

        GL.End();
    }

    private static void DrawCenterPoints(GraphGenerator map, Color color)
    {
        GL.Begin(GL.QUADS);
        GL.Color(color);

        foreach (var point in map.nodesByCenterPosition.Values)
        {
            var x = point.centerPoint.x;
            var y = point.centerPoint.z;
            GL.Vertex3(x - .25f, y - .25f, 0);
            GL.Vertex3(x - .25f, y + .25f, 0);
            GL.Vertex3(x + .25f, y + .25f, 0);
            GL.Vertex3(x + .25f, y - .25f, 0);
        }

        GL.End();
    }

    private static void DrawNodeTypes(GraphGenerator map, Dictionary<GraphGenerator.MapNodeType, Color> colours)
    {
        GL.Begin(GL.TRIANGLES);
        foreach (var node in map.nodesByCenterPosition.Values)
        {
            var color = colours.ContainsKey(node.nodeType) ? colours[node.nodeType] : Color.red;
            GL.Color(color);

            foreach (var edge in node.GetEdges())
            {
                var start = edge.previous.destination.position;
                var end = edge.destination.position;
                GL.Vertex3(node.centerPoint.x, node.centerPoint.z, 0);
                GL.Vertex3(start.x, start.z, 0);
                GL.Vertex3(end.x, end.z, 0);
            }
        }
        GL.End();
    }
}
