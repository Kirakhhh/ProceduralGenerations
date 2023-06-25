using Delaunay;
using Delaunay.Geo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGenerator
{
    public enum MapNodeType
    {
        FreshWater,
        SaltWater,
        Grass,
        Mountain,
        Beach,
        Error,
        Snow
    }

    public Rect plotBounds;

    public Dictionary<Vector3, MapPoint> points;

    public Dictionary<Vector3, MapNode> nodesByCenterPosition;

    public List<MapNodeEdge> edges;

    public GraphGenerator(Voronoi voronoi, NoiseHeightMap heightMap)
    {
        CreateFromVoronoi(voronoi);

        UpdateHeights(heightMap);
    }

    private void CreateFromVoronoi(Voronoi voronoi)
    {
        // предположительно начала ребер по координатам
        points = new Dictionary<Vector3, MapPoint>();

        // ячейки по координатам их центра, все понятно
        nodesByCenterPosition = new Dictionary<Vector3, MapNode>();

        // по координате находим начинающуюся оттуда группу ребер описывающих Node
        var edgesByStartPosition = new Dictionary<Vector3, List<MapNodeEdge>>();

        // лист ребер
        edges = new List<MapNodeEdge>();

        // границы квадрата в котором происходит движ
        plotBounds = voronoi.plotBounds;

        // крайние угловые сайты (точечки изначальные вокруг которых делали диаграмму)
        var bottomLeftSite = voronoi.NearestSitePoint(voronoi.plotBounds.xMin, voronoi.plotBounds.yMin);
        var bottomRightSite = voronoi.NearestSitePoint(voronoi.plotBounds.xMax, voronoi.plotBounds.yMin);
        var topLeftSite = voronoi.NearestSitePoint(voronoi.plotBounds.xMin, voronoi.plotBounds.yMax);
        var topRightSite = voronoi.NearestSitePoint(voronoi.plotBounds.xMax, voronoi.plotBounds.yMax);

        // угловые координаты
        var topLeft = new Vector3(voronoi.plotBounds.xMin, 0, voronoi.plotBounds.yMax);
        var topRight = new Vector3(voronoi.plotBounds.xMax, 0, voronoi.plotBounds.yMax);
        var bottomLeft = new Vector3(voronoi.plotBounds.xMin, 0, voronoi.plotBounds.yMin);
        var bottomRight = new Vector3(voronoi.plotBounds.xMax, 0, voronoi.plotBounds.yMin);

        //ребра диаграммы вороного относительно коордов сайтов (точек)
        var siteEdges = new Dictionary<Vector2, List<LineSegment>>();

        var edgePointsRemoved = 0;

        //для каждого ребрышка в диаграмме вороного
        foreach (var edge in voronoi.Edges())
        {
            // невидимые за полем
            if (edge.visible)
            {

                //координаты ребра диаграммы вороного (обрезанные)
                var p1 = edge.clippedEnds[Delaunay.LR.Side.LEFT];
                var p2 = edge.clippedEnds[Delaunay.LR.Side.RIGHT];
                var segment = new LineSegment(p1, p2);

                //удаляем слишком маленькие ребра
                if (Vector2.Distance(p1.Value, p2.Value) < 0.001f)
                {
                    edgePointsRemoved++;
                    continue;
                }

                //добавляем ребра в дикшинари если их там нет по этому ключу
                if (edge.leftSite != null)
                {
                    if (!siteEdges.ContainsKey(edge.leftSite.Coord))
                        siteEdges.Add(edge.leftSite.Coord, new List<LineSegment>());

                    siteEdges[edge.leftSite.Coord].Add(segment);
                }
                if (edge.rightSite != null)
                {
                    if (!siteEdges.ContainsKey(edge.rightSite.Coord))
                        siteEdges.Add(edge.rightSite.Coord, new List<LineSegment>());

                    siteEdges[edge.rightSite.Coord].Add(segment);
                }
            }
        }
        //Debug.Assert(edgePointsRemoved == 0, string.Format("{0} edge points too close and have been removed", edgePointsRemoved));

        // для каждого сайта в диаграмме вороного
        foreach (var site in voronoi.SiteCoords())
        {
            //берем границы сайта (ребра графа вороного) отсортированные и направленные по часовой стрелке
            var boundries = GetBoundriesForSite(siteEdges, site);

            // центр сайта
            var center = ToVector3(site);

            // создаем ячейку для этого сайта
            var currentNode = new MapNode { centerPoint = center };
            nodesByCenterPosition.Add(center, currentNode);

            MapNodeEdge firstEdge = null;
            MapNodeEdge previousEdge = null;

            for (var i = 0; i < boundries.Count; i++)
            {
                var edge = boundries[i];

                var start = ToVector3(edge.p0.Value);
                var end = ToVector3(edge.p1.Value);
                if (start == end) continue;

                // создаем ребро во всех списках и задаем первое ребро для нода
                previousEdge = AddEdge(edgesByStartPosition, previousEdge, start, end, currentNode);
                if (firstEdge == null) firstEdge = previousEdge;
                if (currentNode.startEdge == null) currentNode.startEdge = previousEdge;

                // Нам нужно выяснить, встречаются ли два ребра, и если нет, то вставить еще несколько ребер, чтобы замкнуть многоугольник (для краев карты)
                var insertEdges = false;
                if (i < boundries.Count - 1)
                {
                    start = ToVector3(boundries[i].p1.Value);
                    end = ToVector3(boundries[i + 1].p0.Value);
                    insertEdges = start != end;
                }
                else if (i == boundries.Count - 1)
                {
                    start = ToVector3(boundries[i].p1.Value);
                    end = ToVector3(boundries[0].p0.Value);
                    insertEdges = start != end;
                }

                if (insertEdges)
                {

                    var startIsTop = start.z == voronoi.plotBounds.yMax;
                    var startIsBottom = start.z == voronoi.plotBounds.yMin;
                    var startIsLeft = start.x == voronoi.plotBounds.xMin;
                    var startIsRight = start.x == voronoi.plotBounds.xMax;

                    var hasTopLeft = site == topLeftSite && !(startIsTop && startIsLeft);
                    var hasTopRight = site == topRightSite && !(startIsTop && startIsRight);
                    var hasBottomLeft = site == bottomLeftSite && !(startIsBottom && startIsLeft);
                    var hasBottomRight = site == bottomRightSite && !(startIsBottom && startIsRight);

                    if (startIsTop)
                    {
                        if (hasTopRight) previousEdge = AddEdge(edgesByStartPosition, previousEdge, start, topRight, currentNode);
                        if (hasBottomRight) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, bottomRight, currentNode);
                        if (hasBottomLeft) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, bottomLeft, currentNode);
                        if (hasTopLeft) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, topLeft, currentNode);

                    }
                    else if (startIsRight)
                    {
                        if (hasBottomRight) previousEdge = AddEdge(edgesByStartPosition, previousEdge, start, bottomRight, currentNode);
                        if (hasBottomLeft) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, bottomLeft, currentNode);
                        if (hasTopLeft) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, topLeft, currentNode);
                        if (hasTopRight) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, topRight, currentNode);
                    }
                    else if (startIsBottom)
                    {
                        if (hasBottomLeft) previousEdge = AddEdge(edgesByStartPosition, previousEdge, start, bottomLeft, currentNode);
                        if (hasTopLeft) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, topLeft, currentNode);
                        if (hasTopRight) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, topRight, currentNode);
                        if (hasBottomRight) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, bottomRight, currentNode);
                    }
                    else if (startIsLeft)
                    {
                        if (hasTopLeft) previousEdge = AddEdge(edgesByStartPosition, previousEdge, start, topLeft, currentNode);
                        if (hasTopRight) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, topRight, currentNode);
                        if (hasBottomRight) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, bottomRight, currentNode);
                        if (hasBottomLeft) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, bottomLeft, currentNode);
                    }

                    previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, end, currentNode);
                }
            }

            // замыкаем нод
            previousEdge.next = firstEdge;
            firstEdge.previous = previousEdge;
            //AddLeavingEdge(firstEdge);
        }

        // отмечаем ребра разных нодов которые соприкасаются
        ConnectOpposites(edgesByStartPosition);
    }


    private void UpdateHeights(NoiseHeightMap heightmap)
    {
        foreach (var node in nodesByCenterPosition.Values)
        {
            node.centerPoint = UpdateHeight(heightmap, node.centerPoint);
        }
        foreach (var point in points.Values)
        {
            point.position = UpdateHeight(heightmap, point.position);
        }
    }

    private static Vector3 UpdateHeight(NoiseHeightMap heightmap, Vector3 oldPosition)
    {
        var position = oldPosition;
        var x = Mathf.FloorToInt(position.x);
        var y = Mathf.FloorToInt(position.z);
        if (x >= 0 && y >= 0 && x < heightmap.map.GetLength(0) && y < heightmap.map.GetLength(1))
        {
            position.y = heightmap.map[x, y];
        }
        return position;
    }

    private List<LineSegment> GetBoundriesForSite(Dictionary<Vector2, List<LineSegment>> siteEdges, Vector2 site)
    {
        var boundries = siteEdges[site];

        // Sort boundries clockwise
        boundries = FlipClockwise(boundries, site);
        boundries = SortClockwise(boundries, site);
        boundries = SnapBoundries(boundries, 0.001f);

        return boundries;
    }

    private static List<LineSegment> SnapBoundries(List<LineSegment> boundries, float snapDistance)
    {
        for (int i = boundries.Count - 1; i >= 0; i--)
        {
            if (Vector2.Distance(boundries[i].p0.Value, boundries[i].p1.Value) < snapDistance)
            {
                var previous = i - 1;
                var next = i + 1;
                if (previous < 0) previous = boundries.Count - 1;
                if (next >= boundries.Count) next = 0;

                if (Vector2.Distance(boundries[previous].p1.Value, boundries[next].p0.Value) < snapDistance)
                {
                    boundries[previous].p1 = boundries[next].p0;
                }
                boundries.Remove(boundries[i]);
            }
        }
        return boundries;
    }

    private void ConnectOpposites(Dictionary<Vector3, List<MapNodeEdge>> edgesByStartPosition)
    {
        foreach (var edge in edges)
        {
            if (edge.neighbor == null)
            {
                var startEdgePosition = edge.previous.destination.position;
                var endEdgePosition = edge.destination.position;

                if (edgesByStartPosition.ContainsKey(endEdgePosition))
                {
                    var list = edgesByStartPosition[endEdgePosition];
                    MapNodeEdge opposite = null;
                    foreach (var item in list)
                    {
                        // We use .5f to snap the coordinates to each other, otherwise there are holes in the graph
                        if (Math.Abs(item.destination.position.x - startEdgePosition.x) < 0.5f && Math.Abs(item.destination.position.z - startEdgePosition.z) < 0.5f)
                        {
                            opposite = item;
                        }
                    }
                    if (opposite != null)
                    {
                        edge.neighbor = opposite;
                        opposite.neighbor = edge;
                    }
                    else
                    {
                        // TODO: We need to check that this is at the world boundry, otherwise it's a bug
                        var isAtEdge = endEdgePosition.x == 0 || endEdgePosition.x == plotBounds.width || endEdgePosition.z == 0 || endEdgePosition.z == plotBounds.height ||
                            startEdgePosition.x == 0 || startEdgePosition.x == plotBounds.width || startEdgePosition.z == 0 || startEdgePosition.z == plotBounds.height;

                        if (!isAtEdge)
                        {
                            edge.node.nodeType = MapNodeType.Error;
                            Debug.Assert(isAtEdge, "Edges without opposites must be at the boundry edge");
                        }
                    }
                }
            }
        }
    }

    private List<LineSegment> SortClockwise(List<LineSegment> segments, Vector2 center)
    {
        segments.Sort((line1, line2) =>
        {
            var firstVector = line1.p0.Value - center;
            var secondVector = line2.p0.Value - center;
            var angle = Vector2.SignedAngle(firstVector, secondVector);

            return angle > 0 ? 1 : (angle < 0 ? -1 : 0);
        });
        return segments;
    }

    private List<LineSegment> FlipClockwise(List<LineSegment> segments, Vector2 center)
    {
        var newSegments = new List<LineSegment>();
        foreach (var line in segments)
        {
            var firstVector = line.p0.Value - center;
            var secondVector = line.p1.Value - center;
            var angle = Vector2.SignedAngle(firstVector, secondVector);

            if (angle > 0) newSegments.Add(new LineSegment(line.p1, line.p0));
            else newSegments.Add(new LineSegment(line.p0, line.p1));
        }
        return newSegments;
    }

    private MapNodeEdge AddEdge(Dictionary<Vector3, List<MapNodeEdge>> edgesByStartPosition, MapNodeEdge previous, Vector3 start, Vector3 end, MapNode node)
    {
        //if (start == end)
        //{
        //    Debug.Assert(start != end, "Start and end vectors must not be the same");
        //}
        var currentEdge = new MapNodeEdge { node = node };

        if (!points.ContainsKey(start)) points.Add(start, new MapPoint { position = start });
        if (!points.ContainsKey(end)) points.Add(end, new MapPoint { position = end });

        currentEdge.destination = points[end];

        if (!edgesByStartPosition.ContainsKey(start)) edgesByStartPosition.Add(start, new List<MapNodeEdge>());
        edgesByStartPosition[start].Add(currentEdge);
        edges.Add(currentEdge);

        if (previous != null)
        {
            previous.next = currentEdge;
            currentEdge.previous = previous;
            //AddLeavingEdge(currentEdge);
        }
        return currentEdge;
    }

    private Vector3 ToVector3(Vector2 vector)
    {
        return new Vector3(Mathf.Round(vector.x * 1000f) / 1000f, 0, Mathf.Round(vector.y * 1000f) / 1000f);
        //return new Vector3(vector.x, 0f, vector.y);
    }

}
