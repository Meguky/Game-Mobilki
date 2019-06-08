using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissle : MonoBehaviour {

    [Header("Required References")]
    [SerializeField] CircleCollider2D scanningRadius;
    [SerializeField] ParticleSystem explosionEffect;

    [Header("Parameters")]
    [SerializeField] float rotationSpeed = 0.5f;
    [SerializeField] float movementSpeed = 10;
    [SerializeField] float explosionRadius;
    private float explosionDamage;

    Transform trackedTarget;
    List<Enemy> availableEnemies = new List<Enemy>();

    public void Initialise(Transform initialTarget, float damage) {
        explosionDamage = damage;
        trackedTarget = initialTarget;
        EnemyEnteredRange(initialTarget.GetComponent<Enemy>());
    }

    // Update is called once per frame
    void Update() {

        TrackEnemy();

        if (trackedTarget != null) {

            RotateTowardsEnemy();

            Vector3 distanceFromTarget = trackedTarget.position - transform.position;
            Vector3 expectedMovement = distanceFromTarget.normalized * movementSpeed * Time.deltaTime;

            //check if we wont overshoot the enemy
            if (expectedMovement.magnitude > distanceFromTarget.magnitude) {
                transform.Translate(distanceFromTarget, Space.World);
            }
            else {
                transform.Translate(expectedMovement, Space.World);
            }
            if ((transform.position - trackedTarget.transform.position).magnitude < 0.1f) {
                Explode();
            }

        }
        else {
            Explode();
        }

    }

    protected void TrackEnemy() {

        if (availableEnemies.Count>0) {

            float[] min = { float.PositiveInfinity, float.PositiveInfinity };

            foreach (Enemy enemy in availableEnemies) {

                float[] currentEnemyDistance = enemy.DistanceToBase();

                if (currentEnemyDistance[0] < min[0] || (currentEnemyDistance[0] == min[0] && currentEnemyDistance[1] < min[1])) {

                    trackedTarget = enemy.transform;
                    min[0] = currentEnemyDistance[0];
                    min[1] = currentEnemyDistance[1];

                }

            }
        }
    }

    void RotateTowardsEnemy() {

        Vector2 direction = new Vector2(trackedTarget.transform.position.x - transform.position.x, trackedTarget.transform.position.y - transform.position.y);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        float rotationStrength = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
        Quaternion newRotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationStrength);

        transform.rotation = newRotation;

    }

    void RemoveFromTrackedEnemies(Enemy enemy) {
        availableEnemies.Remove(enemy);
    }

    public void EnemyEnteredRange(Enemy enemy) {

        enemy.OnDeath.AddListener(RemoveFromTrackedEnemies);
        availableEnemies.Add(enemy);

    }

    public void EnemyExitedRange(Enemy enemy) {

        enemy.OnDeath.RemoveListener(RemoveFromTrackedEnemies);
        availableEnemies.Remove(enemy);

    }

    public void IncreaseDamage()
    {
        explosionDamage += 10;
    }

    void Explode() {

        Instantiate(explosionEffect, transform.position, transform.rotation);

        Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in overlappingColliders) {

            if (collider.tag.Equals("Enemy")) {
                collider.GetComponent<Enemy>().TakeDamage(explosionDamage);
            }

        }

        Destroy(gameObject);

    }

}
