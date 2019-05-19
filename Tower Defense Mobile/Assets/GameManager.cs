using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TowerDefense {
    public class GameManager : MonoBehaviour {

        public static GameManager instance;
        private CameraManager cameraManager;
        private WaitForSeconds startWaveTime;
        private WaitForSeconds endWaveTime;
        private Enemy enemyInstance;
        private GameObject[] remainingEnemiesGameObjects;

        [Header("Save essentials")]
        [SerializeField] private SaveManager saveManager;
        [HideInInspector] public bool saveLoaded = false;


        [Header("Player parameters")]
        [SerializeField] private Structure playerBase;
        [SerializeField] private float playerBaseHealth = 100;
        public float money;
        [SerializeField] private Text waveAnnouncer;

        [Header("Wave parameters")]
        [SerializeField] private int startWave = 1;
        [SerializeField] private float startDelay = 3f;
        [SerializeField] private float endDelay = 3f;

        [SerializeField] private float monsterHealthMultiplier = 3f;
        [SerializeField] private float monsterDamageMultiplier = 5f;
        [SerializeField] private float monsterDensityMultiplier = 1f;
        [SerializeField] private float monsterRewardMultiplier = 1f;

        [Header("Enemies parameters")]
        [SerializeField] private int enemiesInWave = 5;
        [SerializeField] private float enemyDamage = 10;
        [SerializeField] private float enemyHealth = 100;
        [SerializeField] private float enemyReward = 25;
        [SerializeField] private float spawnIntervals = 0.5f;
        [SerializeField] private Enemy[] enemyTypes;
        [SerializeField] private Transform spawnPoint;

        [HideInInspector] public int waveNumber = 1;
        [HideInInspector] public int enemiesCount = 0;

        void Start() {

            if (instance == null) {
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else {
                Destroy(gameObject);
            }

            cameraManager = Camera.main.GetComponent<CameraManager>();

            startWaveTime = new WaitForSeconds(startDelay);
            endWaveTime = new WaitForSeconds(endDelay);
            playerBase.initializeValues(playerBaseHealth, 1);


            //Symulacja waveów

            if (saveManager.Load()) {
                saveLoaded = true;
                startWave = saveManager.state.waveNumber;
                money = saveManager.state.money;
            }

            if (startWave > 1) {
                for (int i = 0; i < startWave; i++) {

                    enemiesInWave += Mathf.RoundToInt(i * monsterDensityMultiplier / 40);
                    enemyHealth += i * monsterHealthMultiplier;
                    enemyDamage += i * monsterDamageMultiplier;
                    enemyReward += i * monsterRewardMultiplier;

                }
                waveNumber = startWave;
            }
        }

        public void Setup() {
            StartCoroutine(GameLoop());
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

        private IEnumerator GameLoop() {

            yield return StartCoroutine(StartWave());

            yield return StartCoroutine(GenerateWave());

            yield return StartCoroutine(PlayWave());

            yield return StartCoroutine(EndWave());

            StartCoroutine(GameLoop());

        }

        private IEnumerator StartWave() {
            waveAnnouncer.text = "Wave " + waveNumber + " approaching!";
            yield return startWaveTime;
        }

        private IEnumerator GenerateWave() {

            enemiesCount += enemiesInWave;
            waveAnnouncer.text = "Enemies left in wave: " + enemiesCount;

            for (int i = 0; i < enemiesInWave; i++) {

                enemyInstance = Instantiate(enemyTypes[0], spawnPoint.position, spawnPoint.rotation);
                enemyInstance.OnDeath.AddListener(OnEnemyDeath);
                enemyInstance.InitialiseWithParameters(enemyHealth, enemyDamage, enemyReward);

                yield return new WaitForSeconds(spawnIntervals);

                if (playerBase.GetHealth() < 0) {
                    break;
                }
            }
        }

        private IEnumerator PlayWave() {
            while (enemiesCount > 0 && playerBase.GetHealth() > 0) {
                waveAnnouncer.text = "Enemies left in wave: " + enemiesCount;
                yield return null;
            }
        }

        private IEnumerator EndWave() {

            if (playerBase.GetHealth() <= 0) {

                waveAnnouncer.text = "Wave failed, retring current wave!";
                remainingEnemiesGameObjects = GameObject.FindGameObjectsWithTag("Enemy");

                for (int i = 0; i < remainingEnemiesGameObjects.Length; i++) {
                    Destroy(remainingEnemiesGameObjects[i]);
                }

                playerBase.gameObject.SetActive(true);
                playerBase.replenishHealth();
                enemiesCount = 0;

            }
            else {

                waveNumber++;
                waveAnnouncer.text = "Wave beaten, next wave is approaching!";
                enemyHealth += waveNumber * monsterHealthMultiplier;
                enemyDamage += waveNumber * monsterDamageMultiplier;
                enemiesInWave += Mathf.RoundToInt(monsterDensityMultiplier * waveNumber / 5.0f);
                enemyReward += waveNumber * monsterRewardMultiplier;

            }
            yield return endWaveTime;

        }

        private void OnEnemyDeath(Enemy enemy) {
            enemiesCount--;
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

        void OnApplicationQuit() {
            saveManager.state.money = money;
            saveManager.state.waveNumber = waveNumber;
            saveManager.Save();
        }

    }
}