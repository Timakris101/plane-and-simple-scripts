using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineScript : MonoBehaviour {
    
    [SerializeField] private AnimationCurve enginePowerByAlt;
    [Tooltip("One MUST be 0")]
    [SerializeField] private float thrustKn;
    [SerializeField] private float afterBurner;
    [Tooltip("One MUST be 0")]
    [SerializeField] private float powerHp;
    [SerializeField] private float wepHp;
    private float propEff = .7f;

    private PlaneController pc;

    void Start() {
        setPlaneController(); 
    }

    void Update() {
        setPlaneController();
    }

    void setPlaneController() {
        pc = null;
        foreach (PlaneController c in transform.parent.GetComponents<PlaneController>()) {
            if (c.enabled) {
                pc = c;
                break;
            }
        } 
    }

    public void setVal(float val) {
        if (thrustKn == 0) powerHp = val;
        if (powerHp == 0) thrustKn = val;
    }

    public float getVal() {
        return thrustKn + powerHp;
    }

    public float getOverPowerVal() {
        return afterBurner + wepHp;
    }

    public float getThrustNewtons(float speed) {
        if (powerHp == 0) return (pc.getInWEP() ? afterBurner : thrustKn) * 1000f * enginePowerByAlt.Evaluate(transform.position.y);

        bool anyPropellers = false;
        for (int i = 0; i < transform.parent.childCount; i++) {
            if (transform.parent.GetChild(i).GetComponent<PropellerScript>() != null) {
                anyPropellers = true;
                break;
            }
        }
        if (!anyPropellers) return 0;
        return (pc.getInWEP() ? wepHp : powerHp) / Mathf.Max(30f, speed) * 745.7f * enginePowerByAlt.Evaluate(transform.position.y) * propEff;
    }

    public string getType() {
        if (powerHp == 0) return "thrust";
        if (thrustKn == 0) return "power";
        return "";
    }
}
