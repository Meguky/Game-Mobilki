using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour{
    
    protected float health = 100;

    protected float movementSpeed = 2;

    protected float distanceFromNextWaypoint;

    Vector3 movementDirection;
    Transform currentDestination;
    int currentTargetIndex = 0;

    //zwraca pozycję na ścierzce waypointów i odległość od obecnego celu
    public float[] DistanceToBase() {
        return new[] { (float)currentTargetIndex, distanceFromNextWaypoint };
    }

    public void TakeDamage(int dmg) {
        health -= dmg;
        if (health<=0) {
            Die();
        }
    }

    public virtual void Die() {
        //W przyszłości kwestie graficzne umierania (animacje/eksplozje/particle etc.)
        Destroy(gameObject);
    }

    protected virtual void GetNextTarget() {

        if (currentTargetIndex == PathController.waypoints.Length) {
            return;
        }
        else {
            movementDirection = (PathController.waypoints[currentTargetIndex].position - transform.position).normalized * movementSpeed;
            currentDestination = PathController.waypoints[currentTargetIndex];
            currentTargetIndex++;
        }

    }

    protected virtual void Move() {

        transform.Translate(movementDirection * Time.deltaTime, Space.World);

        distanceFromNextWaypoint = Vector3.Distance(transform.position, currentDestination.position);

        if (distanceFromNextWaypoint < 0.2f) {
            GetNextTarget();
        }
    }

}
