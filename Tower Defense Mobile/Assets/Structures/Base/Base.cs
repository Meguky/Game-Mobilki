using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : Structure {

    private void Start() {
        health = maxHealth;
    }

    public override void Upgrade(int levels) {
        return;
    }

    public override void TakeDamage(float dmg) {
        if (health-dmg>0) {
            health -= dmg;
            StartCoroutine(UpdateHealthbar());
        }
        else {
            health = 0;
            StartCoroutine(UpdateHealthbar());
            Die();
        }
    }

    public override void Die() {
        EndlessBitDefense.GameManager.instance.OnBaseDestroyed();
    }
}
