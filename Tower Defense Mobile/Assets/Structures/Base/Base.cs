using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : Structure {

    private void Start() {
        health = 1000.0f;    
    }

    public override void Upgrade()
    {
        return;
    }


}
