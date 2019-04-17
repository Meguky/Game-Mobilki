using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : Enemy {
    
    // Start is called before the first frame update
    void Start() {
        GetNextTarget();
    }

    // Update is called once per frame
    void Update() {
        Move();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag.Equals("Structure")) {
            other.GetComponent<Structure>().TakeDamage(10);
            Destroy(gameObject);
        }
    }
}
