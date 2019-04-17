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

        float[] min = { float.NegativeInfinity, float.PositiveInfinity };

        foreach (Enemy enemy in availableEnemies) {

            float[] currentEnemyDistance = enemy.DistanceToBase();

            if (currentEnemyDistance[0]>min[0] || (currentEnemyDistance[0] == min[0] && currentEnemyDistance[1]<min[1])) {

                trackedEnemy = enemy;
                min[0] = currentEnemyDistance[0];
                min[1] = currentEnemyDistance[1];

            }

        }
    }

    //Usuwanie obiektu nie wywołuje "OnColliderExit" po śmierci wrogów pozostają nulle.
    //Dobrze by było to ogarnąć, to tylko szybki fix.
    protected void ValidateEnemyList() {

        List<Enemy> deadReferences = new List<Enemy>();

        foreach (Enemy enemy in availableEnemies) {
            if (enemy == null) {
                deadReferences.Add(enemy);
            }
        }

        foreach (Enemy enemy in deadReferences) {
            availableEnemies.Remove(enemy);
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

    protected void OnTriggerEnter2D(Collider2D other) {
        if (other.tag.Equals("Enemy")) {
            availableEnemies.Add(other.GetComponent<Enemy>());
        }
    }

    protected void OnTriggerExit2D(Collider2D other) {
        if (other.tag.Equals("Enemy")) {
            availableEnemies.Remove(other.GetComponent<Enemy>());
        }
    }

    //tutaj trafić w przyszłości mogą status ailmenty, buffy, debuffy imające się wszystkich broni JamFor(float seconds), SlowFor(float seconds) BoostAttackSpeedFor(float seconds) etc

}
