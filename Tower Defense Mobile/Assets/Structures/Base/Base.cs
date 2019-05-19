using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : Structure {

    private void Start() {
        initializeValues(100,1);
    }

    public override void Upgrade(int levels) {
        return;
    }


}
