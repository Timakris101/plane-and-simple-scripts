using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerScript : MonoBehaviour {

    private float idleCoef;
    [SerializeField] private float engineAccelRate;
    private bool engineOn;
    private bool engineBroken;

    void OnTriggerEnter2D(Collider2D col) {
        if (col.transform.tag == "Ground") Destroy(gameObject);
    }

    void Start() {
        idleCoef = transform.parent.GetComponent<Aerodynamics>().getIdle();
        GetComponent<Animator>().speed = transform.parent.GetComponent<PlaneController>().getEnginesStartOn() ? 0 : 1;
    }

    void Update() {
        engineOn = transform.parent.GetComponent<PlaneController>().getEnginesOn();
        engineBroken = !transform.parent.Find("EngineHitbox").GetComponent<DamageModel>().isAlive();
        if (!engineBroken) {
            if (engineOn && GetComponent<Animator>().speed <= Mathf.Min(transform.parent.GetComponent<PlaneController>().getThrottle() + idleCoef, 1)) {
                GetComponent<Animator>().speed *= engineAccelRate;
                GetComponent<Animator>().speed += engineAccelRate - 1;
            } else {
                GetComponent<Animator>().speed /= engineAccelRate;
            }
        } else {
            if (engineOn && GetComponent<Animator>().speed <= idleCoef) {
                GetComponent<Animator>().speed *= engineAccelRate;
                GetComponent<Animator>().speed += engineAccelRate - 1;
            } else {
                GetComponent<Animator>().speed /= engineAccelRate;
            }
        }
        GetComponent<SpriteRenderer>().sortingOrder *= -1;
    }
}
