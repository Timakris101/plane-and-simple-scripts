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

    void Start() {
        enginesOn = enginesStartOn;
        throttle = enginesStartOn ? 1 : 0;
        setGunnersToManual(false);
    }

    public bool planeDead() {
        bool criticalSystemDamage = false;
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;
            
            if (!transform.GetChild(i).GetComponent<DamageModel>().isCrewRole()) {
                if (!transform.GetChild(i).GetComponent<DamageModel>().isAlive()) {
                    criticalSystemDamage = true;
                    break;
                }
            }
        }
        if (allCrewGoneFromPlane()) return true;
        return criticalSystemDamage || pilotDeadOrGone();
    }

    void Update() {
        handleFeasibleControls();
        if (transform.Find("PilotHitbox") == null) {
            pilotGone = true;
            return;
        }
        unconcious = !transform.Find("PilotHitbox").GetComponent<DamageModel>().isAlive() || GetComponent<GForcesScript>().overGPerson();
        if (transform.Find("Camera") == null) {
            foreach (PlaneController controller in GetComponents<PlaneController>()) {
                if (controller == GetComponent<AiPlaneController>()) {
                    controller.enabled = true;
                } else {
                    controller.enabled = false;
                }
            }
        }
    }

    public void removeCam() {
        if (transform.Find("Camera") != null) transform.Find("Camera").GetComponent<CamScript>().uncoupleCam();
    }

    public bool allCrewGoneFromPlane() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;
            
            if (transform.GetChild(i).GetComponent<DamageModel>().isCrewRole()) {
                if (transform.GetChild(i).GetComponent<DamageModel>().isAlive()) {
                    return false;
                }
            }
        }
        return true;
    }

    public bool pilotDeadOrGone() {
        if (transform.Find("PilotHitbox") == null) {
            return true;
        }
        return !transform.Find("PilotHitbox").GetComponent<DamageModel>().isAlive();
    }

    public int getDir() {
        if (!unconcious && !pilotGone) {
            if (gunnersAreManual()) {
                return GetComponent<AiPlaneController>().wantedDir();
            } else {
                return wantedDir();
            }
        }
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
        if (!unconcious && !pilotGone) {
            if (gunnersAreManual()) {
                GetComponent<AiPlaneController>().handleControls();
            } else {
                handleControls();
            }
        }
        if (pilotGone) setGuns(false);
        
        if (!allCrewGoneFromPlane()) {
            handleNonPilotControls();
            handleSwapping();
        }
    }

    protected virtual void handleNonPilotControls() {
        if (Input.GetKey("j")) {
            GetComponent<BailoutHandler>().callBailOut();
        }
    }

    private void handleSwapping() {
        if (Input.GetKeyDown("v")) {
            toggleGunners();
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
        setBombs(Input.GetKey(KeyCode.Space));
    }

    private void toggleGunners() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunnerScript>() != null) {
                transform.GetChild(i).GetComponent<GunnerScript>().setManualControl(!transform.GetChild(i).GetComponent<GunnerScript>().getManualControl());
            }
        }
    }

    private void setGunnersToManual(bool manual) {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunnerScript>() != null) transform.GetChild(i).GetComponent<GunnerScript>().setManualControl(manual);
        }
    }

    public bool gunnersAreManual() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunnerScript>() != null) return transform.GetChild(i).GetComponent<GunnerScript>().getManualControl();
        }
        return false;
    }

    protected void setGuns(bool shooting) {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunScript>() != null && transform.GetChild(i).GetComponent<BombHolderScript>() == null) transform.GetChild(i).GetComponent<GunScript>().setShooting(shooting);
        }
    }

    protected void setBombs(bool bombing) {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<BombHolderScript>() != null) {
                transform.GetChild(i).GetComponent<BombHolderScript>().setShooting(false);
            }
        }
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<BombHolderScript>() != null && transform.GetChild(i).GetComponent<BombHolderScript>().getAmmo() != 0) {
                transform.GetChild(i).GetComponent<BombHolderScript>().setShooting(bombing);

                if (transform.GetChild(i).GetComponent<BombHolderScript>().getAmmo() == 1) {
                    resetTimerOfBombholderAfterIndex(i);
                }

                break;
            }
        }
    }

    private void resetTimerOfBombholderAfterIndex(int i) {
        for (int j = i + 1; j < transform.childCount; j++) {
            if (transform.GetChild(j).GetComponent<BombHolderScript>() != null) {
                transform.GetChild(j).GetComponent<BombHolderScript>().setTimer(0);
            }
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
