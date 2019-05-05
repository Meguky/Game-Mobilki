using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Enemy : MonoBehaviour, IDamageable<float>{

    public class EnemyEvent : UnityEvent<Enemy> { }

    protected float health = 100;
    public EnemyEvent OnDeath = new EnemyEvent();

    protected LinkedList<Vector3> currentPath = new LinkedList<Vector3>();
    protected LinkedListNode<Vector3> targetIterator;
    
    Vector3 movementDirection;
    Vector3 currentDestination;


    int currentTargetIndex = 0;
    protected float distanceFromNextWaypoint;
    protected float movementSpeed = 2;

    //zwraca pozycję na ścieżce waypointów i odległość od obecnego celu
    public float[] DistanceToBase() {
        return new[] { currentTargetIndex, distanceFromNextWaypoint };
    }

    public void TakeDamage(float dmg) {
        health -= dmg;
        if (health <= 0) {
            Die();
        }
    }

    private void GetNewPath() {
        currentPath = MapManager.instance.FindPathToBaseFrom(transform.position);
    }

    public void RegisterAsMapchangeListener() {
        MapManager.instance.OnMapChange.AddListener(GetNewPath);
    }

    public virtual void Die() {
        //W przyszłości kwestie graficzne umierania (animacje/eksplozje/particle etc.)
        OnDeath.Invoke(this);
        Destroy(gameObject);
    }

    protected virtual void GetNextTarget() {

        if (targetIterator == currentPath.Last) {
            return;
        }
        else {
            movementDirection = (targetIterator.Value - transform.position).normalized * movementSpeed;
            currentDestination = targetIterator.Value;
            targetIterator = targetIterator.Next;
            currentTargetIndex++;
        }

    }

    protected virtual void Move() {

        transform.Translate(movementDirection * Time.deltaTime, Space.World);

        distanceFromNextWaypoint = Vector3.Distance(transform.position, currentDestination);

        if (distanceFromNextWaypoint < 0.2f) {
            GetNextTarget();
        }
    }

}
