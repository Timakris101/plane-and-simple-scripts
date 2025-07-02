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
    [SerializeField] private float alignmentThresh;

    [Header("Roll")]
    [SerializeField] private float rollOverThresh;

    [Header("Atmosphere")]
    [SerializeField] private float airDensity;

    private Sprite origSprite;

    void Start() {
        origSprite = GetComponent<SpriteRenderer>().sprite;
    }

    void Update() {
        handleControls();
        if (AoA() < -rollOverThresh / GetComponent<Rigidbody2D>().velocity.magnitude) {
            rollover();
        }
        if (GetComponent<SpriteRenderer>().sprite == origSprite) {
            transform.Find("Gear").GetComponent<GearControl>().unhideGear();
        }
        GetComponent<Rigidbody2D>().centerOfMass = transform.Find("CoM").localPosition;
    }

    void FixedUpdate() {
        handleThrust();
        handleDrag();
        handleLift();
        handleTorque();
    }

    private void rollover() {
        transform.RotateAround(transform.position, transform.right, 180f);
        GetComponent<Animator>().SetTrigger("Rollover");
        transform.Find("Gear").GetComponent<GearControl>().hideGear();
    }

    private float AoA() {
        Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
        if (velocity.magnitude < 1f) return 0;
        return Vector3.SignedAngle(velocity, transform.right, transform.forward);
    }

    private void handleThrust() {
        GetComponent<Rigidbody2D>().AddForce(transform.right * throttle * maxThrust);
    }

    private float wingAspectRatio() {
        return (Mathf.Pow(wingSpan, 2)) / wingArea;
    }

    private void handleLift() {
        float liftForce = cL.Evaluate(AoA()) * airDensity * Mathf.Pow(GetComponent<Rigidbody2D>().velocity.magnitude, 2) * wingArea / 2f;
        Vector2 liftDir = Vector3.Cross(GetComponent<Rigidbody2D>().velocity, -transform.forward).normalized;
        GetComponent<Rigidbody2D>().AddForceAtPosition(liftDir * liftForce, transform.Find("CoL").position);
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
        if (dirToTurn == 0 && torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude) > .1f) {
            GetComponent<Rigidbody2D>().angularVelocity /= 1 + torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude);
            return;
        }
        bool realignNeeded = GetComponent<Rigidbody2D>().velocity.magnitude > 1f && (AoA() > alignmentThresh || AoA() < -alignmentThresh);
        if (dirToTurn == 0 && !realignNeeded) {
            return;
        }
        if (torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude) < .1f && !realignNeeded) {
            return;
        }
        GetComponent<Rigidbody2D>().angularVelocity = dirToTurn * torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude) * baseTorque;
        if (realignNeeded) GetComponent<Rigidbody2D>().angularVelocity = -AoA() * alignmentStrength * torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude);
    }

    private void handleControls() {
        if (Input.GetKey("w") && throttle < 1) throttle += 0.5f * Time.deltaTime;
        if (Input.GetKey("s") && throttle > 0) throttle -= 0.5f * Time.deltaTime;

        if (Input.GetKeyDown("g")) transform.Find("Gear").GetComponent<GearControl>().toggleGear();
        if (Input.GetKey("s") && throttle <= 0) transform.Find("Gear").GetComponent<GearControl>().brake();
    }

    public float getThrottle() {
        return throttle;
    }
}
