﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense {
    public class GameManager : MonoBehaviour {

        public static GameManager instance;
        
        void Start() {

            if (instance == null) {
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else {
                Destroy(gameObject);
            }

        }

    }
}