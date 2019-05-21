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

    public override void Die() {
        TowerDefense.GameManager.instance.OnBaseDestroyed();
    }
}
