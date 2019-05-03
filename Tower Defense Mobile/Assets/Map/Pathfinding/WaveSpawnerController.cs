using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveSpawnerController : MonoBehaviour {

    [Header("Required References")]
    [SerializeField] Text announcerTextfield;
    [SerializeField] Enemy spawnedEnemy;

    [Header("Required Parameters")]
    [SerializeField] float numberOfWaves;
    [SerializeField] int enemiesPerWave;
    [SerializeField] float spawnIntervals = 0.5f;
    [SerializeField] float waveIntervals = 2.0f;

    float waveCountdown;
    bool currentWaveOver;

    IEnumerator SpawnNextWave() {

        for (int j = 0; j < enemiesPerWave; j++) {

            Instantiate(spawnedEnemy, transform.position, transform.rotation, transform);
            yield return new WaitForSeconds(spawnIntervals);

        }

        currentWaveOver = true;

    }

    // Start is called before the first frame update
    void Start() {
        waveCountdown = waveIntervals;
        currentWaveOver = true;
    }

    // Update is called once per frame
    void Update() {

        if (currentWaveOver) {

            if (waveCountdown <= 0f) {
                StartCoroutine(SpawnNextWave());
                announcerTextfield.text = "";
                currentWaveOver = false;
                waveCountdown = waveIntervals;
            }
            else {
                waveCountdown -= Time.deltaTime;
                announcerTextfield.text = "Next wave approaches in " + (Mathf.Floor(waveCountdown) + 1) + "!";
            }
        }
    }
}
