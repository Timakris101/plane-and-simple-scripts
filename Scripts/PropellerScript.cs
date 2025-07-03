using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerScript : MonoBehaviour {

    private float idleCoef;
    [SerializeField] private float engineAccelRate;
    private bool engineOn;

    void Start() {
        idleCoef = transform.parent.GetComponent<PlaneController>().getIdle();
        GetComponent<Animator>().speed = transform.parent.GetComponent<PlaneController>().getEnginesStartOn() ? 0 : 1;
    }

    void Update() {
        engineOn = transform.parent.GetComponent<PlaneController>().getEnginesOn();
        if (engineOn && GetComponent<Animator>().speed <= Mathf.Min(transform.parent.GetComponent<PlaneController>().getThrottle() + idleCoef, 1)) {
            GetComponent<Animator>().speed *= engineAccelRate;
            GetComponent<Animator>().speed += engineAccelRate - 1;
        } else {
            GetComponent<Animator>().speed /= engineAccelRate;
        }
    }
}
