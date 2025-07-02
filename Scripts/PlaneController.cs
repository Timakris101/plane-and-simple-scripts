using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour {

    [Header("Thrust")]
    [SerializeField] private float throttle;
    [SerializeField] private float maxThrust;

    [Header("Lift / Induced Drag")]
    [SerializeField] private AnimationCurve cL;
    [SerializeField] private float wingArea;
    [SerializeField] private float wingSpan;
    [SerializeField] private float wingEfficiency = .8f;

    [Header("Drag")]
    [SerializeField] private float baseDragCoef = 0.02f;
    [SerializeField] private float frontArea;

    [Header("Torque")]
    [SerializeField] private AnimationCurve torqueStrength;
    [SerializeField] private float baseTorque;
    [SerializeField] private float alignmentStrength;
    [SerializeField] private float alignmentThresh = 45f;

    [Header("Atmosphere")]
    [SerializeField] private float airDensity;

    void Update() {
        handleControls();
    }

    void FixedUpdate() {
        handleThrust();
        handleDrag();
        handleLift();
        handleTorque();
    }

    private float AoA() {
        Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
        if (velocity.magnitude < 1f) return 0;
        return Vector2.SignedAngle(velocity, transform.right);
    }

    private void handleThrust() {
        GetComponent<Rigidbody2D>().AddForce(transform.right * throttle * maxThrust);
    }

    private float wingAspectRatio() {
        return (Mathf.Pow(wingSpan, 2)) / wingArea;
    }

    private void handleLift() {
        float liftForce = cL.Evaluate(AoA()) * airDensity * Mathf.Pow(GetComponent<Rigidbody2D>().velocity.magnitude, 2) * wingArea / 2f;
        Vector2 liftDir = Vector2.Perpendicular(GetComponent<Rigidbody2D>().velocity).normalized;
        GetComponent<Rigidbody2D>().AddForce(liftDir * liftForce);
    }

    private void handleDrag() {
        float inducedDragCoef = Mathf.Pow(cL.Evaluate(AoA()), 2) / (Mathf.PI * wingAspectRatio() * wingEfficiency);
        float totalDragCoef = inducedDragCoef + baseDragCoef;

        float dragForce = totalDragCoef * airDensity * Mathf.Pow(GetComponent<Rigidbody2D>().velocity.magnitude, 2) * frontArea;

        GetComponent<Rigidbody2D>().AddForce(-GetComponent<Rigidbody2D>().velocity.normalized * dragForce);
    }

    private int getWantedDir() {
        int val = 0;
        if (Input.GetKey("d")) {
            val = -1;
        }
        if (Input.GetKey("a")) {
            val = 1;
        }
        if (Input.GetKey("d") && Input.GetKey("a")) {
            val = 0;
        }
        return val;
    }

    private void handleTorque() {
        int dirToTurn = getWantedDir();
        GetComponent<Rigidbody2D>().angularVelocity += dirToTurn * torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude) * baseTorque * Time.fixedDeltaTime;
        if (GetComponent<Rigidbody2D>().velocity.magnitude > 1f && (AoA() > alignmentThresh || AoA() < -alignmentThresh)) GetComponent<Rigidbody2D>().angularVelocity += -AoA() * alignmentStrength * torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude) * Time.fixedDeltaTime;
    }

    private void handleControls() {
        if (Input.GetKey("w") && throttle < 1) throttle += 0.5f * Time.deltaTime;
        if (Input.GetKey("s") && throttle > 0) throttle -= 0.5f * Time.deltaTime;
    }
}
