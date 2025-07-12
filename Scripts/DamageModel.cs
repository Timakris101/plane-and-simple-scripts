using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageModel : MonoBehaviour {

    [SerializeField] private float hitChance;
    [SerializeField] private string[] hitEffects;
    [SerializeField] private float maxHealth;
    [SerializeField] private float health;
    [SerializeField] private bool crewRole;

    [Header("Tail")]
    [SerializeField] private GameObject tail;

    [Header("Wing")]
    [SerializeField] private float ripSpeed;
    [SerializeField] private GameObject wing;
    [SerializeField] private float animatorSpeedFactor;

    private Aerodynamics aero;

    public bool isCrewRole() {
        return crewRole;
    }

    void Start() {
        health = maxHealth;
        aero = transform.parent.GetComponent<Aerodynamics>();
    }

    void Update() {
        if (health > maxHealth) health = maxHealth;
        if (health <= 0) {
            foreach (string effect in hitEffects) {
                if (effect == "wings") {
                    transform.parent.GetComponent<Animator>().speed = transform.parent.GetComponent<Rigidbody2D>().velocity.magnitude / animatorSpeedFactor;
                }
                if (effect == "pilot" && transform.parent.GetComponent<Aerodynamics>() != null) { //if pilothitbox is on plane and not on crew
                    if (transform.parent.Find("Camera") != null) transform.parent.Find("Camera").parent = null;
                }
                if (effect == "tail") {
                    aero.setSpeedOfControlEff(Mathf.Infinity);
                    if (transform.parent.GetComponent<Rigidbody2D>().angularVelocity > 0) {
                        transform.parent.GetComponent<Rigidbody2D>().angularVelocity = Random.Range(.25f, .5f) * Mathf.Pow(transform.parent.GetComponent<Rigidbody2D>().velocity.magnitude, 2f);
                    } else {
                        transform.parent.GetComponent<Rigidbody2D>().angularVelocity = Random.Range(-.25f, -.5f) * Mathf.Pow(transform.parent.GetComponent<Rigidbody2D>().velocity.magnitude, 2f);
                    }
                }
            }   
        }
        foreach (string effect in hitEffects) {
            if (effect == "wings") {
                if (transform.parent.GetComponent<Rigidbody2D>().velocity.magnitude > ripSpeed) {
                    kill();
                }
            }
        }
    }

    public void setHitChance(float val) {
        hitChance = val;
    }

    public void setMaxHealth(float val) {
        maxHealth = val;
        if (health > val) health = val;
    }

    public void hit(float amt) {
        if (Random.Range(0f, 1f) < hitChance) {
            damage(amt);
        }
    }

    public void damage(float amt) {
        health -= amt;
        foreach (string effect in hitEffects) {
            if (effect == "tail") {
                aero.setBaseTorque(health <= 0 ? 0 : aero.getBaseTorque() * (1 - amt / maxHealth));
                if (health <= 0 && !transform.parent.GetComponent<Animator>().GetBool("Tailless")) {
                    GameObject obj = Instantiate(tail, transform.position, transform.rotation);
                    obj.GetComponent<Rigidbody2D>().velocity = transform.parent.GetComponent<Rigidbody2D>().velocity;
                    obj.transform.localScale = transform.parent.localScale;
                    transform.parent.GetComponent<Animator>().SetBool("Tailless", true);

                    transform.parent.GetComponent<BoxCollider2D>().size = new Vector2(transform.parent.GetComponent<BoxCollider2D>().size.x - obj.GetComponent<BoxCollider2D>().size.x, transform.parent.GetComponent<BoxCollider2D>().size.y);
                    transform.parent.GetComponent<BoxCollider2D>().offset = new Vector2(transform.parent.GetComponent<BoxCollider2D>().offset.x + obj.GetComponent<BoxCollider2D>().size.x / 2, transform.parent.GetComponent<BoxCollider2D>().offset.y);
                }
            }

            if (effect == "wings") {
                aero.setWingArea(health <= 0 ? 0 : aero.getWingArea() * (1 - amt / maxHealth));
                if (health / maxHealth < Random.Range(0f, .5f)) {
                    if (transform.parent.Find("Gear") != null) transform.parent.Find("Gear").GetComponent<GearScript>().breakGear();
                }
                if (health / maxHealth < Random.Range(0f, .75f)) {
                    if (transform.parent.Find("Flaps") != null) transform.parent.Find("Flaps").GetComponent<FlapScript>().breakFlaps();
                }
                if (health <= 0 && !transform.parent.GetComponent<Animator>().GetBool("Wingless")) {
                    GameObject obj = Instantiate(wing, transform.position, transform.rotation);
                    obj.GetComponent<Rigidbody2D>().velocity = transform.parent.GetComponent<Rigidbody2D>().velocity;
                    obj.transform.localScale = transform.parent.localScale;
                    transform.parent.GetComponent<Animator>().SetBool("Wingless", true);
                    if (transform.childCount != 0) transform.GetChild(0).parent = null;
                }
            }

            if (effect == "engine") {
                aero.setMaxThrust(health <= 0 ? 0 : aero.getMaxThrust() - amt / maxHealth);
            }
        }
    }

    public void kill() {
        damage(health);
    }

    public bool isAlive() {
        return health > 0;
    }
}
