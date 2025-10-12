using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageModel : MonoBehaviour {

    private string effect;
    [SerializeField] private float hitChance;
    [SerializeField] private float maxHealth;
    [SerializeField] private float health;
    [SerializeField] private bool crewRole;
    [SerializeField] private bool criticalSystem;
    [SerializeField] private GameObject destructiveEffect;
    [SerializeField] private Sprite replacementSprite;
    private float startingValOfEffect;
    [SerializeField] private float effectivenessFalloffRate;

    [Header("Engine")]
    [SerializeField] private float fireDamagePerSec;

    [Header("Tail")]
    [SerializeField] private GameObject tail;

    [Header("Wing")]
    [SerializeField] private float ripSpeed;
    [SerializeField] private GameObject wing;
    [SerializeField] private float animatorSpeedFactor;

    private Aerodynamics aero;
    private bool effectApplied;

    public bool isCrewRole() {
        return crewRole;
    }

    public bool isCritical() {
        return criticalSystem;
    }

    void Awake() {
        health = maxHealth;
    }

    void Start() {
        aero = transform.parent.GetComponent<Aerodynamics>();
        effect = gameObject.name.Replace("Hitbox", "");
        switch(effect) {
            case "Tail":
                startingValOfEffect = aero.getBaseTorque();
                break;

            case "Wing":
                startingValOfEffect = aero.getWingArea();
                break;

            case "Engine":
                startingValOfEffect = GetComponent<EngineScript>().getVal();
                break;
        }
    }

    void Update() {
        if (health > maxHealth) health = maxHealth;
        if (health <= 0) {
            if (destructiveEffect != null && !effectApplied) {
                effectApplied = true;
                Instantiate(destructiveEffect, transform.position, Quaternion.identity, transform);
            }
            if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().sprite = replacementSprite;
            switch (effect) {
                case "Wing":
                    transform.parent.GetComponent<Animator>().speed = Mathf.Min(transform.parent.GetComponent<Rigidbody2D>().linearVelocity.magnitude / animatorSpeedFactor, 2f);
                    aero.setBaseTorque(0);
                    break;

                case "Tail":
                    aero.setSpeedOfControlEff(Mathf.Infinity);
                    if (transform.parent.GetComponent<Rigidbody2D>().linearVelocity.magnitude > 5f) {
                        if (transform.parent.GetComponent<Rigidbody2D>().angularVelocity > 0) {
                            transform.parent.GetComponent<Rigidbody2D>().angularVelocity = Mathf.Min(5f * Mathf.Pow(transform.parent.GetComponent<Rigidbody2D>().linearVelocity.magnitude, 1f), 360f);
                        } else {
                            transform.parent.GetComponent<Rigidbody2D>().angularVelocity = Mathf.Max(-5f * Mathf.Pow(transform.parent.GetComponent<Rigidbody2D>().linearVelocity.magnitude, 1f), -360f);
                        }
                    }
                    aero.setWingArea(0f);
                    break;

                case "Engine":
                    for (int i = 0; i < transform.parent.childCount; i++) {
                        if (transform.parent.GetChild(i).GetComponent<DamageModel>() != null) {
                            transform.parent.GetChild(i).GetComponent<DamageModel>().damage(fireDamagePerSec * Time.deltaTime);
                        }
                    }
                    break;
            }   
        }

        if (effect == "Wing") {
            if (transform.parent.GetComponent<Rigidbody2D>().linearVelocity.magnitude > ripSpeed) {
                kill();
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

    public void hit(float amt, bool withChance) {
        if (Random.Range(0f, 1f) < (withChance ? hitChance : 1f)) {
            damage(amt);
        }
    }

    public void damage(float amt) {
        health -= amt;
        switch(effect) {
            case "Tail":
                if (health <= 0 && !transform.parent.GetComponent<Animator>().GetBool("Tailless")) {
                    handleSpawningTail();
                }
                break;

            case "Wing":
                if (health / maxHealth < Random.Range(0f, .5f)) {
                    if (transform.parent.Find("Gear") != null) transform.parent.Find("Gear").GetComponent<GearScript>().breakGear();
                }
                if (health / maxHealth < Random.Range(0f, .75f)) {
                    if (transform.parent.Find("Flaps") != null) transform.parent.Find("Flaps").GetComponent<FlapScript>().breakFlaps();
                }
                if (health <= 0 && !transform.parent.GetComponent<Animator>().GetBool("Wingless")) {
                    handleSpawningWing();
                }
                break;

            case "Engine":
                break;
        }

        switch(effect) {
            case "Tail":
                aero.setBaseTorque(health <= 0 ? 0 : startingValOfEffect * (1 - ((maxHealth - health) * effectivenessFalloffRate / maxHealth)));
                break;

            case "Wing":
                aero.setWingArea(health <= 0 ? 0 : startingValOfEffect * (1 - ((maxHealth - health) * effectivenessFalloffRate / maxHealth)));
                break;

            case "Engine":
                GetComponent<EngineScript>().setVal(health <= 0 ? 0 : startingValOfEffect * (1 - ((maxHealth - health) * effectivenessFalloffRate / maxHealth)));
                break;
        }
    }

    private void handleSpawningTail() {
        GameObject obj = Instantiate(tail, transform.position, transform.rotation);
        obj.GetComponent<Rigidbody2D>().linearVelocity = transform.parent.GetComponent<Rigidbody2D>().linearVelocity;
        obj.transform.localScale = transform.parent.localScale;
        transform.parent.GetComponent<Animator>().SetBool("Tailless", true);

        transform.parent.GetComponent<BoxCollider2D>().size = new Vector2(transform.parent.GetComponent<BoxCollider2D>().size.x - obj.GetComponent<BoxCollider2D>().size.x, transform.parent.GetComponent<BoxCollider2D>().size.y);
        transform.parent.GetComponent<BoxCollider2D>().offset = new Vector2(transform.parent.GetComponent<BoxCollider2D>().offset.x + obj.GetComponent<BoxCollider2D>().size.x / 2, transform.parent.GetComponent<BoxCollider2D>().offset.y);

        Destroy(obj, 10f);
    }

    private void handleSpawningWing() {
        GameObject obj = Instantiate(wing, transform.position, transform.rotation);
        obj.GetComponent<Rigidbody2D>().linearVelocity = transform.parent.GetComponent<Rigidbody2D>().linearVelocity;
        obj.transform.localScale = transform.parent.localScale;
        transform.parent.GetComponent<Animator>().SetBool("Wingless", true);
        handlePropellerOn(obj);
        
        Destroy(obj, 10f);
    }

    private void handlePropellerOn(GameObject obj) {
        if (obj.transform.childCount != 0) {
            for (int i = 0; i < transform.parent.childCount; i++) {
                GameObject possibleProp = transform.parent.GetChild(i).gameObject;
                if (possibleProp.GetComponent<PropellerScript>() != null) {
                    if (possibleProp.GetComponent<PropellerScript>().isPropOfFallenWing()) {
                        possibleProp.GetComponent<PropellerScript>().enabled = false;
                        possibleProp.GetComponent<Animator>().enabled = false;
                        possibleProp.transform.position = obj.transform.GetChild(0).position;
                        possibleProp.transform.parent = obj.transform.GetChild(0);
                    }
                }
            }
        }
    }

    public void kill() {
        damage(health);
    }

    public bool isAlive() {
        return health > 0;
    }

    public float getHealth() {
        return health;
    }

    public float getMaxHealth() {
        return maxHealth;
    }
}
