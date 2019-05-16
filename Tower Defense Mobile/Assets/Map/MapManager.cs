using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour, IInteractable {

    public static MapManager instance;

    public class MapTile {

        public enum TileType { Walkable, NonWalkable, NonFlyable }

        //core info
        public Vector2Int gridLocation;

        //building
        public bool buildable = true;
        public TileType tileType = TileType.Walkable;
        public Structure builtStructure = null;

        //pathfinding
        public MapTile parent = null;
        public int gCost;
        public int hCost;

        public int fCost {
            get { return gCost + hCost; }
        }

    }

    private Grid mapGrid;

    private MapTile[,] mapTiles = new MapTile[18, 28];

    private Structure currentlySelectedStructure;

    [Header("Pathfinding")]
    [SerializeField] Transform spawnerLocation;
    [SerializeField] Transform baseLocation;
    [SerializeField] Tilemap nonWalkableLayer;
    [SerializeField] Tilemap nonFlyableLayer;
    [SerializeField] Tilemap nonBuildableLayer;
    [HideInInspector] public UnityEvent OnMapChange = new UnityEvent();

    public LinkedList<Vector3> defaultPath = new LinkedList<Vector3>();

    // Start is called before the first frame update
    void Start() {

        //singleton
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
        }

        mapGrid = GetComponent<Grid>();

        InitialiseGridInfo();
        
        defaultPath = FindGroundPathToBaseFrom(spawnerLocation.position);

    }

    public void SetSelectedStuctureTo(Structure structure) {
        currentlySelectedStructure = structure;
    }

    public void SingleTap(Vector3 click) {

        Vector3Int cellPosition = mapGrid.WorldToCell(click);
        Vector3 cellCenterPosition = mapGrid.GetCellCenterWorld(cellPosition);

        if (mapTiles[cellPosition.x, cellPosition.y].buildable) {

            if (mapTiles[cellPosition.x, cellPosition.y].builtStructure == null) {

                if (currentlySelectedStructure != null) {

                    mapTiles[cellPosition.x, cellPosition.y].tileType = MapTile.TileType.NonWalkable;

                    LinkedList<Vector3> testPath = FindGroundPathToBaseFrom(spawnerLocation.position);

                    if (testPath != null) {
                        if (TowerDefense.GameManager.instance.TryPay(currentlySelectedStructure.GetBuildingCost())) {
                            defaultPath = testPath;
                            OnMapChange.Invoke();
                            mapTiles[cellPosition.x, cellPosition.y].builtStructure = Instantiate(currentlySelectedStructure, cellCenterPosition + (new Vector3(0, 0, 1)), transform.rotation);
                        }
                        else {
                            UIManager.instance.PrintToGameLog("Not enough funds!");
                        }
                    }
                    else {
                        mapTiles[cellPosition.x, cellPosition.y].tileType = MapTile.TileType.Walkable;
                        UIManager.instance.PrintToGameLog("Cannot block path to base!");
                    }


                }
                else {
                    UIManager.instance.PrintToGameLog("Select a structure first!");
                }

            }
            else {

                //Placeholder. Potem zrobić otwieranie menu ulepszania / niszczenia struktur
                UIManager.instance.PrintToGameLog("Can't build ontop of other structure!");
            }
        }
        else {
            UIManager.instance.PrintToGameLog("Can't build there!");
        }
    }

    private void InitialiseGridInfo() {

        for (int j = 0; j < 28; ++j) {
            for (int i = 0; i < 18; ++i) {

                mapTiles[i, j] = new MapTile {
                    gridLocation = new Vector2Int(i, j)
                };

                Vector3Int tilemapPosition = new Vector3Int(i, j, 0);

                if (nonBuildableLayer.GetTile(tilemapPosition) != null) {
                    mapTiles[i, j].buildable = false;
                }

                //domyślnie walkable
                if (nonFlyableLayer.GetTile(tilemapPosition) != null) {
                    mapTiles[i, j].tileType = MapTile.TileType.NonFlyable;
                }
                else if (nonWalkableLayer.GetTile(tilemapPosition) != null) {
                    mapTiles[i, j].tileType = MapTile.TileType.NonWalkable;
                }

            }
        }

    }

    public LinkedList<Vector3> FindGroundPathToBaseFrom(Vector3 startLoc) {

        Vector3Int startCellIndex = mapGrid.WorldToCell(startLoc);
        Vector3Int baseCellIndex = mapGrid.WorldToCell(baseLocation.position);

        MapTile startTile = mapTiles[startCellIndex.x, startCellIndex.y];
        MapTile targetTile = mapTiles[baseCellIndex.x, baseCellIndex.y];

        List<MapTile> openSet = new List<MapTile>();
        HashSet<MapTile> closedSet = new HashSet<MapTile>();

        openSet.Add(startTile);

        while (openSet.Count > 0) {

            MapTile currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; ++i) {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);


            if (currentNode.Equals(targetTile)) {

                return TranslatePath(startTile, targetTile);

            }

            foreach (MapTile neighbour in NodeNeighbours(currentNode)) {

                if (neighbour.tileType!=MapTile.TileType.Walkable || closedSet.Contains(neighbour)) {
                    continue;
                }

                //kontrolowanie ruchu po skosie
                Vector2Int movementDirection = neighbour.gridLocation - currentNode.gridLocation;

                if (movementDirection.magnitude>1.0f) {

                    int testedX, testedY;
                    
                    if (movementDirection.x > 0) {
                        testedX = currentNode.gridLocation.x + 1;
                    }
                    else {
                        testedX = currentNode.gridLocation.x - 1;
                    }

                    if (movementDirection.y > 0) {
                        testedY = currentNode.gridLocation.y + 1;
                    }
                    else {
                        testedY = currentNode.gridLocation.y - 1;
                    }

                    //blokowanie podróży do po skosie jeśli "zawadza to" o obiekty po drodze
                    if (mapTiles[currentNode.gridLocation.x, testedY].tileType != MapTile.TileType.Walkable || mapTiles[testedX, currentNode.gridLocation.y].tileType != MapTile.TileType.Walkable) {
                        continue;
                    }

                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                if (newMovementCostToNeighbour < neighbour.gCost || !(openSet.Contains(neighbour))) {

                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetTile);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour)) {
                        openSet.Add(neighbour);
                    }
                }

            }
        }

        return null;

    }

    List<MapTile> NodeNeighbours(MapTile node) {

        List<MapTile> neighbours = new List<MapTile>();

        for (int x = -1; x <= 1; ++x) {
            for (int y = -1; y <= 1; ++y) {

                if (x == 0 && y == 0) {
                    continue;
                }

                int checkX = node.gridLocation.x + x;
                int checkY = node.gridLocation.y + y;

                if (checkX >= 0 && checkX < mapTiles.GetLength(0) && checkY >= 0 && checkY < mapTiles.GetLength(1)) {
                    neighbours.Add(mapTiles[checkX, checkY]);
                }
            }
        }

        return neighbours;

    }

    int GetDistance(MapTile nodeA, MapTile nodeB) {

        int distX = Mathf.Abs(nodeA.gridLocation.x - nodeB.gridLocation.x);
        int distY = Mathf.Abs(nodeA.gridLocation.y - nodeB.gridLocation.y);

        if (distX > distY) {
            return 14 * distY + 10 * (distX - distY);
        }
        return 14 * distX + 10 * (distY - distX);
    }

    LinkedList<Vector3> TranslatePath(MapTile startNode, MapTile targetNode) {

        LinkedList<Vector3> localPath = new LinkedList<Vector3>();
        MapTile currentNode = targetNode;

        while (currentNode != startNode) {
            localPath.AddFirst(mapGrid.GetCellCenterWorld(new Vector3Int(currentNode.gridLocation.x, currentNode.gridLocation.y, 0)));
            currentNode = currentNode.parent;
        }

        return localPath;

    }

}
