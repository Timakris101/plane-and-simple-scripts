using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class PlaneController : VehicleController {
    protected bool inWEP;
    private float throttleChangeSpeed = 1f;
    private bool pilotDead => !transform.Find("PilotHitbox").GetComponent<DamageModel>().isAlive();
    private bool pilotGone => transform.Find("PilotHitbox") == null;
    private bool unconcious => GetComponent<GForcesScript>().isPersonSleepy();
    private bool onGround;

    void OnCollisionStay2D() {
        onGround = true;
    }

    void OnCollisionExit2D() {
        onGround = false;
    }

    void OnEnable() {
        setGunnersToManual(false);
    }

    public override bool whenToRemoveCamera() {return pilotDeadOrGone();}

    public override bool vehicleDead() {
        bool criticalSystemDamage = false;
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;
            
            if (!transform.GetChild(i).GetComponent<DamageModel>().isCrewRole() && transform.GetChild(i).GetComponent<DamageModel>().isCritical()) {
                if (!transform.GetChild(i).GetComponent<DamageModel>().isAlive()) {
                    criticalSystemDamage = true;
                    break;
                }
            }
        }
        if (allCrewGoneFromVehicle()) return true;
        return criticalSystemDamage || pilotDeadOrGone();
    }

    void Update() {
        base.Update();
        if (pilotGone) {
            return;
        }
    }

    public void removeCam() {
        if (transform.Find("Camera") != null) transform.Find("Camera").GetComponent<CamScript>().uncoupleCam();
    }

    public bool pilotDeadOrGone() {
        if (pilotGone) {
            return true;
        }
        return pilotDead;
    }

    public float getDir() {
        if (!pilotDeadOrGone()) {
            if (gunnersAreManual()) {
                return GetComponent<AiPlaneController>().wantedDir() * (unconcious ? Constants.GForceEffectConstants.unconciousPilotEffectiveness : 1f);
            } else {
                return wantedDir() * (unconcious ? Constants.GForceEffectConstants.unconciousPilotEffectiveness : 1f);
            }
        }
        return 0;
    }

    protected virtual float wantedDir() {
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

    public override void handleFeasibleControls() {
        if (!pilotDeadOrGone() && !unconcious) {
            if (gunnersAreManual()) {
                GetComponent<AiPlaneController>().handleControls();
            } else {
                handleControls();
            }
        }
        if (pilotDeadOrGone()) setGuns(false);
        
        if (!allCrewGoneFromVehicle()) {
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
        if (Input.GetKey("w") && getThrottle() < 1) setThrottle(getThrottle() + throttleChangeSpeed * Time.deltaTime);
        if (Input.GetKey("s") && getThrottle() > 0) setThrottle(getThrottle() - throttleChangeSpeed * Time.deltaTime);

        inWEP = false;
        if (Input.GetKey("w") && getThrottle() + throttleChangeSpeed * Time.deltaTime > 1) inWEP = true;

        if (Input.GetKeyDown("i")) toggleEngines();

        if (Input.GetKeyDown("f") && transform.Find("Flaps") != null) transform.Find("Flaps").GetComponent<FlapScript>().toggleFlaps();

        if (Input.GetKeyDown("g") && transform.Find("Gear") && !onGround) {
            for (int i = 0; i < transform.childCount; i++) {
                if (transform.GetChild(i).GetComponent<GearScript>() != null) {
                    transform.GetChild(i).GetComponent<GearScript>().toggleGear();
                }
            }
        }
        if (Input.GetKey("s") && getThrottle() - throttleChangeSpeed * Time.deltaTime < 0 && transform.Find("Gear")) transform.Find("Gear").GetComponent<GearScript>().brake();

        setGuns(Input.GetMouseButton(0));
        setBombs(Input.GetKey(KeyCode.Space));
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
        progenyWithScript("EngineScript", gameObject)[0].GetComponent<EngineScript>().setEngines(!progenyWithScript("EngineScript", gameObject)[0].GetComponent<EngineScript>().getEnginesOn());
    }

    public void setEngines(bool b) {
        progenyWithScript("EngineScript", gameObject)[0].GetComponent<EngineScript>().setEngines(b);
    }

    public void setThrottle(float val) {
        progenyWithScript("EngineScript", gameObject)[0].GetComponent<EngineScript>().setThrottle(val);
    }

    public float getThrottle() {
        return progenyWithScript("EngineScript", gameObject)[0].GetComponent<EngineScript>().getThrottle();
    }

    public bool getEnginesOn() {
        return progenyWithScript("EngineScript", gameObject)[0].GetComponent<EngineScript>().getEnginesOn();
    }

    public bool getInWEP() {
        return inWEP;
    }
}
