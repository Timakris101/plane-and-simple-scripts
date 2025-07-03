using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour {

    [Header("Thrust")]
    [SerializeField] private float throttle;
    [SerializeField] private float maxThrust;
    [SerializeField] private float idle;
    [SerializeField] private float WEP;
    private bool inWEP;
    [SerializeField] private float throttleChangeSpeed;
    private bool enginesOn;
    [SerializeField] private bool enginesStartOn;

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

    [Header("GForces")]
    [SerializeField] private float rollOverThresh;
    [SerializeField] private float currentGs;
    private Vector3 prevVel;

    private Sprite origSprite;

    void Start() {
        origSprite = GetComponent<SpriteRenderer>().sprite;
        enginesOn = enginesStartOn;
    }

    void Update() {
        handleControls();
        if (currentGs < rollOverThresh) {
            rollover();
        }
        if (GetComponent<SpriteRenderer>().sprite == origSprite) {
            transform.Find("Gear").GetComponent<GearScript>().unhideGear();
            transform.Find("Flaps").GetComponent<FlapScript>().unhideFlaps();
        }

        GetComponent<Rigidbody2D>().centerOfMass = transform.Find("CoM").localPosition;

        calculateGs();
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
        if (enginesOn) GetComponent<Rigidbody2D>().AddForce(transform.right * (inWEP ? WEP : Mathf.Min(idle + throttle, 1)) * maxThrust);
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
        float totalDragCoef = inducedDragCoef + cD.Evaluate(AoA()) + (fs.getFlapDrag() * (transform.Find("Flaps").localEulerAngles.z - 90f) / fs.getMaxDeflection()) + transform.Find("Gear").GetComponent<GearScript>().getGearDrag();

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

    private void calculateGs() {
        Vector3 curVel = GetComponent<Rigidbody2D>().velocity;
        Vector3 currentForces = curVel - prevVel;
        
        currentForces = Vector3.Project(currentForces, transform.up);

        if (currentForces.magnitude != 0) currentGs = (currentForces.y / transform.up.y) + Mathf.Cos(Vector3.SignedAngle(transform.right, Vector3.right, transform.forward) / 180f * 3.14f);

        prevVel = GetComponent<Rigidbody2D>().velocity;
    }

    private void handleTorque() {
        int dirToTurn = getWantedDir();
        if (GetComponent<Rigidbody2D>().velocity.magnitude < speedOfControlEffectiveness) {
            return;
        }
        GetComponent<Rigidbody2D>().angularVelocity = dirToTurn * torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude) * baseTorque;
        bool fwdAxisAlignedWithWorld = Mathf.Abs(transform.eulerAngles.y) <= 1f && Mathf.Abs(transform.eulerAngles.y) <= 1f;
        bool positiveAoA = AoA() >= 0;
        int correctionDir = fwdAxisAlignedWithWorld ^ positiveAoA ? 1 : -1;
        if (AoA() > alignmentThresh || AoA() < -alignmentThresh) GetComponent<Rigidbody2D>().angularVelocity += correctionDir * Mathf.Abs(AoA()) * alignmentStrength * torqueStrength.Evaluate(GetComponent<Rigidbody2D>().velocity.magnitude);
    }

    private void handleControls() {
        if (Input.GetKey("w") && throttle < 1) throttle += throttleChangeSpeed * Time.deltaTime;
        if (Input.GetKey("s") && throttle > 0) throttle -= throttleChangeSpeed * Time.deltaTime;

        inWEP = false;
        if (Input.GetKey("w") && throttle + throttleChangeSpeed * Time.deltaTime > 1) inWEP = true;

        if (Input.GetKeyDown("i")) toggleEngines();

        if (Input.GetKeyDown("f")) transform.Find("Flaps").GetComponent<FlapScript>().toggleFlaps();

        if (Input.GetKeyDown("g")) transform.Find("Gear").GetComponent<GearScript>().toggleGear();
        if (Input.GetKey("s") && throttle - throttleChangeSpeed * Time.deltaTime < 0) transform.Find("Gear").GetComponent<GearScript>().brake();
    }

    public void toggleEngines() {
        enginesOn = !enginesOn;
    }

    public float getThrottle() {
        return throttle;
    }

    public float getIdle() {
        return idle;
    }

    public bool getEnginesOn() {
        return enginesOn;
    }

    public bool getEnginesStartOn() {
        return enginesStartOn;
    }
}
