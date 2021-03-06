﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class SaveTileContent {
    public int level;
    public string name;
}

public class SaveState {

    public float money = 1000;
    public int waveNumber = 1;
    public SaveTileContent[] tiles = new SaveTileContent[18 * 28];

    public SaveState() {
        for (int i = 0; i < 28 * 18; i++) {
            tiles[i] = new SaveTileContent();
        }
    }

    public void SetMapTiles(MapTile[,] _mapTiles) {
        for (int j = 0; j < 28; j++) {
            for (int i = 0; i < 18; i++) {
                if (_mapTiles[i, j].builtStructure != null) {

                    tiles[i + j * 18].name = _mapTiles[i, j].builtStructure.GetStructureName();
                    tiles[i + j * 18].level = _mapTiles[i, j].builtStructure.GetStructureLevel();

                }
                else {
                    tiles[i + j * 18] = new SaveTileContent();
                }
            }
        }
    }
}
