using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {
    [Header("Base Stats")]
    [SerializeField] private float initSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float explosionRad;
    

    void OnCollisionEnter2D() {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, Mathf.Max(explosionRad, 0.5f));
        foreach (Collider2D hit in hits) {
            if (hit.transform.GetComponent<DamageModel>() != null) {
                hit.transform.GetComponent<DamageModel>().hit(Random.Range(damage / 2f, damage));
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
