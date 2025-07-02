using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearControl : MonoBehaviour {

    [SerializeField] private bool gearDownAtStart = true;
    [SerializeField] private PhysicsMaterial2D brakeMat;
    [SerializeField] private PhysicsMaterial2D rollMat;
    private float timeBraking;
    private float time;

    void Start() {
        if (gearDownAtStart) gearDown();
    }

    public void gearDown() {
        GetComponent<BoxCollider2D>().enabled = true;
        GetComponent<Animator>().SetBool("gearDeployed", true);
    }

    public void gearUp() {
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<Animator>().SetBool("gearDeployed", false);
    }

    public bool isGearDown() {
        return GetComponent<Animator>().GetBool("gearDeployed");
    }

    public void toggleGear() {
        if (isGearDown()) {
            gearUp();
        } else {
            gearDown();
        }
    }

    public void brake() {
        GetComponent<BoxCollider2D>().sharedMaterial = brakeMat;
        timeBraking++;
    }

    private void unbrakeIfNoBrake() {
        time++;
        if (time > timeBraking + 1) {
            time = 0;
            timeBraking = 0;
            GetComponent<BoxCollider2D>().sharedMaterial = rollMat;
        }
    }

    public void hideGear() {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    public void unhideGear() {
        GetComponent<SpriteRenderer>().enabled = true;
    }

    void Update() {
        unbrakeIfNoBrake();
    }
}
