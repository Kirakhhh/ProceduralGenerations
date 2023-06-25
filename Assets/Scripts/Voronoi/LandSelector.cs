using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LandSelector 
{
    public static void SelectLandForGraph(GraphGenerator graph)
    {
        SetNodesToGrass(graph);
        SetLowNodesToWater(graph, 0.2f);
        SetEdgesToWater(graph);
        FillOcean(graph);
        SetBeaches(graph);
        AddMountains(graph);
        AverageCenterPoints(graph);
    }

    private static void SetNodesToGrass(GraphGenerator graph)
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            if (node.nodeType != GraphGenerator.MapNodeType.Error) node.nodeType = GraphGenerator.MapNodeType.Grass;
        }
    }

    private static void SetEdgesToWater(GraphGenerator graph)
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            if (node.IsEdge()) node.nodeType = GraphGenerator.MapNodeType.FreshWater;
        }
    }

    private static void SetLowNodesToWater(GraphGenerator graph, float cutoff)
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            if (node.centerPoint.y <= cutoff)
            {
                var allZero = true;
                foreach (var edge in node.GetEdges())
                {
                    if (edge.destination.position.y > cutoff)
                    {
                        allZero = false;
                        break;
                    }
                }
                if (allZero && node.nodeType != GraphGenerator.MapNodeType.Error) 
                    node.nodeType = GraphGenerator.MapNodeType.FreshWater;
            }
        }
    }

    private static void AverageCenterPoints(GraphGenerator graph)
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            node.centerPoint = new Vector3(node.centerPoint.x, node.GetCorners().Average(x => x.position.y), node.centerPoint.z);
        }
    }

    private static void AddMountains(GraphGenerator graph)
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            if (node.GetElevation() > 15f || node.GetHeightDifference() > 7f)
            {
                node.nodeType = GraphGenerator.MapNodeType.Mountain;
            }
            if (node.GetElevation() > 17f)
            {
                node.nodeType = GraphGenerator.MapNodeType.Snow;
            }
        }
    }

    private static void FillOcean(GraphGenerator graph)
    {
        var startNode = graph.nodesByCenterPosition.FirstOrDefault(x => x.Value.IsEdge() && x.Value.nodeType == GraphGenerator.MapNodeType.FreshWater).Value;
        FloodFill(startNode, GraphGenerator.MapNodeType.FreshWater, GraphGenerator.MapNodeType.SaltWater);
    }

    private static void FloodFill(MapNode node, GraphGenerator.MapNodeType targetType, GraphGenerator.MapNodeType replacementType)
    {
        if (targetType == replacementType) return;
        if (node.nodeType != targetType) return;
        node.nodeType = replacementType;
        foreach (var neighbor in node.GetNeighborNodes())
        {
            FloodFill(neighbor, targetType, replacementType);
        }
    }

    private static void SetBeaches(GraphGenerator graph)
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            if (node.nodeType == GraphGenerator.MapNodeType.Grass)
            {
                foreach (var neighbor in node.GetNeighborNodes())
                {
                    if (neighbor.nodeType == GraphGenerator.MapNodeType.SaltWater)
                    {
                        if (node.GetHeightDifference() < 0.8f)
                        {
                            node.nodeType = GraphGenerator.MapNodeType.Beach;
                        }
                        break;
                    }
                }
            }
        }
    }

}
