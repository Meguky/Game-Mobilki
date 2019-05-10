using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyDetectionHandler : MonoBehaviour{

    [SerializeField] Enemy.EnemyEvent OnEnemyEnteredRadius;
    [SerializeField] Enemy.EnemyEvent OnEnemyExitedRadius;

    protected void OnTriggerEnter2D(Collider2D other) {
        if (other.tag.Equals("Enemy")) {
            OnEnemyEnteredRadius.Invoke(other.GetComponent<Enemy>());
        }
    }

    protected void OnTriggerExit2D(Collider2D other) {
        if (other.tag.Equals("Enemy")) {
            OnEnemyExitedRadius.Invoke(other.GetComponent<Enemy>());
        }
    }

}
