using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawnerController : MonoBehaviour {

    [SerializeField]
    EnemyController spawnedEnemy;

    [SerializeField]
    int numberOfEnemies;

    [SerializeField]
    float spawnIntervals;

    IEnumerator SpawningCoroutine() {
        for (int i=0; i<numberOfEnemies; i++) {
            yield return new WaitForSeconds(spawnIntervals);
            Instantiate(spawnedEnemy);
        }
        yield return new WaitForSeconds(.1f);
    }

    // Start is called before the first frame update
    void Start() {
        StartCoroutine(SpawningCoroutine());
    }

    // Update is called once per frame
    void Update() {

    }
}
