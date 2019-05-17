using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class MapTile {

        public enum TileType { Walkable, NonWalkable, NonFlyable }

        //core info
        public Vector2Int gridLocation;

        //building
        public bool buildable = true;
        public TileType tileType = TileType.Walkable;
        public Structure builtStructure = null;

        //pathfinding
        public MapTile parent = null;
        public int gCost;
        public int hCost;

        public int fCost {
            get { return gCost + hCost; }
        }

}