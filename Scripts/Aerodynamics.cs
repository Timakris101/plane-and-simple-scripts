using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aerodynamics : MonoBehaviour {

    [Header("Thrust")]
    [SerializeField] private float maxThrust;
    [SerializeField] private float idle;
    [SerializeField] private float WEP;

    [Header("Lift / Induced Drag")]
    [SerializeField] private AnimationCurve cL;
    [SerializeField] private float wingArea;
    [SerializeField] private float wingSpan;
    [SerializeField] private float wingEfficiency;

    [Header("Drag")]
    [SerializeField] private AnimationCurve cD;
    [SerializeField] private float frontArea;

    [Header("Torque")]
    [SerializeField] private AnimationCurve torqueStrength;
    [SerializeField] private float baseTorque;
    [SerializeField] private float alignmentStrength;
    [SerializeField] private float alignmentThresh;
    [SerializeField] private float speedOfControlEffectiveness;

    [Header("Atmosphere")]
    private static float seaLevelAirDensity = 20f;
    private static float scaleHeight = 8500f;

    private PlaneController pc;

    void Start() {
        setPlaneController();         
    }

    void Update() {
        setPlaneController();
    }

    void setPlaneController() {
        pc = null;
        foreach (PlaneController c in GetComponents<PlaneController>()) {
            if (c.enabled) {
                pc = c;
                break;
            }
        } 
    }

    void FixedUpdate() {
        setPlaneController();
        handleThrust();
        handleDrag();
        handleLift();
        handleTorque();

        GetComponent<Rigidbody2D>().centerOfMass = transform.Find("CoM").localPosition;
    }

    private float getAirDensity() {
        return seaLevelAirDensity / Mathf.Exp(transform.position.y / scaleHeight);
    }

    private void handleLift() {
        FlapScript fs = null;
        if (transform.Find("Flaps") != null) fs = transform.Find("Flaps").GetComponent<FlapScript>();
        float liftForce = (cL.Evaluate(AoA()) + (fs == null ? 0 : (fs.getFlapEffectiveness() * fs.deflection() / fs.getMaxDeflection()))) * getAirDensity() * Mathf.Pow(GetComponent<Rigidbody2D>().velocity.magnitude, 2) * wingArea / 2f;
        Vector2 liftDir = transform.localScale.y * Vector3.Cross(GetComponent<Rigidbody2D>().velocity, -transform.forward).normalized;
        GetComponent<Rigidbody2D>().AddForceAtPosition(liftDir * liftForce, transform.Find("CoL").position);
    }

    private void handleDrag() {
        FlapScript fs = null;
        if (transform.Find("Flaps") != null) fs = transform.Find("Flaps").GetComponent<FlapScript>();
        float inducedDragCoef = Mathf.Pow(cL.Evaluate(AoA()), 2) / (Mathf.PI * wingAspectRatio() * wingEfficiency);
        float totalDragCoef = inducedDragCoef + cD.Evaluate(AoA()) + (fs == null ? 0 : (fs.getFlapDrag() * fs.deflection() / fs.getMaxDeflection())) + (transform.Find("Gear") == null ? 0 : transform.Find("Gear").GetComponent<GearScript>().getGearDrag());

        float dragForce = totalDragCoef * getAirDensity() * Mathf.Pow(GetComponent<Rigidbody2D>().velocity.magnitude, 2) * frontArea;

        GetComponent<Rigidbody2D>().AddForce(-GetComponent<Rigidbody2D>().velocity.normalized * dragForce);
    }

    private void handleTorque() {
        if (pc != null) {
            float dirToTurn = pc.getDir();
            if (GetComponent<Rigidbody2D>().velocity.magnitude < speedOfControlEffectiveness) {
                return;
            }
            GetComponent<Rigidbody2D>().angularVelocity = dirToTurn * torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude) * baseTorque;
            bool positiveAoA = AoA() >= 0;
            int correctionDir = positiveAoA ? -1 : 1;
            if (AoA() > alignmentThresh || AoA() < -alignmentThresh) GetComponent<Rigidbody2D>().angularVelocity += correctionDir * Mathf.Abs(AoA()) * alignmentStrength * torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude) * transform.localScale.y;
        }
    }

    private void handleThrust() {
        if (pc != null) {
            bool anyPropellers = false;
            for (int i = 0; i < transform.childCount; i++) {
                if (transform.GetChild(i).GetComponent<PropellerScript>() != null) {
                    anyPropellers = true;
                    break;
                }
            }
            if (!anyPropellers) return;
            if (pc.getEnginesOn()) {
                GetComponent<Rigidbody2D>().AddForce(transform.right * (pc.getInWEP() ? WEP : Mathf.Min(idle + pc.getThrottle(), 1)) * maxThrust);
            }
        }
    }

    private float AoA() {
        Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
        if (velocity.magnitude < 1f) return 0;
        return Vector3.SignedAngle(velocity, transform.right, transform.forward) * transform.localScale.y;
    }

    private float wingAspectRatio() {
        return (Mathf.Pow(wingSpan, 2)) / wingArea;
    }

    public float getIdle() {
        return idle;
    }

    public float getWEP() {
        return WEP;
    }

    public void setBaseTorque(float val) {
        baseTorque = val;
    }

    public float getBaseTorque() {
        return baseTorque;
    }

    public void setAlignmentThresh(float val) {
        alignmentThresh = val;
    }

    public float getAlignmentThresh() {
        return alignmentThresh;
    }

    public void setSpeedOfControlEff(float val) {
        speedOfControlEffectiveness = val;
    }

    public float getSpeedOfControlEff() {
        return speedOfControlEffectiveness;
    }

    public void setWingArea(float val) {
        wingArea = val;
    }

    public float getWingArea() {
        return wingArea;
    }

    public void setWingEfficiency(float val) {
        wingEfficiency = val;
    }

    public float getWingEfficiency() {
        return wingEfficiency;
    }

    public void setMaxThrust(float val) {
        maxThrust = val;
    }

    public float getMaxThrust() {
        return maxThrust;
    }

    public float getFrontArea() {
        return frontArea;
    }

    public void setFrontArea(float val) {
        frontArea = val;
    }

    public void setAlignmentStrength(float val) {
        alignmentStrength = val;
    }

    public float getAlignmentStrength() {
        return alignmentStrength;
    }
}
