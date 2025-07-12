using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {
    [Header("Base Stats")]
    [SerializeField] private float initSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float explosionRad;
    [SerializeField] private float penetrationVal;
    

    void OnCollisionEnter2D(Collision2D col) {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, explosionRad == 0 ? transform.localScale.x : explosionRad, col.relativeVelocity, penetrationVal);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.transform.GetComponent<DamageModel>() != null) {
                hit.collider.transform.GetComponent<DamageModel>().hit(Random.Range(damage / 2f, damage));
            }
        }
        Destroy(gameObject);
    }

    void Start() {
        
    }

    void Update() {
        
    }

    public float getInitSpeed() {
        return initSpeed;
    }
}
