using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : Enemy {

    [SerializeField]
    private float movementSpeed = 5;

    Vector3 movementDirection;
    Transform currentDestination;
    int currentTargetIndex = 0;

    void GetNextTarget() {

        if (currentTargetIndex == PathController.waypoints.Length) {
            return;
        }
        else {
            movementDirection = (PathController.waypoints[currentTargetIndex].position - transform.position).normalized * movementSpeed;
            currentDestination = PathController.waypoints[currentTargetIndex];
            currentTargetIndex++;
        }

    }
    
    public override void Move() {
        transform.Translate(movementDirection * Time.deltaTime, Space.World);
        if (Vector3.Distance(transform.position, currentDestination.position) < 0.2f) {
            GetNextTarget();
        }
    }
    
    // Start is called before the first frame update
    void Start() {
        GetNextTarget();
    }

    // Update is called once per frame
    void Update() {
        Move();
    }

}
