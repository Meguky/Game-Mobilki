using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingManager : MonoBehaviour {

    private Vector3 clickPosition, fixedClickPosition;
    Grid buildGrid;

    // Start is called before the first frame update
    void Start() {
        buildGrid = GetComponent<Grid>();
    }

    // Obsługa wydarzenia dotykowego ekranu. Funkcje myszki na potrzeby playtestowe, przerobić na dotyk do builda.
    void Update() {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
            if (Input.touchCount > 0 && Input.touchCount < 2 && !EventSystem.current.IsPointerOverGameObject()) {
                if (Input.GetTouch(0).phase == TouchPhase.Began) {
                    ManageClick(Input.GetTouch(0).position);
                }
            }
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor) {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
                ManageClick(Input.mousePosition);
            }
        }

    }

    //TODO wywalić zarządzanie raycastami do osobnego managera
    private void ManageClick(Vector3 click) {

        Vector3 wp = Camera.main.ScreenToWorldPoint(click);
        Vector2 touchPos = new Vector2(wp.x, wp.y);
        Collider2D hit = Physics2D.OverlapPoint(touchPos);

        if (hit != null && hit == gameObject.GetComponent<Collider2D>()) {

            clickPosition = new Vector3(touchPos.x, touchPos.y, transform.position.z);

            Vector3Int cellPosition = buildGrid.WorldToCell(clickPosition);
            fixedClickPosition = buildGrid.GetCellCenterWorld(cellPosition);

        }
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
