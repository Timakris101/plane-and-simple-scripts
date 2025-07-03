using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour {

    [Header("Thrust")]
    [SerializeField] private float throttle;
    [SerializeField] private float maxThrust;
    [SerializeField] private float idleCoef = 0.05f;
    [SerializeField] private bool enginesOn;
    [SerializeField] private bool enginesStartOn;

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
    private bool realignNeeded;

    [Header("Roll")]
    [SerializeField] private float rollOverThresh;

    [Header("Atmosphere")]
    [SerializeField] private float airDensity;

    private Sprite origSprite;

    void Start() {
        origSprite = GetComponent<SpriteRenderer>().sprite;
        enginesOn = enginesStartOn;
    }

    void Update() {
        handleControls();
        if (AoA() < -rollOverThresh / Mathf.Sqrt(GetComponent<Rigidbody2D>().velocity.magnitude) && !realignNeeded) {
            rollover();
        }
        if (GetComponent<SpriteRenderer>().sprite == origSprite) {
            transform.Find("Gear").GetComponent<GearScript>().unhideGear();
            transform.Find("Flaps").GetComponent<FlapScript>().unhideFlaps();
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
        transform.Find("Gear").GetComponent<GearScript>().hideGear();
        transform.Find("Flaps").GetComponent<FlapScript>().hideFlaps();
    }

    private float AoA() {
        Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
        if (velocity.magnitude < 1f) return 0;
        return Vector3.SignedAngle(velocity, transform.right, transform.forward);
    }

    private void handleThrust() {
        GetComponent<Rigidbody2D>().AddForce(transform.right * Mathf.Min(idleCoef + throttle, 1) * maxThrust);
    }

    private float wingAspectRatio() {
        return (Mathf.Pow(wingSpan, 2)) / wingArea;
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
        float totalDragCoef = inducedDragCoef + baseDragCoef + (fs.getFlapDrag() * (transform.Find("Flaps").localEulerAngles.z - 90f) / fs.getMaxDeflection()) + transform.Find("Gear").GetComponent<GearScript>().getGearDrag();

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
        realignNeeded = GetComponent<Rigidbody2D>().velocity.magnitude > 1f && (AoA() > alignmentThresh || AoA() < -alignmentThresh);
        int dirToTurn = realignNeeded ? 0 : getWantedDir();
        if (dirToTurn == 0 && torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude) > .1f) {
            GetComponent<Rigidbody2D>().angularVelocity /= 1 + torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude);
            return;
        }
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

        if (Input.GetKeyDown("i")) toggleEngines();

        if (Input.GetKeyDown("f")) transform.Find("Flaps").GetComponent<FlapScript>().toggleFlaps();

        if (Input.GetKeyDown("g")) transform.Find("Gear").GetComponent<GearScript>().toggleGear();
        if (Input.GetKey("s") && throttle <= 0) transform.Find("Gear").GetComponent<GearScript>().brake();
    }

    public void toggleEngines() {
        enginesOn = !enginesOn;
    }

    public float getThrottle() {
        return throttle;
    }

    public float getIdle() {
        return idleCoef;
    }

    public bool getEnginesOn() {
        return enginesOn;
    }

    public bool getEnginesStartOn() {
        return enginesStartOn;
    }
}
