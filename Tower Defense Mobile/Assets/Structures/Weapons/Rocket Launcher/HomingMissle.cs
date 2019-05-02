using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissle : MonoBehaviour {

    [SerializeField]
    float rotationSpeed = 0.5f;
    [SerializeField]
    float movementSpeed = 10;
    [SerializeField]
    float damage = 50;

    Transform trackedTarget;
    RocketLauncher originLauncher;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

        if (trackedTarget == null) {
            trackedTarget = originLauncher.GetTargetUpdate();
        }

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

    }

    public void InitialiseBullet(Transform target, RocketLauncher origin) {

        trackedTarget = target;
        originLauncher = origin;

    }

    void RotateTowardsEnemy() {

        Vector2 direction = new Vector2(trackedTarget.transform.position.x - transform.position.x, trackedTarget.transform.position.y - transform.position.y);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        float rotationStrength = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
        Quaternion newRotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationStrength);

        transform.rotation = newRotation;

    }

    void OnTriggerEnter2D(Collider2D other) {
        if (Transform.ReferenceEquals(trackedTarget, other.transform)) {
            other.GetComponent<Enemy>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }

}
