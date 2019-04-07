using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    [SerializeField]
    private float movementSpeed;

    Vector3 movementDirection;
    Transform currentDestination;
    int currentTargetIndex=0;

    void GetNextTarget() {

        if (currentTargetIndex == PathController.waypoints.Length) {
            Destroy(gameObject);
            return;
        }
        else {
            movementDirection = (PathController.waypoints[currentTargetIndex].position - transform.position).normalized * movementSpeed;
            currentDestination = PathController.waypoints[currentTargetIndex];
            currentTargetIndex++;
        }
        
    }

    // Start is called before the first frame update
    void Start() {
        GetNextTarget();
    }

    // Update is called once per frame
    void Update() {
        transform.Translate(movementDirection * Time.deltaTime, Space.World);
        if (Vector3.Distance(transform.position, currentDestination.position)<0.2f) {
            GetNextTarget();
        }
    }

}
