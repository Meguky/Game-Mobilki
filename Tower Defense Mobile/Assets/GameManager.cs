using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EndlessBitDefense {
    public class GameManager : MonoBehaviour {

        public static GameManager instance;

        private CameraManager cameraManager;
        
        [Header("Save essentials")]
        private SaveManager saveManager;
        [HideInInspector] public bool saveLoaded = false;

        [Header("Required References")]
        [SerializeField] private Base playerBase;
        [SerializeField] private Text waveAnnouncer;
        [SerializeField] private UIManager uiManager;

        [Header("Player parameters")]
        [SerializeField] private float money;

        [Header("Wave parameters")]
        [SerializeField] private int startWave = 1;
        [SerializeField] private float startDelay = 3f;
        [SerializeField] private float endDelay = 3f;

        [HideInInspector] public int waveNumber = 1;
        [HideInInspector] public int enemiesCount = 0;

        [SerializeField] private float monsterHealthMultiplier = 3f;
        [SerializeField] private float monsterDamageMultiplier = 5f;
        [SerializeField] private float monsterRewardMultiplier = 1f;
        [SerializeField] private float monsterDensityMultiplier = 1f;

        private bool breakWave = false;
        private bool goBackInWaves = false;
        private WaitForSeconds endWaveTime;
        private Enemy enemyInstance;
        private GameObject[] remainingEnemiesGameObjects;

        [Header("Enemy parameters")]
        [SerializeField] private int enemiesInWave = 5;
        [SerializeField] private float spawnIntervals = 0.5f;
        [SerializeField] private Enemy[] enemyTypes;
        [SerializeField] private Transform spawnPoint;

        public float GetMoney() {
            return money;
        }

        void Start() {

            if (instance == null) {
                //DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else {
                Destroy(gameObject);
            }

            cameraManager = Camera.main.GetComponent<CameraManager>();
            saveManager = SaveManager.Instance;

            endWaveTime = new WaitForSeconds(endDelay);

            //Symulacja waveów
            if (saveManager.Load()) {

                saveLoaded = true;
                money = saveManager.state.money;
                startWave = saveManager.state.waveNumber;

                if (startWave > 1) {
                    for (int i = 0; i < startWave-1; i++) {
                        NextWave();
                    }
                }

            }

        }

        public void Setup() {
            StartCoroutine(GameLoop());
        }

        public void EarnMoney(float amount) {
            money += amount;
            uiManager.UpdateMoney();
        }

        public bool TryPay(float amount) {

            if (amount <= money) {
                money -= amount;
                uiManager.UpdateMoney();
                return true;
            }
            else {
                return false;
            }

        }

        private IEnumerator GameLoop() {

            yield return StartCoroutine(StartWave());

            yield return StartCoroutine(GenerateWave());

            yield return StartCoroutine(PlayWave());

            yield return StartCoroutine(EndWave());

            StartCoroutine(GameLoop());

        }

        private IEnumerator StartWave() {

            waveAnnouncer.text = "Wave " + waveNumber + " approaching!";

            float counter = 0.0f;

            while (counter<startDelay && !breakWave) {
                yield return new WaitForEndOfFrame();
                counter += Time.deltaTime;
            }

        }

        private IEnumerator GenerateWave() {

            enemiesCount += enemiesInWave;
            waveAnnouncer.text = "Enemies left in wave: " + enemiesCount;

            for (int i = 0; i < enemiesInWave; i++) {

                enemyInstance = Instantiate(enemyTypes[0], spawnPoint.position, spawnPoint.rotation);
                enemyInstance.OnDeath.AddListener(OnEnemyDeath);
                enemyInstance.ScaleParameters(monsterHealthMultiplier * waveNumber/10, monsterDamageMultiplier * waveNumber/10, monsterRewardMultiplier * waveNumber/10);

                yield return new WaitForSeconds(spawnIntervals);

                if (breakWave) {
                    break;
                }

            }
        }

        private IEnumerator PlayWave() {
            while (!breakWave) {
                yield return null;
            }
        }

        private IEnumerator EndWave() {

            if (goBackInWaves) {

                goBackInWaves = false;

                waveAnnouncer.text = "Reverting to previous stage!";

                CleanupStage();
                PreviousWave();

            }
            else if (playerBase.GetHealth() <= 0) {

                CleanupStage();

                waveAnnouncer.text = "Wave failed, retring current wave!";

                Debug.Log("Breakwave:" + breakWave);


            }
            else {

                waveAnnouncer.text = "Wave beaten, next wave is approaching!";

                NextWave();

            }

            yield return endWaveTime;

        }

        private void CleanupStage() {

            remainingEnemiesGameObjects = GameObject.FindGameObjectsWithTag("Enemy");

            for (int i = 0; i < remainingEnemiesGameObjects.Length; i++) {
                remainingEnemiesGameObjects[i].GetComponent<Enemy>().Die(false);
            }
            playerBase.ReplenishHealth();
            breakWave = false;

        }

        public void RevertWave() {

            if (waveNumber > 1) {
                breakWave = true;
                goBackInWaves = true;
            }
            else {
                uiManager.PrintToGameLog("You are already on stage 1!");
            }

        }

        private void NextWave() {

            waveNumber++;
            enemiesInWave += Mathf.RoundToInt(monsterDensityMultiplier * waveNumber / 5.0f);

        }

        private void PreviousWave() {

            enemiesInWave -= Mathf.RoundToInt(monsterDensityMultiplier * waveNumber / 5.0f);
            waveNumber--;

        }

        private void OnEnemyDeath(Enemy enemy) {

            enemiesCount--;

            if (enemiesCount==0) {
                breakWave = true;
            }

            waveAnnouncer.text = "Enemies left in wave: " + enemiesCount;

        }

        public void OnBaseDestroyed() {
            breakWave = true;
        }

        private void Update() {
            ManageTouch();
        }

        //Zmienne na rzecz obsługi dotyku
        Vector2 touchStartPosition, touchPreviousPosition, touchCurrentPosition;

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
                    if (Input.GetTouch(0).phase == TouchPhase.Ended && (touchStartPosition - touchCurrentPosition).magnitude < 5.0f) {

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

        void OnApplicationPause() {
            if (saveManager!=null) {
                saveManager.Save(this, MapManager.instance);
            }
        }

        void OnApplicationQuit() {
            saveManager.Save(this, MapManager.instance);
        }

    }
}