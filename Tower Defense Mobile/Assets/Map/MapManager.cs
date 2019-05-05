using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapManager : MonoBehaviour, IInteractable {

    public static MapManager instance;

    private Grid buildGrid;

    private Structure currentlySelectedStructure;
    private Structure[,] structures = new Structure[18, 28];

    public class GridNode
    {
        public bool isWalkable;
        public Vector2Int gridLocation;
        public GridNode parent;

        public int gCost;
        public int hCost;

        public GridNode(Vector2Int location, bool walkable)
        {
            isWalkable = walkable;
            gridLocation = location;
        }

        public void SetGCost(int _gCost)
        {
            gCost = _gCost;
        }

        public void SetHCost(int _hCost)
        {
            hCost = _hCost;
        }

        public int fCost
        {
            get { return gCost + hCost; }
        }
    }
    public GridNode[,] gridNodes = new GridNode[18, 28];
    //public List<GridNode> defaultPath;
    public LinkedList<GridNode> defaultPath;

    // Start is called before the first frame update
    void Start() {

        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
        }

        buildGrid = GetComponent<Grid>();
    }

    public void SingleTap(Vector3 click) {

        Vector3Int cellPosition = buildGrid.WorldToCell(click);
        Vector3 cellCenterPosition = buildGrid.GetCellCenterWorld(cellPosition);

        if (structures[cellPosition.x, cellPosition.y] == null) {

            if (currentlySelectedStructure != null) {

                structures[cellPosition.x, cellPosition.y] = Instantiate(currentlySelectedStructure, cellCenterPosition, transform.rotation);

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

    List<GridNode> NodeNeighbours (GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();

        for (int x = -1 ; x<=1 ; ++x){
            for (int y = -1 ; y<=1 ; ++y)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridLocation.x + x;
                int checkY = node.gridLocation.y + y;

                if (checkX >= 0 && checkX < gridNodes.GetLength(0) && checkY >= 0 && checkY < gridNodes.GetLength(1))
                    neighbours.Add(gridNodes[checkX, checkY]);
            }
        }
        return neighbours;
    }

    public void FindPathToBase(Vector3Int startLoc)
    {
        GridNode baseNode = gridNodes[9, 0];
        GridNode startNode = gridNodes[startLoc.x, startLoc.y];

        List<GridNode> openSet = new List<GridNode>();
        HashSet<GridNode> closedSet = new HashSet<GridNode>();

        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            GridNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; ++i)
            {
                if(openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode.Equals(baseNode))
            {
                RetracePath(startNode, baseNode);
                return;
            }

            foreach (GridNode neighbour in NodeNeighbours(currentNode))
            {
                if (!(neighbour.isWalkable) || closedSet.Contains(neighbour))
                    continue;

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !(openSet.Contains(neighbour)))
                {
                    neighbour.SetGCost(newMovementCostToNeighbour);
                    neighbour.SetHCost(GetDistance(neighbour, baseNode));
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }

            }
        }
    }

    void RetracePath(GridNode startNode, GridNode targetNode)
    {
        defaultPath = new LinkedList<GridNode>();
        GridNode currentNode = targetNode;

        while(currentNode != startNode)
        {
            defaultPath.AddFirst(currentNode);
            currentNode = currentNode.parent;
        }
        //defaultPath.Reverse();
    }

    private int GetDistance (GridNode nodeA, GridNode nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridLocation.x - nodeB.gridLocation.x);
        int distY = Mathf.Abs(nodeA.gridLocation.y - nodeB.gridLocation.y);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }

}
