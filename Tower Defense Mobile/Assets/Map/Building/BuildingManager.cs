using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingManager : MonoBehaviour, IInteractable {

    public static BuildingManager instance;

    private Grid buildGrid;

    private Structure currentlySelectedStructure;
    private Structure[,] structures = new Structure[18, 28];

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

}
