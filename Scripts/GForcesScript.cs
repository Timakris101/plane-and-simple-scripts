using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GForcesScript : MonoBehaviour {
    [SerializeField] private float rollOverThresh;
    [SerializeField] private Vector3 currentGs;
    [SerializeField] private float feltGs;
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
        if (feltGs < rollOverThresh) {
            rollover();
        }
        if (GetComponent<SpriteRenderer>().sprite == origSprite) {
            if (transform.Find("Gear") != null) transform.Find("Gear").GetComponent<GearScript>().unhideGear();
            if (transform.Find("Flaps") != null) transform.Find("Flaps").GetComponent<FlapScript>().unhideFlaps();
        }

        if (overGPlaneToDeath()) Destroy(gameObject);
        if (overGPlane()) transform.Find("WingHitbox").GetComponent<DamageModel>().kill();
        if (overGPilotToDeath()) transform.Find("CockpitHitbox").GetComponent<DamageModel>().kill();
    }

    void FixedUpdate() {
        calculateGs();
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

        if (currentForces.magnitude != 0) currentGs = ((currentForces / Physics2D.gravity.magnitude) + (transform.localScale.y * Vector3.up));
        feltGs = Vector3.Project(currentGs, transform.up).y / transform.up.y;

        prevVel = GetComponent<Rigidbody2D>().velocity;
    }

    public bool overGPlaneToDeath() {
        return Mathf.Abs(currentGs.magnitude) > Mathf.Abs(planeDestroyingGs);
    }

    public bool overGPlane() {
        return Mathf.Abs(currentGs.magnitude) > Mathf.Abs(planeStructDestroyingGs);
    }

    public bool overGPilotToDeath() {
        return Mathf.Abs(feltGs) > Mathf.Abs(killingGs);
    }

    public bool overGPilot() {
        return Mathf.Abs(feltGs) > Mathf.Abs(sleepyGs);
    }
}
