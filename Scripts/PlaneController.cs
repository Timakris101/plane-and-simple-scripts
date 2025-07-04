using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour {
    [Header("Controls")]
    [SerializeField] protected float throttle;
    protected bool inWEP;
    [SerializeField] protected float throttleChangeSpeed;
    protected bool enginesOn;
    [SerializeField] protected bool enginesStartOn;

    private bool onGround;

    void OnCollisionStay2D() {
        onGround = true;
    }

    void OnCollisionExit2D() {
        onGround = false;
    }

    void Start() {
        enginesOn = enginesStartOn;
    }

    void Update() {
        if (transform.Find("CockpitHitbox").GetComponent<DamageModel>().isAlive() && !GetComponent<GForcesScript>().overGPilot()) {
            handleControls();
        }
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

    public virtual void handleControls() {
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
