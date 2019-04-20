using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {


    [SerializeField]
    float rotationSpeed = 15.0f;
    [SerializeField]
    float movementSpeed = 10;

    Transform trackedTarget;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (trackedTarget != null) {
            RotateTowardsEnemy();

            transform.Translate((trackedTarget.position - transform.position).normalized * movementSpeed * Time.deltaTime, Space.World);
        }
        else {
            Destroy(gameObject);
        }
    }

    public void InitialiseBullet(Transform target) {

        trackedTarget = target;

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
            other.GetComponent<Enemy>().TakeDamage(10);
            Destroy(gameObject);
        }
    }

}
