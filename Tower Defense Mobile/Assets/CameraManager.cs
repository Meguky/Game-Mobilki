using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    float positionUpperBound = 17.8f;
    float positionLowerBound = 0.0f;

    [Header("Parameters")]
    [SerializeField] float movementSpeed = 4.0f;
    [SerializeField] float movementStrength = 0.4f;

    Vector3 targetPosition;

    public void MoveCameraBy(Vector3 distance) {

        targetPosition = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y + distance.y * movementStrength, positionLowerBound, positionUpperBound), transform.position.z);

    }

    void Start() {
        targetPosition = transform.position;
    }

    void Update() {
        if ((transform.position-targetPosition).magnitude>0.1f) {
            float translationSpeed = Mathf.Min(movementSpeed * Time.deltaTime, 1);
            transform.position = Vector3.Lerp(transform.position, targetPosition, translationSpeed);
        }
    }



}
