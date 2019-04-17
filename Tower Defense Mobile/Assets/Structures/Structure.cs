using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Structure : MonoBehaviour {

    protected int health = 100;

    public void TakeDamage(int dmg) {
        health -= dmg;
        if (health <= 0) {
            Destruct();
        }
    }

    public virtual void Destruct() {
        //W przyszłości kwestie graficzne umierania (animacje/eksplozje/particle etc.)
        Destroy(gameObject);
    }

}
