using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerScript : MonoBehaviour {

    private float idleCoef;
    [SerializeField] private float engineAccelRate;
    private bool engineOn;
    private bool engineBroken;
    [Tooltip("X: position x | Y: position y | Z: rotation z | W: order (0 means gone)")]
    [SerializeField] private Quaternion[] valsOfPropAtAnimIndexNonWingless;
    [Tooltip("X: position x | Y: position y | Z: rotation z | W: order (0 means gone)")]
    [SerializeField] private Quaternion[] valsOfPropAtAnimIndexWingless;

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

        Quaternion[] arrToUse = (transform.parent.GetComponent<Animator>().GetBool("Wingless") ? valsOfPropAtAnimIndexWingless : valsOfPropAtAnimIndexNonWingless);
        int indexToUse = int.Parse(transform.parent.GetComponent<SpriteRenderer>().sprite.name.Substring(transform.parent.GetComponent<SpriteRenderer>().sprite.name.IndexOf("_") + 1));
        transform.localPosition = new Vector3(arrToUse[indexToUse].x, arrToUse[indexToUse].y, 0f);
        transform.localEulerAngles = new Vector3(0, 0, arrToUse[indexToUse].z);
        GetComponent<SpriteRenderer>().sortingOrder = (int) arrToUse[indexToUse].w;
        GetComponent<SpriteRenderer>().enabled = arrToUse[indexToUse].w != 0;
    }
}
