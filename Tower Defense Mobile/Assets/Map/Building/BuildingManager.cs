using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingManager : MonoBehaviour, IInteractable {

    private Vector3 clickPosition, fixedClickPosition;
    Grid buildGrid;

    // Start is called before the first frame update
    void Start() {
        buildGrid = GetComponent<Grid>();
    }

    public void SingleTap(Vector3 click) {

        clickPosition = new Vector3(click.x, click.y, transform.position.z);

        Vector3Int cellPosition = buildGrid.WorldToCell(clickPosition);
        Debug.Log(cellPosition);
        fixedClickPosition = buildGrid.GetCellCenterWorld(cellPosition);

    }

    //DEBUG
    private void OnDrawGizmos() {
        //Rysowanie miejsca kliknięcia
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(clickPosition, 0.25f);
        //Rysowanie miejsca dopasowanego do siatki
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(fixedClickPosition, 0.25f);
    }

}
