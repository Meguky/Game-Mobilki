﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : Structure {

    private void Start() {
        replenishHealth();
    }

    public override void Upgrade() {
        return;
    }


}
