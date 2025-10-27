using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class Aerodynamics : MonoBehaviour {

    [Header("Thrust")]
    [SerializeField] private EngineScript es;

    [Header("Lift / Induced Drag")]
    [SerializeField] private AnimationCurve cL;
    [SerializeField] private float wingArea;
    private float startWingArea;
    [SerializeField] private float wingSpan;
    [SerializeField] private float wingEfficiency;

    [Header("Drag")]
    [SerializeField] private AnimationCurve cD;
    [SerializeField] private float frontArea;

    [Header("Torque")]
    [SerializeField] private AnimationCurve cT;
    [SerializeField] private AnimationCurve torqueStrength;
    [SerializeField] private float irlTurnTime;
    private float baseTorque;
    private float instantaneousTurnRateFactor = 1.5f;
    [SerializeField] private float speedOfControlEffectiveness;
    private float elevatorArea = .15f;

    [Header("Atmosphere")]
    private static float seaLevelAirDensity = 20f;
    private static float normalSeaLevelAirDensity = 1.225f;
    private static float scaleHeight = 8500f;

    private PlaneController pc;

    void Awake() {
        baseTorque = 360f / irlTurnTime * Mathf.Pow(seaLevelAirDensity / normalSeaLevelAirDensity, 1f/3f) * instantaneousTurnRateFactor;
        startWingArea = wingArea;
    }

    void Start() {
        setPlaneController(); 
        es = progenyWithScript("EngineScript", gameObject)[0].GetComponent<EngineScript>();        
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
        float liftForce = .5f * (cL.Evaluate(AoA()) + (fs == null ? 0 : (fs.getFlapEffectiveness() * fs.deflection() / fs.getMaxDeflection()))) * getAirDensity() * Mathf.Pow(GetComponent<Rigidbody2D>().linearVelocity.magnitude, 2) * wingArea;
        Vector2 liftDir = transform.localScale.y * Vector3.Cross(GetComponent<Rigidbody2D>().linearVelocity, -transform.forward).normalized;
        GetComponent<Rigidbody2D>().AddForceAtPosition(liftDir * liftForce, transform.Find("CoL").position);
    }

    private void handleDrag() {
        FlapScript fs = null;
        float frontAreaPlusElev = frontArea + elevatorArea * Mathf.Abs(pc.getDir());
        if (transform.Find("Flaps") != null) fs = transform.Find("Flaps").GetComponent<FlapScript>();
        float inducedDragCoef = Mathf.Pow(cL.Evaluate(AoA()), 2) / (Mathf.PI * wingAspectRatio() * wingEfficiency);
        float totalDragCoef = inducedDragCoef + cD.Evaluate(AoA()) + (fs == null ? 0 : (fs.getFlapDrag() * fs.deflection() / fs.getMaxDeflection())) + (transform.Find("Gear") == null ? 0 : transform.Find("Gear").GetComponent<GearScript>().getGearDrag());

        float dragForce = .5f * totalDragCoef * getAirDensity() * Mathf.Pow(GetComponent<Rigidbody2D>().linearVelocity.magnitude, 2) * frontAreaPlusElev;

        GetComponent<Rigidbody2D>().AddForce(-GetComponent<Rigidbody2D>().linearVelocity.normalized * dragForce);
    }

    private void handleTorque() {
        if (pc != null) {
            float dirToTurn = pc.getDir();

            if (GetComponent<Rigidbody2D>().linearVelocity.magnitude < speedOfControlEffectiveness) return;
                
            GetComponent<Rigidbody2D>().angularVelocity = dirToTurn * torqueStrength.Evaluate(GetComponent<Rigidbody2D>().linearVelocity.magnitude) * baseTorque;

            float torque = .5f * cT.Evaluate(AoA()) * transform.localScale.y * Mathf.Pow(GetComponent<Rigidbody2D>().linearVelocity.magnitude, 2) * Mathf.Max(wingArea, startWingArea / 2f) * wingSpan * getAirDensity();
            if (Mathf.Abs(AoA()) > 3f) {
                GetComponent<Rigidbody2D>().angularVelocity += torque / GetComponent<Rigidbody2D>().mass;
            }
        }
    }

    private void handleThrust() {
        GetComponent<Rigidbody2D>().AddForce(transform.right * es.getThrustNewtons(GetComponent<Rigidbody2D>().linearVelocity.magnitude));
    }

    private float AoA() {
        Vector2 velocity = GetComponent<Rigidbody2D>().linearVelocity;
        if (velocity.magnitude < 1f) return 0;
        return Vector3.SignedAngle(velocity, transform.right, transform.forward) * transform.localScale.y;
    }

    private float wingAspectRatio() {
        return (Mathf.Pow(wingSpan, 2)) / wingArea;
    }

    public void setBaseTorque(float val) {
        baseTorque = val;
    }

    public float getBaseTorque() {
        return baseTorque;
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

    public float getFrontArea() {
        return frontArea;
    }

    public void setFrontArea(float val) {
        frontArea = val;
    }
}
