using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerDefense {
    public class GameManager : MonoBehaviour {

        public static GameManager instance;

        private CameraManager cameraManager;

        public float money = 1000;

        void Start() {

            if (instance == null) {
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else {
                Destroy(gameObject);
            }

            cameraManager = Camera.main.GetComponent<CameraManager>();

        }

        public void EarnMoney(float amount) {
            money += amount;
            UIManager.instance.UpdateMoney();
        }

        public bool TryPay(float amount) {
            if (amount <= money) {
                money -= amount;
                UIManager.instance.UpdateMoney();
                return true;
            }
            else {
                return false;
            }
        }

        private void Update() {
            ManageTouch();
        }

        //Zmienne na rzecz obsługi dotyku
        Vector2 touchStartPosition, touchPreviousPosition ,touchCurrentPosition;

        private bool IsPointerOverUIObject() {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        void ManageTouch() {

            //Test platformy też żeby kodu ciągle niezmieniać. Można po developmencie wywalić.
            if (Application.platform == RuntimePlatform.Android) {
                if (Input.touchCount > 0 && Input.touchCount < 2 && !IsPointerOverUIObject()) {

                    if (Input.GetTouch(0).phase == TouchPhase.Began) {
                        touchCurrentPosition = touchPreviousPosition = touchStartPosition = Input.GetTouch(0).position;
                    }
                    if (Input.GetTouch(0).phase == TouchPhase.Moved) {
                        touchPreviousPosition = touchCurrentPosition;
                        touchCurrentPosition = Input.GetTouch(0).position;
                        cameraManager.MoveCameraBy(touchPreviousPosition - touchCurrentPosition);
                    }
                    if (Input.GetTouch(0).phase == TouchPhase.Ended && (touchStartPosition-touchCurrentPosition).magnitude<5.0f) {

                        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                        Vector2 touchPos = new Vector2(worldPos.x, worldPos.y);
                        Collider2D hit = Physics2D.OverlapPoint(touchPos);

                        MonoBehaviour[] scripts = hit.gameObject.GetComponents<MonoBehaviour>();
                        foreach (MonoBehaviour script in scripts) {
                            if (script is IInteractable) {
                                ((IInteractable)script).SingleTap(worldPos);
                            }
                        }
                    }

                }
            }
            //Wersja stosująca myszkę - do debugu
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) {
                if (!EventSystem.current.IsPointerOverGameObject()) {

                    if (Input.GetMouseButtonDown(0)) {
                        touchCurrentPosition = touchPreviousPosition = touchStartPosition = Input.mousePosition;
                    }
                    if (Input.GetMouseButton(0)) {
                        touchPreviousPosition = touchCurrentPosition;
                        touchCurrentPosition = Input.mousePosition;
                        cameraManager.MoveCameraBy(touchPreviousPosition - touchCurrentPosition);
                    }
                    if (Input.GetMouseButtonUp(0) && (touchStartPosition - touchCurrentPosition).magnitude < 5.0f) {

                        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        Vector2 touchPos = new Vector2(worldPos.x, worldPos.y);
                        Collider2D hit = Physics2D.OverlapPoint(touchPos);

                        MonoBehaviour[] scripts = hit.gameObject.GetComponents<MonoBehaviour>();
                        foreach (MonoBehaviour script in scripts) {
                            if (script is IInteractable) {
                                ((IInteractable)script).SingleTap(worldPos);
                            }
                        }
                    }
                }
            }

        }

    }
}