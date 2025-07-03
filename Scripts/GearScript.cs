using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearScript : MonoBehaviour {

    [SerializeField] private bool gearDownAtStart = true;
    [SerializeField] private float gearDrag;
    [SerializeField] private PhysicsMaterial2D brakeMat;
    [SerializeField] private PhysicsMaterial2D rollMat;
    [SerializeField] private Sprite gearDeployedSprite;
    [SerializeField] private Sprite gearUpSprite;

    private float timeBraking;
    private float time;

    void Start() {
        GetComponent<Animator>().SetBool("gearDownAtStart", gearDownAtStart);
        GetComponent<Animator>().SetBool("gearDeployed", gearDownAtStart);
        if (gearDownAtStart) {
            GetComponent<SpriteRenderer>().sprite = gearDeployedSprite;
        } else {
            GetComponent<SpriteRenderer>().sprite = gearUpSprite;
        }
    }

    public float getGearDrag() {
        return gearDrag;
    }

    public void gearDown() {
        GetComponent<Animator>().SetBool("gearDeployed", true);
    }

    public void gearUp() {
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
        GetComponent<BoxCollider2D>().enabled = GetComponent<SpriteRenderer>().sprite == gearDeployedSprite;
    }
}
