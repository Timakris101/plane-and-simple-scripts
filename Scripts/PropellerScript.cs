using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerScript : MonoBehaviour {

    private float idleCoef;
    [SerializeField] private float engineAccelRate;
    private bool engineOn;
    private bool engineBroken;

    private PlaneController pc;

    void OnTriggerEnter2D(Collider2D col) {
        if (col.transform.tag == "Ground") Destroy(gameObject);
    }

    void Start() {
        idleCoef = transform.parent.GetComponent<Aerodynamics>().getIdle();
        GetComponent<Animator>().speed = transform.parent.GetComponent<PlaneController>().getEnginesStartOn() ? 0 : 1;
    }

    void setPlaneController() {
        foreach (PlaneController c in transform.parent.GetComponents<PlaneController>()) {
            if (c.enabled) {
                pc = c;
                break;
            }
        } 
    }

    void Update() {
        setPlaneController();
        engineOn = pc.getEnginesOn();
        engineBroken = !transform.parent.Find("EngineHitbox").GetComponent<DamageModel>().isAlive();
        if (!engineBroken) {
            if (engineOn && GetComponent<Animator>().speed <= Mathf.Min(pc.getThrottle() + idleCoef, 1)) {
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
        if (engineOn) {
            GetComponent<SpriteRenderer>().sortingOrder *= -1;
        } else {
            GetComponent<SpriteRenderer>().sortingOrder = -1;
        }
    }
}
