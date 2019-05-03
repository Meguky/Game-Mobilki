using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Structure : MonoBehaviour, IDamageable<float> {

    protected float health = 100;

    public void TakeDamage(float dmg) {
        health -= dmg;
        if (health <= 0) {
            Die();
        }
    }

    public virtual void Die() {
        //W przyszłości kwestie graficzne umierania (animacje/eksplozje/particle etc.)
        Destroy(gameObject);
    }

}
