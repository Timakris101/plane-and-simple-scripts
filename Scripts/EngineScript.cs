using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineScript : MonoBehaviour {
    protected VehicleController vc;

    void Start() {
        setVehicleController(); 
    }

    void Update() {
        setVehicleController();
    }

    public virtual void setVal(float val) {}

    void setVehicleController() {
        vc = null;
        foreach (VehicleController c in transform.parent.GetComponents<VehicleController>()) {
            if (c.enabled) {
                vc = c;
                break;
            }
        } 
    }

    public virtual float getVal() {return 0f;}
    public virtual float getOverPowerVal() {return 0f;}

    public virtual float getThrustNewtons(float speed, bool reverse) {return 0f;}
    public virtual float getThrustNewtons(float speed) {return 0f;}
    public virtual float getThrustNewtons() {return 0f;}

    public virtual string getType() {return "";}
}
