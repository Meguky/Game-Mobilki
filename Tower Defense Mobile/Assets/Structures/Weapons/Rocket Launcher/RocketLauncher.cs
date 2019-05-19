using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : Weapon {


    [Header("Required References")]
    [SerializeField] Transform cannonHead;
    [SerializeField] Transform barrelExit;
    [SerializeField] HomingMissle usedProjectile;


    [Header("Parameters")]
    [SerializeField] float rotationSpeed = 15.0f;
    [SerializeField] float attacksPerSecond = 3f;
    [SerializeField] float rocketDamage = 30f;
    float currCooldown = 0;

    new void Start() {

        attacksPerSecond = 1.0f / attacksPerSecond;
        availableEnemies = new List<Enemy>();
        weaponRange = GetComponent<CircleCollider2D>();
        structureName = "RocketLauncher";
    }

    void RotateCannonTowardsEnemy() {

        Vector2 direction = new Vector2(trackedEnemy.transform.position.x - transform.position.x, trackedEnemy.transform.position.y - transform.position.y);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        float rotationStrength = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
        Quaternion newRotation = Quaternion.Lerp(cannonHead.rotation, targetRotation, rotationStrength);

        cannonHead.rotation = newRotation;

    }

    void TryShoot() {

        currCooldown += Time.deltaTime;

        if (currCooldown >= attacksPerSecond) {

            HomingMissle newProjectile = Instantiate(usedProjectile, barrelExit.position, barrelExit.rotation);
            newProjectile.Initialise(trackedEnemy.transform, rocketDamage);
            currCooldown = 0;

        }
    }

    //trzeba to zrobić inaczej, ulepszenie jednej wyrzutni zwieksza damage rakiet z kazdej wyrzutni
    public override void Upgrade(int levels) {
        for(int i = 0; i < levels; i++){
            buildingCost += upgradeCost;
            upgradeCost *= 2f;
            rocketDamage *= 2f;
            structureLevel++;
        }
    }

    new void Update() {
        if (availableEnemies.Count != 0) {
            TrackEnemy();
            RotateCannonTowardsEnemy();
            TryShoot();
        }
    }
}
