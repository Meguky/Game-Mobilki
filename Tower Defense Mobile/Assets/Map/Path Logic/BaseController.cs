using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour {

    [SerializeField]
    int baseHP;

    void OnTriggerEnter2D(Collider2D collider) {
        baseHP-=10;
        Debug.Log("Base was attacked and has " + baseHP + "HP left!" );
        Destroy(collider.gameObject);
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
