using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewScript : MonoBehaviour {

    [SerializeField] private float chuteSpeed;
    [SerializeField] private float chuteEff;
    [SerializeField] private float maxHealth;
    private bool onGround;

    void OnCollisionEnter2D(Collision2D col) {
        if (col.transform.tag == "Ground") {
            for (int i = 0; i < transform.childCount; i++) {
                if (transform.GetChild(i).GetComponent<Animator>() == null) continue;
                transform.GetChild(i).GetComponent<Animator>().SetBool("OnGround", true);
            }
            if (GetComponent<Animator>().GetBool("Dead") && !onGround) {
                transform.localEulerAngles += new Vector3(0, 0, 45f);
                GetComponent<Rigidbody2D>().angularVelocity = 1f;
            }
            onGround = true;
        }
    }

    void Start() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;
            transform.GetChild(i).GetComponent<DamageModel>().setMaxHealth(maxHealth);
        }
    }

    void Update() {
        handleHealth();
        if (!onGround) {
            handleSpeed();
            handleDir();
        }
    }

    void handleSpeed() {
        if (GetComponent<Rigidbody2D>().velocity.magnitude > chuteSpeed) {
            GetComponent<Rigidbody2D>().velocity -= GetComponent<Rigidbody2D>().velocity.normalized * (Time.deltaTime * chuteEff * GetComponent<Rigidbody2D>().velocity.magnitude);
        }
    }

    void handleDir() {
        GetComponent<Rigidbody2D>().angularVelocity = 0f;
        if (GetComponent<Rigidbody2D>().velocity.magnitude > 1f) transform.up = -GetComponent<Rigidbody2D>().velocity.normalized;
    }

    void handleHealth() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;
            
            if (!transform.GetChild(i).GetComponent<DamageModel>().isAlive()) {
                GetComponent<Animator>().SetBool("Dead", true);
            }
        }
    }
}
