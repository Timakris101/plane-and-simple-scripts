using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour {
    [Header("Thrust")]
    [SerializeField] protected float throttle;
    protected bool inWEP;
    [SerializeField] protected float throttleChangeSpeed;
    protected bool enginesOn;
    [SerializeField] protected bool enginesStartOn;

    [Header("GForces")]
    [SerializeField] private float rollOverThresh;
    [SerializeField] private float currentGs;
    private Vector3 prevVel;

    [Header("Death")]
    [SerializeField] private float crushSpeed;
    [SerializeField] private float pilotCrushSpeed;

    private bool onGround;

    private Sprite origSprite;

    void OnCollisionEnter2D(Collision2D col) {
        if (col.transform.tag == "Ground") {
            float rs = Vector3.Project(col.contacts[0].relativeVelocity, -col.contacts[0].normal).magnitude; //relative speed
            if (rs > crushSpeed) Destroy(gameObject);
            if (rs > pilotCrushSpeed) transform.Find("CockpitHitbox").GetComponent<DamageModel>().damage(rs);
        }
    }

    void OnCollisionStay2D() {
        onGround = true;
    }

    void OnCollisionExit2D() {
        onGround = false;
    }

    void Start() {
        origSprite = GetComponent<SpriteRenderer>().sprite;
        enginesOn = enginesStartOn;
    }

    void Update() {
        if (transform.Find("CockpitHitbox").GetComponent<DamageModel>().isAlive()) {
            handleControls();
            if (currentGs < rollOverThresh) {
                rollover();
            }
        }
        if (GetComponent<SpriteRenderer>().sprite == origSprite) {
            if (transform.Find("Gear") != null) transform.Find("Gear").GetComponent<GearScript>().unhideGear();
            if (transform.Find("Flaps") != null) transform.Find("Flaps").GetComponent<FlapScript>().unhideFlaps();
        }

        GetComponent<Rigidbody2D>().centerOfMass = transform.Find("CoM").localPosition;

        calculateGs();
    }

    private void rollover() {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * -1, transform.localScale.z);
        GetComponent<Animator>().SetTrigger("Rollover");
        if (transform.Find("Gear") != null) transform.Find("Gear").GetComponent<GearScript>().hideGear();
        if (transform.Find("Flaps") != null) transform.Find("Flaps").GetComponent<FlapScript>().hideFlaps();
    }

    public virtual int getWantedDir() {
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

        if (currentForces.magnitude != 0) currentGs = transform.localScale.y * ((currentForces.y / transform.up.y) + Mathf.Cos(Vector3.SignedAngle(transform.up, Vector3.up, transform.forward) / 180f * 3.14f));

        prevVel = GetComponent<Rigidbody2D>().velocity;
    }

    protected virtual void handleControls() {
        if (Input.GetKey("w") && throttle < 1) throttle += throttleChangeSpeed * Time.deltaTime;
        if (Input.GetKey("s") && throttle > 0) throttle -= throttleChangeSpeed * Time.deltaTime;

        inWEP = false;
        if (Input.GetKey("w") && throttle + throttleChangeSpeed * Time.deltaTime > 1) inWEP = true;

        if (Input.GetKeyDown("i")) toggleEngines();

        if (Input.GetKeyDown("f") && transform.Find("Flaps") != null) transform.Find("Flaps").GetComponent<FlapScript>().toggleFlaps();

        if (Input.GetKeyDown("g") && transform.Find("Gear") && !onGround) transform.Find("Gear").GetComponent<GearScript>().toggleGear();
        if (Input.GetKey("s") && throttle - throttleChangeSpeed * Time.deltaTime < 0 && transform.Find("Gear")) transform.Find("Gear").GetComponent<GearScript>().brake();

        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunScript>() != null) transform.GetChild(i).GetComponent<GunScript>().setShooting(Input.GetMouseButton(0));
        }
    }

    public void toggleEngines() {
        enginesOn = !enginesOn;
    }

    public float getThrottle() {
        return throttle;
    }

    public bool getEnginesOn() {
        return enginesOn;
    }

    public bool getEnginesStartOn() {
        return enginesStartOn;
    }

    public bool getInWEP() {
        return inWEP;
    }
}
