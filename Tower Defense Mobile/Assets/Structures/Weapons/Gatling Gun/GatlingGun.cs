using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatlingGun : Weapon {

    [Header("Required References")]
    [SerializeField] Transform cannonHead;
    [SerializeField] ParticleSystem gunfireEffect;
    [SerializeField] ParticleSystem gunshotEffect;

    [Header("Parameters")]
    [SerializeField] float rotationSpeed = 15.0f;
    [SerializeField] float attacksPerSecond = 3f;
    private float attackingFrequency;
    [SerializeField] float attackDamage = 5f;
    

    float currCooldown = 0;

    new void Start() {

        attackingFrequency = 1.0f / attacksPerSecond;
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

        if (currCooldown >= attackingFrequency) {

            gunfireEffect.Play();
            Instantiate(gunshotEffect, trackedEnemy.transform.position, trackedEnemy.transform.rotation);
            trackedEnemy.TakeDamage(attackDamage);
            currCooldown = 0;

        }
    }

    //test
    public override void Upgrade() {

        buildingCost += upgradeCost;
        upgradeCost *= 2f;

        attackDamage *= 1.1f;
        attacksPerSecond *= 1.5f;
        attackingFrequency = 1.0f / attacksPerSecond;

    }

    new void Update() {
        if (availableEnemies.Count != 0) {
            TrackEnemy();
            RotateCannonTowardsEnemy();
            TryShoot();
        }
    }
}
