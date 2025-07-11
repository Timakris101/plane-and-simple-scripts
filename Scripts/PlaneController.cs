using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour {
    protected float throttle;
    protected bool inWEP;
    private float throttleChangeSpeed = 1f;
    private bool enginesOn;
    private bool unconcious;
    private bool pilotGone;
    [SerializeField] private bool enginesStartOn;

    private bool onGround;

    void OnCollisionStay2D() {
        onGround = true;
    }

    void OnCollisionExit2D() {
        onGround = false;
    }

    protected void Start() {
        enginesOn = enginesStartOn;
        throttle = enginesStartOn ? 1 : 0;
    }

    public bool planeDead() {
        if (transform.Find("PilotHitbox") == null) return true;
        if (!transform.Find("PilotHitbox").GetComponent<DamageModel>().isAlive()) return true;

        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;
            
            if (!transform.GetChild(i).GetComponent<DamageModel>().isCrewRole()) {
                if (!transform.GetChild(i).GetComponent<DamageModel>().isAlive()) {
                    return true;
                }
            }
        }

        return false;
    }

    protected void Update() {
        handleFeasibleControls();
        if (transform.Find("PilotHitbox") == null) {
            pilotGone = true;
            return;
        }
        unconcious = !transform.Find("PilotHitbox").GetComponent<DamageModel>().isAlive() || GetComponent<GForcesScript>().overGPilot();
    }

    public int getDir() {
        if (!unconcious && !pilotGone) return wantedDir();
        return 0;
    }

    protected virtual int wantedDir() {
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

    public virtual void handleFeasibleControls() {
        if (!unconcious && !pilotGone) handleControls();
        if (pilotGone) setGuns(false);

        bool anyCrewAlive = false;
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;
            
            if (transform.GetChild(i).GetComponent<DamageModel>().isCrewRole()) {
                if (transform.GetChild(i).GetComponent<DamageModel>().isAlive()) {
                    anyCrewAlive = true;
                    break;
                }
            }
        }
        if (anyCrewAlive) {
            handleNonPilotControls();
        }
    }

    protected virtual void handleNonPilotControls() {
        if (Input.GetKey("j")) {
            GetComponent<BailoutHandler>().callBailOut();
        }
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

        setGuns(Input.GetMouseButton(0));
    }

    private void setGuns(bool shooting) {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunScript>() != null) transform.GetChild(i).GetComponent<GunScript>().setShooting(shooting);
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
