using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour, IInteractable {

    public static MapManager instance;

    private TowerDefense.GameManager gameManager;

    private Grid mapGrid;
    private MapTile[,] mapTiles = new MapTile[18, 28];
    private Structure currentlySelectedStructure;
    private MapTile highlightedTile;
    [SerializeField] private Transform structureContextMenu;

    [Header("Save essentials")]
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private Structure[] structures;

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
        gameManager = TowerDefense.GameManager.instance;

        InitialiseGridInfo();

        if (gameManager.saveLoaded) {
            ReconstructMap();
        }

        defaultPath = FindGroundPathToBaseFrom(spawnerLocation.position);

    }

    public void SingleTap(Vector3 click) {

        Vector3Int cellPosition = mapGrid.WorldToCell(click);
        Vector3 cellCenterPosition = mapGrid.GetCellCenterWorld(cellPosition);

        if (highlightedTile != null) {
            UnhighlightTile();
        }
        else if (mapTiles[cellPosition.x, cellPosition.y].buildable) {

            if (mapTiles[cellPosition.x, cellPosition.y].builtStructure == null) {

                if (currentlySelectedStructure != null) {

                    mapTiles[cellPosition.x, cellPosition.y].tileType = MapTile.TileType.NonWalkable;

                    LinkedList<Vector3> testPath = FindGroundPathToBaseFrom(spawnerLocation.position);

                    if (testPath != null) {

                        if (gameManager.TryPay(currentlySelectedStructure.GetBuildingCost())) {

                            defaultPath = testPath;
                            OnMapChange.Invoke();
                            mapTiles[cellPosition.x, cellPosition.y].builtStructure = Instantiate(currentlySelectedStructure, cellCenterPosition + new Vector3(0,0,1), transform.rotation);

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

            }
            else {
                HighlightTile(mapTiles[cellPosition.x, cellPosition.y]);
            }
        }
        else {
            UIManager.instance.PrintToGameLog("Can't build there!");
        }
    }

    public void SetSelectedStuctureTo(Structure structure) {
        currentlySelectedStructure = structure;
        UnhighlightTile();
    }

    public void HighlightTile(MapTile tile) {

        if (highlightedTile == tile) {
            UnhighlightTile();
            return;
        }

        highlightedTile = tile;

        Vector3 offset = new Vector3(0, -1.1f, 0);
        structureContextMenu.position = tile.builtStructure.transform.position + offset;

        structureContextMenu.gameObject.SetActive(true);
    }

    public void UnhighlightTile() {

        highlightedTile = null;

        structureContextMenu.gameObject.SetActive(false);

    }

    public void UpgradeHighlightedStructure() {

        if (gameManager.TryPay(highlightedTile.builtStructure.GetUpgradeCost())) {
            highlightedTile.builtStructure.Upgrade();
        }
        else {
            UIManager.instance.PrintToGameLog("Not enough funds!");
        }

    }

    public void SellHighlightedStructure() {

        highlightedTile.builtStructure.Sell();
        highlightedTile.builtStructure = null;
        UnhighlightTile();

    }

    private void InitialiseGridInfo() {
        Debug.Log("Initializing map");
        for (int j = 0; j < 28; ++j) {
            for (int i = 0; i < 18; ++i) {

                mapTiles[i, j] = new MapTile {
                    gridLocation = new Vector2Int(i, j),
                    builtStructure = null
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

    public void ReconstructMap() {

        for (int j = 0; j < 28; ++j) {
            for (int i = 0; i < 18; ++i) {

                if (saveManager.state.tiles[i + j * 18].name != "") {

                    string structureName = saveManager.state.tiles[i + j * 18].name;
                    Structure loadedStructure;
                    Debug.Log(structureName + " on " + i + ", " + j);

                    switch (structureName) {
                        case "Wall":
                            loadedStructure = structures[0];
                            break;
                        case "RocketLauncher":
                            loadedStructure = structures[1];
                            break;
                        case "GatlingGun":
                            loadedStructure = structures[2];
                            break;
                        default:
                            continue;
                    }

                    mapTiles[i, j].builtStructure = Instantiate(loadedStructure, mapGrid.GetCellCenterWorld(new Vector3Int(i, j, 0)) + new Vector3(0,0,1), transform.rotation);
                    mapTiles[i, j].builtStructure.Upgrade(saveManager.state.tiles[i + j * 18].level);
                    mapTiles[i, j].tileType = MapTile.TileType.NonWalkable;

                }

            }
        }

        defaultPath = FindGroundPathToBaseFrom(spawnerLocation.position);

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

                if (neighbour.tileType != MapTile.TileType.Walkable || closedSet.Contains(neighbour)) {
                    continue;
                }

                //kontrolowanie ruchu po skosie
                Vector2Int movementDirection = neighbour.gridLocation - currentNode.gridLocation;

                if (movementDirection.magnitude > 1.0f) {

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
    /// <summary>
    /// Callback sent to all game objects before the application is quit.
    /// </summary>
    void OnApplicationQuit() {
        saveManager.state.setMapTiles(mapTiles);
        saveManager.Save();
    }
}
