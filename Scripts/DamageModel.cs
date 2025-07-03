using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageModel : MonoBehaviour {

    [SerializeField] private float hitChance;
    [SerializeField] private string[] hitEffects;
    [SerializeField] private float maxHealth;
    [SerializeField] private float health;

    private Aerodynamics aero;

    void Start() {
        health = maxHealth;
        aero = transform.parent.GetComponent<Aerodynamics>();
    }

    void Update() {
        
    }

    public void hit(float damage) {
        if (Random.Range(0f, 1f) < hitChance) {
            applyEffects(damage);
        }
    }

    private void applyEffects(float damage) {
        health -= damage;
        foreach (string effect in hitEffects) {
            if (effect == "tail") {
                aero.setBaseTorque(health <= 0 ? 0 : aero.getBaseTorque() * (1 - damage / maxHealth));
            }
            if (effect == "wings") {
                aero.setWingArea(health <= 0 ? 0 : aero.getWingArea() * (1 - damage / maxHealth));
                if (health / maxHealth < Random.Range(0f, .5f)) {
                    if (transform.parent.Find("Gear") != null) transform.parent.Find("Gear").GetComponent<GearScript>().breakGear();
                }
                if (health / maxHealth < Random.Range(0f, .75f)) {
                    if (transform.parent.Find("Flaps") != null) transform.parent.Find("Flaps").GetComponent<FlapScript>().breakFlaps();
                }
            }
            if (effect == "engine") {
                aero.setMaxThrust(health <= 0 ? 0 : aero.getMaxThrust() - damage / maxHealth);
            }
            if (effect == "pilot") {
                if (health == 0) {
                    aero.setBaseTorque(0);
                    aero.setAlignmentThresh(0);
                }
            }
        }
    }
}
