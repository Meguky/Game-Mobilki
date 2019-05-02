using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSelfdestructer : MonoBehaviour {

    List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    // Start is called before the first frame update
    void Start() {
        particleSystems = new List<ParticleSystem>(GetComponents<ParticleSystem>());
    }

    // Update is called once per frame
    void Update() {

        if (particleSystems.Count == 0) {
            Destroy(gameObject);
        }

        for (int i = particleSystems.Count - 1; i >= 0; i--) {
            if (!particleSystems[i].IsAlive()) {
                particleSystems.RemoveAt(i);
            }
        }


    }
}
