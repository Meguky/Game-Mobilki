using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerDefense {
    public class GameManager : MonoBehaviour {

        public static GameManager instance;

        void Start() {

            if (instance == null) {
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else {
                Destroy(gameObject);
            }

        }

        private void Update() {
            ManageTouch();
        }

        //Elementy PC tylko na potrzeby testów. WYWALIĆ (lub zakomentaować) przed buildem.
        void ManageTouch() {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
                if (Input.touchCount > 0 && Input.touchCount < 2 && !EventSystem.current.IsPointerOverGameObject()) {
                    if (Input.GetTouch(0).phase == TouchPhase.Ended) {

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
            else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor) {
                if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) {

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