using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TowerDefense {
    public class GameManager : MonoBehaviour {
        private CameraManager cameraManager;
        private WaitForSeconds startWaveTime;
        private WaitForSeconds endWaveTime;
        private Enemy enemyInstance;
        private GameObject[] remainingEnemiesGameObjects;
        public static GameManager instance;
        [Header("Player parameters")]
        public Structure playerBase;
        public float playerBaseHealth = 1000;
        public float money = 1000;
        public Text waveAnnouncer;
        [Header("Wave parameters")]
        public int startWave = 1;
        public float startDelay = 3f;
        public float endDelay = 3f;

        public float monsterHealthMultiplier = 3f;
        public float monsterDamageMultiplier = 5f;
        public float monsterDensityMultiplier = 1f;
        [Header("Enemies parameters")]
        public int enemiesInWave = 5;
        public float enemyDamage = 10;
        public float enemyHealth = 100;
        public float spawnIntervals = 0.5f;
        public Enemy[] enemiesTypes;
        public Transform spawnPoint;
        
        
        [HideInInspector]public int waveNumber = 1;
        [HideInInspector]public int enemiesCount = 0;
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
            if(startWave > 1){
                for(int i = 0 ; i < startWave;i++){
                    enemyHealth = enemyHealth + i * monsterHealthMultiplier;
                    enemyDamage = enemyDamage + i * monsterDamageMultiplier;
                    enemiesInWave = enemiesInWave + Mathf.RoundToInt(i * monsterDensityMultiplier / 40);
                }   
                waveNumber = startWave;
            }
        }

        public void setup(){
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

        private IEnumerator GameLoop(){
            yield return StartCoroutine(StartWave());

            yield return StartCoroutine(GenerateWave());

            yield return StartCoroutine(PlayWave());

            yield return StartCoroutine(EndWave());

            StartCoroutine(GameLoop());
        }
        
        private IEnumerator StartWave(){
            waveAnnouncer.text = "Wave " + waveNumber + " approaching!";
            yield return startWaveTime;
        }

        private IEnumerator GenerateWave(){
            for(int i = 0; i < enemiesInWave;i++){
                enemyInstance = Instantiate(enemiesTypes[0],spawnPoint.position,spawnPoint.rotation);
                enemyInstance.OnDeath.AddListener(OnEnemyDeath);
                enemyInstance.setup(enemyHealth, enemyDamage);
                enemiesCount++;
                waveAnnouncer.text = "Enemies left in wave: " + enemiesCount;
                yield return new WaitForSeconds(spawnIntervals);
                if(playerBase.getHealth() < 0){
                    break;
                }
            }
        }

        private IEnumerator PlayWave(){
            while(enemiesCount > 0 && playerBase.getHealth() > 0){
                waveAnnouncer.text = "Enemies left in wave: " + enemiesCount;
                yield return null;
            }
        }

        private IEnumerator EndWave(){
            if(playerBase.getHealth() <= 0){
                waveAnnouncer.text = "Wave failed, retring current wave!";
                remainingEnemiesGameObjects = GameObject.FindGameObjectsWithTag("Enemy");
                for(int i = 0; i < remainingEnemiesGameObjects.Length; i++){
                    Destroy(remainingEnemiesGameObjects[i]);
                }
                playerBase.gameObject.SetActive(true);
                playerBase.setup(playerBaseHealth);
                enemiesCount = 0;
            }else{
                waveNumber++;
                waveAnnouncer.text = "Wave beaten, next wave is approaching!";
                enemyHealth = enemyHealth + waveNumber * 10;
                enemyDamage = enemyDamage + waveNumber * 20;
                enemiesInWave = enemiesInWave + Mathf.RoundToInt(waveNumber / 5.0f);
            }
            yield return endWaveTime;
        }

        private void OnEnemyDeath(Enemy enemy){
            enemiesCount--;
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