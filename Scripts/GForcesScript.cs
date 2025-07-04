using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GForcesScript : MonoBehaviour {
    [SerializeField] private float rollOverThresh;
    [SerializeField] private float currentGs;
    private Vector3 prevVel;
    private Sprite origSprite;
    [SerializeField] private float sleepyGs;
    [SerializeField] private float killingGs;
    [SerializeField] private float planeStructDestroyingGs;
    [SerializeField] private float planeDestroyingGs;
    
    void Start() {
        origSprite = GetComponent<SpriteRenderer>().sprite;
    }

    void Update() {
        if (currentGs < rollOverThresh) {
            rollover();
        }
        if (GetComponent<SpriteRenderer>().sprite == origSprite) {
            if (transform.Find("Gear") != null) transform.Find("Gear").GetComponent<GearScript>().unhideGear();
            if (transform.Find("Flaps") != null) transform.Find("Flaps").GetComponent<FlapScript>().unhideFlaps();
        }
        calculateGs();

        if (overGPlaneToDeath()) Destroy(gameObject);
        if (overGPlane()) transform.Find("WingHitbox").GetComponent<DamageModel>().kill();
        if (overGPilotToDeath()) transform.Find("CockpitHitbox").GetComponent<DamageModel>().kill();
    }

    private void rollover() {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * -1, transform.localScale.z);
        GetComponent<Animator>().SetTrigger("Rollover");
        if (transform.Find("Gear") != null) transform.Find("Gear").GetComponent<GearScript>().hideGear();
        if (transform.Find("Flaps") != null) transform.Find("Flaps").GetComponent<FlapScript>().hideFlaps();
    }

    private void calculateGs() {
        Vector3 curVel = GetComponent<Rigidbody2D>().velocity;
        Vector3 currentForces = curVel - prevVel;
        
        currentForces = Vector3.Project(currentForces, transform.up);

        if (currentForces.magnitude != 0) currentGs = transform.localScale.y * ((currentForces.y / transform.up.y) + Mathf.Cos(Vector3.SignedAngle(transform.up, Vector3.up, transform.forward) / 180f * 3.14f));

        prevVel = GetComponent<Rigidbody2D>().velocity;
    }

    public bool overGPlaneToDeath() {
        return Mathf.Abs(currentGs) > Mathf.Abs(planeDestroyingGs);
    }

    public bool overGPlane() {
        return Mathf.Abs(currentGs) > Mathf.Abs(planeStructDestroyingGs);
    }

    public bool overGPilotToDeath() {
        return Mathf.Abs(currentGs) > Mathf.Abs(killingGs);
    }

    public bool overGPilot() {
        return Mathf.Abs(currentGs) > Mathf.Abs(sleepyGs);
    }
}
