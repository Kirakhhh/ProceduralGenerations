using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GraphGenerator;

public class MapPoint
{
    public Vector3 position { get; set; }
}

public class MapNode
{
    private float? _heightDifference;

    public Vector3 centerPoint { get; set; }
    public MapNodeEdge startEdge { get; set; }
    public MapNodeType nodeType { get; set; }
    public IEnumerable<MapNodeEdge> GetEdges()
    {
        yield return startEdge;

        var next = startEdge.next;
        while (next != startEdge)
        {
            yield return next;
            next = next.next;
        }
    }

    public IEnumerable<MapPoint> GetCorners()
    {
        yield return startEdge.destination;

        var next = startEdge.next;
        while (next != startEdge)
        {
            yield return next.destination;
            next = next.next;
        }
    }

    public bool IsEdge()
    {
        foreach (var edge in GetEdges())
        {
            if (edge.neighbor == null) return true;
        }
        return false;
    }

    public float GetElevation()
    {
        return centerPoint.y;
    }

    public float GetHeightDifference()
    {
        if (!_heightDifference.HasValue)
        {
            var lowestY = centerPoint.y;
            var highestY = centerPoint.y;
            foreach (var corner in GetCorners())
            {
                if (corner.position.y > highestY) highestY = corner.position.y;
                if (corner.position.y < lowestY) lowestY = corner.position.y;
            }
            _heightDifference = highestY - lowestY;
        }
        return _heightDifference.Value;
    }

    public List<MapNode> GetNeighborNodes()
    {
        return GetEdges().Where(x => x.neighbor != null && x.neighbor.node != null).Select(x => x.neighbor.node).ToList();
    }

}


public class MapNodeEdge
{
    public MapPoint destination { get; set; }
    public MapNodeEdge next { get; set; }
    public MapNodeEdge previous { get; set; }
    public MapNodeEdge neighbor { get; set; }

    public MapNode node;

    public Vector3 GetStartPosition()
    {
        return previous.destination.position;
    }

    public Vector3 GetEndPosition()
    {
        return destination.position;
    }

}