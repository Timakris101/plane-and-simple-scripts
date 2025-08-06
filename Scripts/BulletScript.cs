using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {
    [Header("Base Stats")]
    [SerializeField] private float initSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float explosionRad;
    [SerializeField] private float penetrationVal;
    
    [Header("Plane")]
    [SerializeField] private GameObject planeFired;

    void OnCollisionEnter2D(Collision2D col) {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, explosionRad == 0 ? transform.localScale.x : explosionRad, -col.relativeVelocity, penetrationVal);
        foreach (RaycastHit2D hit in hits) {
            if (initSpeed != 0 && hit.transform.gameObject != col.gameObject) continue;
            if (hit.collider.transform.GetComponent<DamageModel>() != null) {
                hit.collider.transform.GetComponent<DamageModel>().hit(Random.Range(damage / 2f, damage));
            }
        }
        Destroy(gameObject);
    }

    public void setPlaneFired(GameObject plane) {
        planeFired = plane;
    }

    void Update() {
        Physics2D.IgnoreCollision(planeFired.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        for (int i = 0; i < planeFired.transform.childCount; i++) {
            if (planeFired.transform.GetChild(i).GetComponent<Collider2D>() != null) Physics2D.IgnoreCollision(planeFired.transform.GetChild(i).GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
        }

        transform.right = GetComponent<Rigidbody2D>().velocity.normalized;
    }

    public float getInitSpeed() {
        return initSpeed;
    }
}
