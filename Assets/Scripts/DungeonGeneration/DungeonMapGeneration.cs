using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using Random = UnityEngine.Random;


public class DungeonMapGeneration : MonoBehaviour
{
    public int seed;
    public GameObject room;
    public GameObject dote;
    public GameObject line;
    public LineRenderer lineRend;
    public GameObject[] floorTiles;
    public GameObject[] outerWallTiles;

    private Transform boardHolder;
    private Transform corridorHolder;
    private Transform tilesHolder;
    private Transform generationHolder;

    private Dictionary<int, Vector3> mainRoomsCoords = new Dictionary<int, Vector3>();
    private Dictionary<int, Tuple<float, float>> mainRoomsScales = new Dictionary<int, Tuple<float, float>>();
    public List<Tuple<int, int>> pointsOfMainCorridors = new List<Tuple<int, int>>();
    MinSpanTree minSpanTree;
    Triangulation triangulation;

    (float,float) getRandomPointInCircle(int radius)
    {
        double r = radius * Math.Sqrt(Random.value);
        double theta = 2 * Math.PI * Random.value;
        double x = 0 + r * Math.Cos(theta);
        double y = 0 + r * Math.Sin(theta);
        return ((float)x, (float)y);
    }

    IEnumerator BoardSetup()
    {
        int numOfRooms = 100;
        int averageWidth = 0;
        int averageHight = 0;
        int tempWidth;
        int tempHight;
        generationHolder = GameObject.Find("Dungeon Generator").transform;

        //удаление дочерних элементов прошлой генерации
        GameObject[] allChildren = new GameObject[transform.childCount];
        int childNum = 0;
        foreach (Transform child in transform)
        {
            allChildren[childNum] = child.gameObject;
            childNum += 1;
        }
        foreach (GameObject child in allChildren)
        {
            Destroy(child.gameObject);
        }

        boardHolder = new GameObject("Board").transform;
        tilesHolder = new GameObject("Tiles").transform;
        corridorHolder = new GameObject("Corridors").transform;
        boardHolder.SetParent(generationHolder);
        tilesHolder.SetParent(generationHolder);
        corridorHolder.SetParent(generationHolder);
        Random.InitState(seed);

        for (int i = 0; i < numOfRooms; i++)
        {
            GameObject instance = Instantiate(room, new Vector3(getRandomPointInCircle(30).Item1, getRandomPointInCircle(30).Item2, 0f), Quaternion.identity) as GameObject;
            tempWidth = Random.Range(3, 20);
            tempHight = Random.Range(3, 20);
            instance.transform.localScale += new Vector3(tempWidth, tempHight, 1f);
            instance.GetComponent<SpriteRenderer>().color = new Color(Random.value, Random.value, Random.value, 1);
            instance.name = "physRoom" + i;
            instance.transform.SetParent(boardHolder);

            averageWidth += tempWidth;
            averageHight += tempHight;
        }
        averageWidth /= numOfRooms;
        averageHight /= numOfRooms;


        //ускорение обработки столкновений
        Time.timeScale = 10.0f;
        yield return new WaitForSeconds(60); // пауза для просчитывания положения комнат
        Time.timeScale = 1.0f; 

        for (int i = 0; i < numOfRooms; i++)
        {
            Transform mainRoom = boardHolder.Find("physRoom" + i);
            float mainRoomX = mainRoom.transform.position.x;
            float mainRoomY = mainRoom.transform.position.y;
            float mainRoomScaleX = mainRoom.transform.localScale.x;
            float mainRoomScaleY = mainRoom.transform.localScale.y;
            mainRoomX = mainRoomScaleX % 2 == 0 ? (float)Math.Round(mainRoomX) - 0.5f: (float)Math.Floor(mainRoomX) ;
            mainRoomY = mainRoomScaleY % 2 == 0 ? (float)Math.Round(mainRoomY) - 0.5f : (float)Math.Floor(mainRoomY) ;

            GameObject instance = Instantiate(dote, new Vector3(mainRoomX, mainRoomY, 0f), Quaternion.identity) as GameObject;
            instance.transform.localScale = new Vector3(mainRoomScaleX, mainRoomScaleY, 0f);
            instance.GetComponent<SpriteRenderer>().color = mainRoom.GetComponent<SpriteRenderer>().color;
            Destroy(GameObject.Find("physRoom" + i));
            instance.name = "room" + i;
            instance.transform.SetParent(boardHolder);
            mainRoom = boardHolder.Find("room" + i);
            
            if (mainRoom.transform.localScale.x >= 1.5 * averageWidth && mainRoom.transform.localScale.y >= 1.5 * averageHight)
            {
                mainRoomsCoords.Add(i, new Vector3(mainRoom.transform.position.x, mainRoom.transform.position.y, 0f));
                mainRoomsScales.Add(i, Tuple.Create(mainRoom.transform.localScale.x, mainRoom.transform.localScale.y));

                instance.GetComponent<SpriteRenderer>().color = new Color(100, 0, 0, 1);
                instance.transform.name = "MainRoom" + i;
                GameObject instance2 = Instantiate(dote, new Vector3(mainRoom.transform.position.x, mainRoom.transform.position.y, 0f), Quaternion.identity) as GameObject;
                instance2.transform.localScale += new Vector3(0.1f, 0.1f, 0f);
                instance2.name = "neighbors: ";
                instance2.GetComponent<SpriteRenderer>().sortingOrder = -1;
                instance2.transform.SetParent(mainRoom);
            }
        }
        
        triangulation = new Triangulation(mainRoomsCoords);
        for (int i = 0; i < triangulation.pointsOfTriangulationVectors.Count; i++)
        {
            GameObject instance = Instantiate(line, new Vector3(0f, 0f, 1f), Quaternion.identity) as GameObject;
            lineRend = instance.transform.GetComponent<LineRenderer>();
            instance.name = "Edge " + triangulation.pointsOfTriangulationVectors[i].Item1 + "," + triangulation.pointsOfTriangulationVectors[i].Item2;
            lineRend.positionCount = 2;
            lineRend.SetPosition(0, new Vector3(mainRoomsCoords[triangulation.pointsOfTriangulationVectors[i].Item1].x, mainRoomsCoords[triangulation.pointsOfTriangulationVectors[i].Item1].y, 0));
            lineRend.SetPosition(1, new Vector3(mainRoomsCoords[triangulation.pointsOfTriangulationVectors[i].Item2].x, mainRoomsCoords[triangulation.pointsOfTriangulationVectors[i].Item2].y, 0));
        }

        minSpanTree = new MinSpanTree(triangulation);
        for (int i = 0; i < minSpanTree.pointsOfMinSpanTreeVectors.Count; i++)
        {
            //визуализация ребер графа 
            //GameObject line = GameObject.Find("Edge " + minSpanTree.pointsOfMinSpanTreeVectors[i].Item1 + "," + minSpanTree.pointsOfMinSpanTreeVectors[i].Item2);
            //line.name = "MainEdge " + minSpanTree.pointsOfMinSpanTreeVectors[i].Item1 + "," + minSpanTree.pointsOfMinSpanTreeVectors[i].Item2;
            //lineRend = line.GetComponent<LineRenderer>();
            //lineRend.startColor = Color.blue;
            //lineRend.endColor = Color.blue;
            Transform mainRoom1 = boardHolder.Find("MainRoom" + minSpanTree.pointsOfMinSpanTreeVectors[i].Item1);
            Transform mainRoom2 = boardHolder.Find("MainRoom" + minSpanTree.pointsOfMinSpanTreeVectors[i].Item2);
            mainRoom1.GetChild(0).name += minSpanTree.pointsOfMinSpanTreeVectors[i].Item2 + " ";
            mainRoom2.GetChild(0).name += minSpanTree.pointsOfMinSpanTreeVectors[i].Item1 + " ";
            Destroy(GameObject.Find("Edge " + minSpanTree.pointsOfMinSpanTreeVectors[i].Item1 + "," + minSpanTree.pointsOfMinSpanTreeVectors[i].Item2));
            pointsOfMainCorridors.Add(minSpanTree.pointsOfMinSpanTreeVectors[i]);
        }

        for (int i = 0; i < triangulation.pointsOfTriangulationVectors.Count; i++)
        {
            GameObject line = GameObject.Find("Edge " + triangulation.pointsOfTriangulationVectors[i].Item1 + "," + triangulation.pointsOfTriangulationVectors[i].Item2);
            if (line)
            {
                if (Random.Range(0, 4) == 2) // шанс 25% вернуть ребро
                {
                    //визуализация ребер графа
                    //line.name = "randMainEdge " + triangulation.pointsOfTriangulationVectors[i].Item1 + "," + triangulation.pointsOfTriangulationVectors[i].Item2;
                    //lineRend = line.GetComponent<LineRenderer>();
                    //lineRend.SetColors(Color.blue, Color.blue);
                    Transform mainRoom1 = boardHolder.Find("MainRoom" + triangulation.pointsOfTriangulationVectors[i].Item1);
                    Transform mainRoom2 = boardHolder.Find("MainRoom" + triangulation.pointsOfTriangulationVectors[i].Item2);

                    mainRoom1.GetChild(0).name += triangulation.pointsOfTriangulationVectors[i].Item2 + " ";
                    mainRoom2.GetChild(0).name += triangulation.pointsOfTriangulationVectors[i].Item1 + " ";
                    pointsOfMainCorridors.Add(triangulation.pointsOfTriangulationVectors[i]);
                }
                Destroy(line);
            }
        }
       

        for (int i = 0; i < pointsOfMainCorridors.Count; i++)
        {
            var xRoom1 = (float)Math.Round(mainRoomsCoords[pointsOfMainCorridors[i].Item1].x); // координаты центров точек отрезка
            var yRoom1 = (float)Math.Round(mainRoomsCoords[pointsOfMainCorridors[i].Item1].y);
            var xRoom2 = (float)Math.Round(mainRoomsCoords[pointsOfMainCorridors[i].Item2].x);
            var yRoom2 = (float)Math.Round(mainRoomsCoords[pointsOfMainCorridors[i].Item2].y);
            var xScaleRoom1 = mainRoomsScales[pointsOfMainCorridors[i].Item1].Item1; // размеры комнат для этих центров
            var yScaleRoom1 = mainRoomsScales[pointsOfMainCorridors[i].Item1].Item2;
            var xScaleRoom2 = mainRoomsScales[pointsOfMainCorridors[i].Item2].Item1;
            var yScaleRoom2 = mainRoomsScales[pointsOfMainCorridors[i].Item2].Item2;

            var xMiddle = (float)Math.Round((xRoom1 + xRoom2) / 2); //середина между центрами точек
            var yMiddle = (float)Math.Round((yRoom1 + yRoom2) / 2);

            var xLeftBoundRoom1 = xRoom1 - xScaleRoom1 / 2 + 1; // в конце прибавляется/отнимается толщина стен
            var xRightBoundRoom1 = xRoom1 + xScaleRoom1 / 2 - 1;
            var yBotBoundRoom1 = yRoom1 - yScaleRoom1 / 2 + 1;
            var yTopBoundRoom1 = yRoom1 + yScaleRoom1 / 2 - 1;

            var xLeftBoundRoom2 = xRoom2 - xScaleRoom2 / 2 + 1;
            var xRightBoundRoom2 = xRoom2 + xScaleRoom2 / 2 - 1;
            var yBotBoundRoom2 = yRoom2 - yScaleRoom2 / 2 + 1;
            var yTopBoundRoom2 = yRoom2 + yScaleRoom2 / 2 - 1;
            
            GameObject instance = Instantiate(line, new Vector3(0f, 0f, 1f), Quaternion.identity) as GameObject;
            lineRend = instance.transform.GetComponent<LineRenderer>();
            instance.name = "Corridor " + pointsOfMainCorridors[i].Item1 + "," + pointsOfMainCorridors[i].Item2;
            lineRend.startColor = Color.blue;
            lineRend.endColor = Color.blue;

            if (xLeftBoundRoom1 < xMiddle && xMiddle < xRightBoundRoom1 && xLeftBoundRoom2 < xMiddle && xMiddle < xRightBoundRoom2)
            {
                lineRend.positionCount = 2;
                lineRend.SetPosition(0, new Vector3(xMiddle, yRoom1, 0));
                lineRend.SetPosition(1, new Vector3(xMiddle, yRoom2, 0));
                instance.transform.SetParent(corridorHolder);
            }
            else if (yBotBoundRoom1 < yMiddle && yMiddle < yTopBoundRoom1 && yBotBoundRoom2 < yMiddle && yMiddle < yTopBoundRoom2)
            {
                lineRend.positionCount = 2;
                lineRend.SetPosition(0, new Vector3(xRoom1, yMiddle, 0));
                lineRend.SetPosition(1, new Vector3(xRoom2, yMiddle, 0));
                instance.transform.SetParent(corridorHolder);
            }
            else
            {
                lineRend.positionCount = 3;
                lineRend.SetPosition(0, new Vector3(xRoom1, yRoom1, 0));
                lineRend.SetPosition(1, new Vector3(xRoom1, yRoom2, 0));
                lineRend.SetPosition(2, new Vector3(xRoom2, yRoom2, 0));
                Debug.Log(instance.name+": pos0 " + xRoom1 + " " + yRoom1 + ", pos1 " + xRoom1 + " " + yRoom2 + ", pos2 " + xRoom2 + " " + yRoom2);
                instance.transform.SetParent(corridorHolder);
            }
        }
        
        bool changeDirection = false;
        foreach (Transform corridor in corridorHolder.GetComponentsInChildren<Transform>())
        {
            if (corridor.name != "Corridors" && corridor.GetComponent<LineRenderer>().positionCount == 3)
            {
                Vector3[] oldCorridorPositions = new Vector3[3];
                corridor.GetComponent<LineRenderer>().GetPositions(oldCorridorPositions);
                double imprecision = 10;
                foreach (Transform otherCorridor in corridorHolder.GetComponentsInChildren<Transform>())
                {

                    if (otherCorridor.name != "Corridors" && otherCorridor.name != corridor.name)
                    {
                        Vector3[] newCorridorPositions = new Vector3[3];
                        otherCorridor.GetComponent<LineRenderer>().GetPositions(newCorridorPositions);
                        for (int j = 0; j < oldCorridorPositions.Length - 1; j++)
                        {
                            for (int k = 0; k < newCorridorPositions.Length - 1; k++)
                            {
                                if (oldCorridorPositions[j].x == oldCorridorPositions[j + 1].x && newCorridorPositions[k].x == newCorridorPositions[k + 1].x)
                                {
                                    if (Math.Abs(oldCorridorPositions[j].x - newCorridorPositions[k].x) <= imprecision && (oldCorridorPositions[j].y != newCorridorPositions[k].y || oldCorridorPositions[j+1].y != newCorridorPositions[k].y || oldCorridorPositions[j + 1].y != newCorridorPositions[k + 1].y || oldCorridorPositions[j].y != newCorridorPositions[k + 1].y))
                                    {
                                        changeDirection = true;
                                        goto UseChange;
                                    }
                                }
                                if (oldCorridorPositions[j].y == oldCorridorPositions[j + 1].y && newCorridorPositions[k].y == newCorridorPositions[k + 1].y)
                                {
                                    if (Math.Abs(oldCorridorPositions[j].y - newCorridorPositions[k].y) <= imprecision && (oldCorridorPositions[j].x != newCorridorPositions[k].x || oldCorridorPositions[j + 1].x != newCorridorPositions[k].x || oldCorridorPositions[j + 1].x != newCorridorPositions[k + 1].x || oldCorridorPositions[j].x != newCorridorPositions[k + 1].x))
                                    {
                                        changeDirection = true;
                                        goto UseChange;
                                    }
                                }
                            }
                        }
                    }
                }
            UseChange:
                if (changeDirection)
                {
                    var xRoom1 = corridor.GetComponent<LineRenderer>().GetPosition(0).x;
                    var xRoom2 = corridor.GetComponent<LineRenderer>().GetPosition(2).x;
                    var yRoom1 = corridor.GetComponent<LineRenderer>().GetPosition(0).y;
                    var yRoom2 = corridor.GetComponent<LineRenderer>().GetPosition(2).y;

                    corridor.GetComponent<LineRenderer>().SetPosition(0, new Vector3(xRoom2, yRoom2, 0));
                    corridor.GetComponent<LineRenderer>().SetPosition(1, new Vector3(xRoom2, yRoom1, 0));
                    corridor.GetComponent<LineRenderer>().SetPosition(2, new Vector3(xRoom1, yRoom1, 0));
                }

                
            }

            if (corridor.name != "Corridors")
            {
                //здесь не мейн комнаты, а все остальные
                for (int i = 0; i < numOfRooms; i++)
                {
                    bool checkSideRoom = false;
                    GameObject sideRoom = GameObject.Find("room" + i);
                    if (sideRoom)
                    {
                        var xRoom = sideRoom.transform.position.x; // координаты центров точек отрезка
                        var yRoom = sideRoom.transform.position.y;
                        var xScaleRoom = sideRoom.transform.localScale.x; // размеры комнат для этих центров
                        var yScaleRoom = sideRoom.transform.localScale.y;

                        var xLeftBoundRoom = xRoom - xScaleRoom / 2 + 1f;
                        var xRightBoundRoom = xRoom + xScaleRoom / 2 - 1f;
                        var yBotBoundRoom = yRoom - yScaleRoom / 2 + 1f;
                        var yTopBoundRoom = yRoom + yScaleRoom / 2 - 1f;

                        Vector3[] CorridorPositions = new Vector3[corridor.GetComponent<LineRenderer>().positionCount];
                        corridor.GetComponent<LineRenderer>().GetPositions(CorridorPositions);
                        for (int j = 0; j < CorridorPositions.Length - 1; j++)
                        {
                            if (CorridorPositions[j].x == CorridorPositions[j + 1].x)
                            {
                                var topYPoint = Math.Max(CorridorPositions[j].y, CorridorPositions[j + 1].y);
                                var botYPoint = Math.Min(CorridorPositions[j].y, CorridorPositions[j + 1].y);
                                if (xLeftBoundRoom < CorridorPositions[j].x && CorridorPositions[j].x < xRightBoundRoom)
                                {
                                    if (botYPoint < yBotBoundRoom && yBotBoundRoom < topYPoint  || botYPoint < yTopBoundRoom && yTopBoundRoom < topYPoint)
                                    {
                                        checkSideRoom = true;
                                        goto UseChange;
                                    }
                                }
                            }
                            if (CorridorPositions[j].y == CorridorPositions[j + 1].y)
                            {
                                var rightXPoint = Math.Max(CorridorPositions[j].x, CorridorPositions[j + 1].x);
                                var leftXPoint = Math.Min(CorridorPositions[j].x, CorridorPositions[j + 1].x);
                                if (yBotBoundRoom < CorridorPositions[j].y && CorridorPositions[j].y < yTopBoundRoom)
                                {
                                    if (leftXPoint < xLeftBoundRoom && xLeftBoundRoom < rightXPoint || leftXPoint < xRightBoundRoom && xRightBoundRoom < rightXPoint)
                                    {
                                        checkSideRoom = true;
                                        goto UseChange;
                                    }
                                }
                            }
                        }
                        UseChange:
                        if (checkSideRoom)
                        {
                            sideRoom.GetComponent<SpriteRenderer>().color = new Color(0, 100, 0, 1);
                            sideRoom.name = "SideRoom" + i;
                        }
                    }
                }
            }

           
        }

        for (int i = 0; i < numOfRooms; i++)
        {
            GameObject delRoom = GameObject.Find("room" + i);
            if (delRoom)
            {
                Destroy(delRoom);
            }

        }
        //Time.timeScale = 10.0f;
        yield return new WaitForSeconds(0.1f); // пауза для просчитывания положения комнат
        //Time.timeScale = 1.0f;

        foreach (Transform room in boardHolder.GetComponentsInChildren<Transform>())
        {
            if (room.name != "Board" && !room.name.StartsWith("neighbors"))
                for (int x = 0; x < room.localScale.x; x++)
                {
                    for (int y = 0; y < room.localScale.y; y++)
                    {
                        GameObject toInstantiate;
                        string nameofTile;
                        if (x == 0 || x == room.localScale.x-1 || y == 0 || y == room.localScale.y-1) 
                        { 
                            toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                            nameofTile = "WallTile from ";
                        }
                        else 
                        {
                            toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                            nameofTile = "FloorTile from ";
                        }
                    
                        float newX = (room.position.x - room.localScale.x / 2 + 0.5f) + 1 * x;
                        float newY = (room.position.y - room.localScale.y / 2 + 0.5f) + 1 * y;
                        GameObject instance = Instantiate(toInstantiate, new Vector3(newX, newY, 0f), Quaternion.identity) as GameObject;
                        instance.name = nameofTile + room.name;
                        instance.transform.SetParent(tilesHolder);
                    }
                }

        }

        foreach (Transform corridor in corridorHolder.GetComponentsInChildren<Transform>())
            if (corridor.name != "Corridors")
            {
                Vector3[] CorridorPositions = new Vector3[corridor.GetComponent<LineRenderer>().positionCount];
                corridor.GetComponent<LineRenderer>().GetPositions(CorridorPositions);
                for (int j = 0; j < CorridorPositions.Length - 1; j++)
                {
                    bool secondLine = false;
                    if (j == 1)
                        secondLine = true;
                    if (CorridorPositions[j].x == CorridorPositions[j + 1].x)
                    {
                        var x = CorridorPositions[j].x;
                        var topYPoint = Math.Max(CorridorPositions[j].y, CorridorPositions[j + 1].y);
                        var botYPoint = Math.Min(CorridorPositions[j].y, CorridorPositions[j + 1].y);
                        for (float y = botYPoint; y <= topYPoint; y++)
                        {
                            foreach (Transform tile in tilesHolder.GetComponentsInChildren<Transform>())
                            {
                                if (tile.name != "Tiles" && tile.position.x == x && tile.position.y == y)
                                {
                                    if (tile.name.StartsWith("WallTile"))
                                    {
                                        SetupTile(floorTiles, x, y, "FloorTile");
                                        bool voidOnLeft = true;
                                        bool voidOnRight = true;
                                        foreach (Transform neighborTile in tilesHolder.GetComponentsInChildren<Transform>())
                                            if (neighborTile.name != "Tiles" && neighborTile.position.y == y)
                                            {
                                                if (neighborTile.position.x == x + 1)
                                                {
                                                    voidOnRight = false;
                                                }
                                                else if (neighborTile.position.x == x - 1)
                                                {
                                                    voidOnLeft = false;
                                                }
                                            }
                                                
                                        if (voidOnLeft)
                                        {
                                            if (secondLine)
                                            {
                                                if (y == botYPoint)
                                                    SetupTile(outerWallTiles, x - 1, y - 1, "WallTile");
                                                else if (y == topYPoint)
                                                    SetupTile(outerWallTiles, x - 1, y + 1, "WallTile");
                                            }
                                            SetupTile(outerWallTiles, x - 1, y, "WallTile");
                                        }
                                        else if (voidOnRight)
                                        {
                                            if (secondLine)
                                            {
                                                if (y == botYPoint)
                                                    SetupTile(outerWallTiles, x + 1, y - 1, "WallTile");
                                                else if (y == topYPoint)
                                                    SetupTile(outerWallTiles, x + 1, y + 1, "WallTile");
                                            }
                                            SetupTile(outerWallTiles, x + 1, y, "WallTile");
                                        }
                                        Destroy(tile.gameObject);
                                        goto skip;
                                    }
                                    if (tile.name.StartsWith("FloorTile"))
                                    {
                                        if (secondLine && (y == botYPoint || y == topYPoint))
                                        {
                                            bool voidOnLeft = true;
                                            bool voidOnRight = true;
                                            foreach (Transform neighborTile in tilesHolder.GetComponentsInChildren<Transform>())
                                                if (neighborTile.name != "Tiles" && neighborTile.position.y == y)
                                                    if (neighborTile.position.x == x + 1)
                                                    {
                                                        voidOnRight = false;
                                                    }
                                                    else if (neighborTile.position.x == x - 1)
                                                    {
                                                        voidOnLeft = false;
                                                    }
                                            if (voidOnLeft)
                                            {
                                                if (y == botYPoint)
                                                    SetupTile(outerWallTiles, x - 1, y - 1, "WallTile");
                                                else
                                                    SetupTile(outerWallTiles, x - 1, y + 1, "WallTile");
                                                SetupTile(outerWallTiles, x - 1, y, "WallTile");
                                            }
                                            else if (voidOnRight)
                                            {
                                                if (y == botYPoint)
                                                    SetupTile(outerWallTiles, x + 1, y - 1, "WallTile");
                                                else
                                                    SetupTile(outerWallTiles, x + 1, y + 1, "WallTile");
                                                SetupTile(outerWallTiles, x + 1, y, "WallTile");
                                            }
                                        }
                                        goto skip;
                                    }
                                }
                            }
                            SetupTile(floorTiles, x, y, "FloorTile");
                            SetupTile(outerWallTiles, x - 1, y, "WallTile");
                            SetupTile(outerWallTiles, x + 1, y, "WallTile");
                        skip:
                            ;
                        }
                    }
                    if (CorridorPositions[j].y == CorridorPositions[j + 1].y)
                    {
                        var y = CorridorPositions[j].y;
                        var rightXPoint = Math.Max(CorridorPositions[j].x, CorridorPositions[j + 1].x);
                        var leftXPoint = Math.Min(CorridorPositions[j].x, CorridorPositions[j + 1].x);
                        for (float x = leftXPoint; x <= rightXPoint; x++)
                        {
                            foreach (Transform tile in tilesHolder.GetComponentsInChildren<Transform>())
                            {
                                if (tile.name != "Tiles" && tile.position.x == x && tile.position.y == y)
                                {
                                    if (tile.name.StartsWith("WallTile"))
                                    {
                                        SetupTile(floorTiles, x, y, "FloorTile");
                                        bool voidOnBot = true;
                                        bool voidOnTop = true;
                                        foreach (Transform neighborTile in tilesHolder.GetComponentsInChildren<Transform>())
                                            if (neighborTile.name != "Tiles" && neighborTile.position.x == x)
                                            {
                                                if (neighborTile.position.y == y + 1)
                                                {
                                                    voidOnTop = false;
                                                }
                                                else if (neighborTile.position.y == y - 1)
                                                {
                                                    voidOnBot = false;
                                                }
                                            }
                                        if (voidOnBot)
                                        {
                                            if (secondLine)
                                            {
                                                if (x == leftXPoint)
                                                    SetupTile(outerWallTiles, x - 1, y - 1, "WallTile");
                                                else if (x == rightXPoint)
                                                    SetupTile(outerWallTiles, x + 1, y - 1, "WallTile");
                                            }
                                            SetupTile(outerWallTiles, x, y - 1, "WallTile");
                                        }
                                        else if (voidOnTop)
                                        {
                                            if (secondLine)
                                            {
                                                if (x == leftXPoint)
                                                    SetupTile(outerWallTiles, x - 1, y + 1, "WallTile");
                                                else if (x == rightXPoint)
                                                    SetupTile(outerWallTiles, x + 1, y + 1, "WallTile");
                                            }
                                            SetupTile(outerWallTiles, x, y + 1, "WallTile");
                                        }
                                        Destroy(tile.gameObject);
                                        goto skip;
                                    }
                                    if (tile.name.StartsWith("FloorTile"))
                                    {
                                        if (secondLine && (x == rightXPoint || x == leftXPoint))
                                        {
                                            bool voidOnBot = true;
                                            bool voidOnTop = true;
                                            foreach (Transform neighborTile in tilesHolder.GetComponentsInChildren<Transform>())
                                                if (neighborTile.name != "Tiles" && neighborTile.position.x == x)
                                                {
                                                    if (neighborTile.position.y == y + 1)
                                                        voidOnTop = false;
                                                    else if (neighborTile.position.y == y - 1)
                                                        voidOnBot = false;
                                                }
                                            if (voidOnBot)
                                            {
                                                if (x == leftXPoint)
                                                    SetupTile(outerWallTiles, x - 1, y - 1, "WallTile");
                                                else
                                                    SetupTile(outerWallTiles, x + 1, y - 1, "WallTile");
                                                SetupTile(outerWallTiles, x, y - 1, "WallTile");
                                            }
                                            else if (voidOnTop)
                                            {
                                                if (x == leftXPoint)

                                                    SetupTile(outerWallTiles, x - 1, y + 1, "FloorTile");
                                                else
                                                    SetupTile(outerWallTiles, x + 1, y + 1, "FloorTile");
                                                SetupTile(outerWallTiles, x, y + 1, "FloorTile");
                                            }
                                        }
                                        goto skip;
                                    }
                                }
                            }
                            SetupTile(floorTiles, x, y, "FloorTile");
                            SetupTile(outerWallTiles, x, y - 1, "WallTile");
                            SetupTile(outerWallTiles, x, y + 1, "WallTile");
                        skip:
                            ;
                        }
                    }
                }
            }
       
        mainRoomsCoords.Clear();
        mainRoomsScales.Clear();
        pointsOfMainCorridors.Clear();
    }

    public void SetupTile(GameObject[] tiles, float x, float y, string name)
    {
        GameObject tile = tiles[Random.Range(0, tiles.Length)];
        GameObject instanceTile = Instantiate(tile, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
        instanceTile.name = name + room.name;
        instanceTile.transform.SetParent(tilesHolder);
    }

    public void SetupScene()
    {
        StartCoroutine(BoardSetup());
    }
}
