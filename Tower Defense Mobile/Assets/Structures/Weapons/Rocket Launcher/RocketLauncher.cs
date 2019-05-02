﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : Weapon {

    [SerializeField]
    Transform cannonHead, barrelExit;
    [SerializeField]
    float rotationSpeed = 15.0f;
    [SerializeField]
    float attacksPerSecond = 3f;
    float currCooldown = 0;

    [SerializeField]
    HomingMissle usedProjectile;

    Transform bulletSpawnpoint;

    new void Start() {

        attacksPerSecond = 1.0f / attacksPerSecond;
        bulletSpawnpoint = transform.Find("CannonHead/BulletSpawnpoint");
        availableEnemies = new List<Enemy>();
        weaponRange = GetComponent<CircleCollider2D>();

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

        if (currCooldown>= attacksPerSecond) {

            HomingMissle newProjectile = Instantiate(usedProjectile, bulletSpawnpoint.position, bulletSpawnpoint.rotation);
            newProjectile.InitialiseBullet(trackedEnemy.transform, this);
            currCooldown = 0;

        }
    }

    public Transform GetTargetUpdate() {
        return trackedEnemy.transform;
    }

    new void Update() {
        if (availableEnemies.Count != 0) {
            TrackEnemy();
            RotateCannonTowardsEnemy();
            TryShoot();
        }
    }
}