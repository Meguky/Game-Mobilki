using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MapManager : MonoBehaviour, IInteractable {

    public static MapManager instance;

    private Grid mapGrid;

    private Structure currentlySelectedStructure;
    private Structure[,] structures = new Structure[18, 28];

    [Header("Pathfinding")]
    [SerializeField] Transform spawnerLocation;
    [SerializeField] Transform baseLocation;

    [HideInInspector] public UnityEvent OnMapChange;

    public GridNode[,] gridNodes = new GridNode[18, 28];

    public LinkedList<Vector3> defaultPath = new LinkedList<Vector3>();

    // Start is called before the first frame update
    void Start() {

        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
        }

        mapGrid = GetComponent<Grid>();

        OnMapChange = new UnityEvent();

        //Inicjalizacja grida
        for (int i=0; i<18; ++i) {
            for (int j = 0; j < 28; ++j) {
                gridNodes[i, j] = new GridNode {
                    gridLocation = new Vector2Int(i, j),
                    isWalkable = true,
                    parent = null
                };
            }
        }

        defaultPath = FindPathToBaseFrom(spawnerLocation.position);

    }

    public void SingleTap(Vector3 click) {

        Vector3Int cellPosition = mapGrid.WorldToCell(click);
        Vector3 cellCenterPosition = mapGrid.GetCellCenterWorld(cellPosition);

        if (structures[cellPosition.x, cellPosition.y] == null) {

            if (currentlySelectedStructure != null) {

                gridNodes[cellPosition.x, cellPosition.y].isWalkable = false;

                LinkedList<Vector3> testPath = FindPathToBaseFrom(spawnerLocation.position);

                if (testPath != null) {
                    if (TowerDefense.GameManager.instance.TryPay(currentlySelectedStructure.GetBuildingCost())) {
                        defaultPath = testPath;
                        OnMapChange.Invoke();
                        structures[cellPosition.x, cellPosition.y] = Instantiate(currentlySelectedStructure, cellCenterPosition + (new Vector3(0, 0, 1)), transform.rotation);
                    }
                    else {
                        UIManager.instance.PrintToGameLog("Not enough funds!");
                    }
                }
                else {
                    gridNodes[cellPosition.x, cellPosition.y].isWalkable = true;
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

    public void SetSelectedStuctureTo(Structure structure) {
        currentlySelectedStructure = structure;
    }

    public class GridNode {

        public bool isWalkable;
        public Vector2Int gridLocation;
        public GridNode parent;

        public int gCost;
        public int hCost;

        public int fCost {
            get { return gCost + hCost; }
        }

    }

    //private void updateDefaultPath() {
    //    defaultPath = FindPathToBaseFrom(spawnerLocation.position);
    //}

    public LinkedList<Vector3> FindPathToBaseFrom(Vector3 startLoc) {

        Vector3Int startCellIndex = mapGrid.WorldToCell(startLoc);
        Vector3Int baseCellIndex = mapGrid.WorldToCell(baseLocation.position);

        GridNode startNode = gridNodes[startCellIndex.x, startCellIndex.y];
        GridNode baseNode = gridNodes[baseCellIndex.x, baseCellIndex.y];

        List<GridNode> openSet = new List<GridNode>();
        HashSet<GridNode> closedSet = new HashSet<GridNode>();

        openSet.Add(startNode);

        while (openSet.Count > 0) {

            GridNode currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; ++i) {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);


            if (currentNode.Equals(baseNode)) {

                return TranslatePath(startNode, baseNode);

            }

            foreach (GridNode neighbour in NodeNeighbours(currentNode)) {

                if (!neighbour.isWalkable || closedSet.Contains(neighbour)) {
                    continue;
                }

                //blokowanie ruchu po skosie przy zabudowanych działkach
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
                    if (!gridNodes[currentNode.gridLocation.x, testedY].isWalkable || !gridNodes[testedX, currentNode.gridLocation.y].isWalkable) {
                        continue;
                    }

                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                if (newMovementCostToNeighbour < neighbour.gCost || !(openSet.Contains(neighbour))) {

                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, baseNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour)) {
                        openSet.Add(neighbour);
                    }
                }

            }
        }

        return null;

    }

    List<GridNode> NodeNeighbours(GridNode node) {

        List<GridNode> neighbours = new List<GridNode>();

        for (int x = -1; x <= 1; ++x) {
            for (int y = -1; y <= 1; ++y) {
                if (x == 0 && y == 0) {
                    continue;
                }

                int checkX = node.gridLocation.x + x;
                int checkY = node.gridLocation.y + y;

                if (checkX >= 0 && checkX < gridNodes.GetLength(0) && checkY >= 0 && checkY < gridNodes.GetLength(1)) {
                    neighbours.Add(gridNodes[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    int GetDistance(GridNode nodeA, GridNode nodeB) {

        int distX = Mathf.Abs(nodeA.gridLocation.x - nodeB.gridLocation.x);
        int distY = Mathf.Abs(nodeA.gridLocation.y - nodeB.gridLocation.y);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }

    LinkedList<Vector3> TranslatePath(GridNode startNode, GridNode targetNode) {

        LinkedList<Vector3> localPath = new LinkedList<Vector3>();
        GridNode currentNode = targetNode;

        while (currentNode != startNode) {
            localPath.AddFirst(mapGrid.GetCellCenterWorld(new Vector3Int(currentNode.gridLocation.x, currentNode.gridLocation.y, 0)));
            currentNode = currentNode.parent;
        }

        return localPath;

    }

}
