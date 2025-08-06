using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewScript : MonoBehaviour {

    [SerializeField] private float chuteSpeed;
    [SerializeField] private float chuteEff;
    private bool onGround;

    void OnCollisionEnter2D(Collision2D col) {
        if (col.transform.tag == "Ground") {
            for (int i = 0; i < transform.childCount; i++) {
                if (transform.GetChild(i).GetComponent<Animator>() == null) continue;
                transform.GetChild(i).GetComponent<Animator>().SetBool("OnGround", true);
            }
            onGround = true;
        }
    }

    void OnCollisionStay2D(Collision2D col) {
        if (col.transform.tag == "Ground" && !GetComponent<Animator>().GetBool("Dead")) {
            transform.up = col.contacts[0].normal;
            GetComponent<Rigidbody2D>().drag = 100000f;
        }
        if (GetComponent<Animator>().GetBool("Dead") && onGround && Mathf.Abs(Vector3.Angle(col.contacts[0].normal, transform.up)) < 45f) {
            transform.localEulerAngles = new Vector3(0, 0, 90f);
        }
    }

    void Start() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;
            transform.GetChild(i).GetComponent<DamageModel>().setHitChance(1f);
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
