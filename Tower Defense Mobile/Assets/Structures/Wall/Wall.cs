﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : Structure {
    // Start is called before the first frame update
    void Start() {
        buildingCost = 50.0f;
        structureName = "Wall";
    }

    // Update is called once per frame
    void Update() {

    }

    public override void Upgrade(int levels)
    {
        return;
    }
}
