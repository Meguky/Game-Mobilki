using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Structure {

    //logika poszukiwania celu na razie jest wspólna, chociaż to do dyskusji
    //Na razie bronie preferują wroga który jest najbliżej bazy

    protected List<Enemy> availableEnemies;
    protected Enemy trackedEnemy;
    protected CircleCollider2D weaponRange;

    protected void TrackEnemy() {

        float[] min = { float.PositiveInfinity, float.PositiveInfinity };

        foreach (Enemy enemy in availableEnemies) {

            float[] currentEnemyDistance = enemy.DistanceToBase();

            if (currentEnemyDistance[0]<min[0] || (currentEnemyDistance[0] == min[0] && currentEnemyDistance[1]<min[1])) {

                trackedEnemy = enemy;
                min[0] = currentEnemyDistance[0];
                min[1] = currentEnemyDistance[1];

            }

        }
    }

    protected void Start() {

        availableEnemies = new List<Enemy>();
        weaponRange = GetComponent<CircleCollider2D>();

    }

    protected void Update() {

        if (availableEnemies.Count != 0) {

            TrackEnemy();

        }
    }

    void RemoveFromTrackedEnemies(Enemy enemy) {
        availableEnemies.Remove(enemy);
    }

    protected void OnTriggerEnter2D(Collider2D other) {
        if (other.tag.Equals("Enemy")) {
            Enemy enemy = other.GetComponent<Enemy>();
            enemy.OnDeath.AddListener(RemoveFromTrackedEnemies);
            availableEnemies.Add(enemy);
        }
    }

    protected void OnTriggerExit2D(Collider2D other) {
        if (other.tag.Equals("Enemy")) {
            Enemy enemy = other.GetComponent<Enemy>();
            enemy.OnDeath.RemoveListener(RemoveFromTrackedEnemies);
            availableEnemies.Remove(enemy);
        }
    }

    //tutaj trafić w przyszłości mogą status ailmenty, buffy, debuffy imające się wszystkich broni JamFor(float seconds), SlowFor(float seconds) BoostAttackSpeedFor(float seconds) etc

}
