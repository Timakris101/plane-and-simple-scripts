using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GForcesScript : MonoBehaviour {
    [SerializeField] private float rollOverThresh;
    [SerializeField] private Vector3 currentGs;
    [SerializeField] private float feltGs;
    private Vector3 prevVel;
    [SerializeField] private float sleepyGs;
    [SerializeField] private float killingGs;
    [SerializeField] private float planeStructDestroyingGs;
    [SerializeField] private float planeDestroyingGs;

    [Header("DestructiveEffects")]
    [SerializeField] private GameObject fire;
    [SerializeField] private GameObject explosion;
    private bool destroyed = false;

    void FixedUpdate() {
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Wait")) {
            if (transform.Find("Gear") != null) transform.Find("Gear").GetComponent<GearScript>().unhideGear();
            if (transform.Find("Flaps") != null) transform.Find("Flaps").GetComponent<FlapScript>().unhideFlaps();
        }
        if (feltGs < rollOverThresh && GetComponent<Rigidbody2D>().velocity.magnitude > 1f) {
            rollover();
        }
        if (overGPlaneToDeath() && !destroyed) {
            destroyed = true;
            Instantiate(explosion, transform.position, Quaternion.identity);
            Instantiate(fire, transform, false);
            GetComponent<Aerodynamics>().setSpeedOfControlEff(Mathf.Infinity);
        }
        if (overGPlane()) {
            if (transform.Find("WingHitbox") != null) transform.Find("WingHitbox").GetComponent<DamageModel>().kill();
            if (transform.Find("TailHitbox") != null) transform.Find("TailHitbox").GetComponent<DamageModel>().kill();
        }
        if (overGPersonToDeath()) {
            for (int i = 0; i < transform.childCount; i++) {
                if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;
                if (!transform.GetChild(i).GetComponent<DamageModel>().isCrewRole()) continue;

                transform.GetChild(i).GetComponent<DamageModel>().kill();
            }
        }
        calculateGs();
    }

    private void rollover() {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * -1, transform.localScale.z);
        GetComponent<Animator>().SetTrigger("Rollover");
        if (transform.Find("Gear") != null) transform.Find("Gear").GetComponent<GearScript>().hideGear();
        if (transform.Find("Flaps") != null) transform.Find("Flaps").GetComponent<FlapScript>().hideFlaps();
    }

    private void calculateGs() {
        if (prevVel.magnitude != 0) {
            Vector3 curVel = GetComponent<Rigidbody2D>().velocity;
            Vector3 currentForces = curVel - prevVel;

            if (currentForces.magnitude != 0) currentGs = transform.localScale.y * (currentForces + Vector3.up);
            feltGs = Vector3.Dot(Vector3.Project(currentGs, transform.up), transform.up);
        }
        prevVel = GetComponent<Rigidbody2D>().velocity;
    }

    public bool overGPlaneToDeath() {
        return currentGs.magnitude > planeDestroyingGs;
    }

    public bool overGPlane() {
        return currentGs.magnitude > planeStructDestroyingGs;
    }

    public bool overGPersonToDeath() {
        return currentGs.magnitude > killingGs;
    }

    public bool overGPerson() {
        return Mathf.Abs(feltGs) > Mathf.Abs(sleepyGs);
    }
}
