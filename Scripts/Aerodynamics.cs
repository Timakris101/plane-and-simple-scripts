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
    [SerializeField] private float airDensity;

    private PlaneController pc;

    void Start() {
        pc = GetComponent<PlaneController>();
    }

    void FixedUpdate() {
        handleThrust();
        handleDrag();
        handleLift();
        handleTorque();
    }

    private void handleLift() {
        FlapScript fs = transform.Find("Flaps").GetComponent<FlapScript>();
        float liftForce = (cL.Evaluate(AoA()) + (fs.getFlapEffectiveness() * (transform.Find("Flaps").localEulerAngles.z - 90f) / fs.getMaxDeflection())) * airDensity * Mathf.Pow(GetComponent<Rigidbody2D>().velocity.magnitude, 2) * wingArea / 2f;
        Vector2 liftDir = Vector3.Cross(GetComponent<Rigidbody2D>().velocity, -transform.forward).normalized;
        GetComponent<Rigidbody2D>().AddForceAtPosition(liftDir * liftForce, transform.Find("CoL").position);
    }

    private void handleDrag() {
        FlapScript fs = transform.Find("Flaps").GetComponent<FlapScript>();
        float inducedDragCoef = Mathf.Pow(cL.Evaluate(AoA()), 2) / (Mathf.PI * wingAspectRatio() * wingEfficiency);
        float totalDragCoef = inducedDragCoef + cD.Evaluate(AoA()) + (fs.getFlapDrag() * (transform.Find("Flaps").localEulerAngles.z - 90f) / fs.getMaxDeflection()) + transform.Find("Gear").GetComponent<GearScript>().getGearDrag();

        float dragForce = totalDragCoef * airDensity * Mathf.Pow(GetComponent<Rigidbody2D>().velocity.magnitude, 2) * frontArea;

        GetComponent<Rigidbody2D>().AddForce(-GetComponent<Rigidbody2D>().velocity.normalized * dragForce);
    }

    private void handleTorque() {
        int dirToTurn = GetComponent<PlaneController>().getWantedDir();
        if (GetComponent<Rigidbody2D>().velocity.magnitude < speedOfControlEffectiveness) {
            return;
        }
        GetComponent<Rigidbody2D>().angularVelocity = dirToTurn * torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude) * baseTorque;
        bool fwdAxisAlignedWithWorld = Mathf.Abs(transform.eulerAngles.y) <= 1f && Mathf.Abs(transform.eulerAngles.y) <= 1f;
        bool positiveAoA = AoA() >= 0;
        int correctionDir = fwdAxisAlignedWithWorld ^ positiveAoA ? 1 : -1;
        if (AoA() > alignmentThresh || AoA() < -alignmentThresh) GetComponent<Rigidbody2D>().angularVelocity += correctionDir * Mathf.Abs(AoA()) * alignmentStrength * torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude);
    }

    private void handleThrust() {
        if (pc.getEnginesOn()) GetComponent<Rigidbody2D>().AddForce(transform.right * (pc.getInWEP() ? WEP : Mathf.Min(idle + pc.getThrottle(), 1)) * maxThrust);
    }

    private float AoA() {
        Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
        if (velocity.magnitude < 1f) return 0;
        return Vector3.SignedAngle(velocity, transform.right, transform.forward);
    }

    private float wingAspectRatio() {
        return (Mathf.Pow(wingSpan, 2)) / wingArea;
    }

    public float getIdle() {
        return idle;
    }
}
