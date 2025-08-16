using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {
    private static float explosiveRangeOfCertainHit = 5f;

    [Header("Base Stats")]
    [SerializeField] private float initSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float explosionRad;
    [SerializeField] private float penetrationVal;
    [SerializeField] private float maxFlyPastDist;
    [SerializeField] private float armingDist;
    [SerializeField] private float fuseTimeSec;
    [SerializeField] private GameObject explosiveEffect;
    private float timer;
    
    [Header("Plane")]
    [SerializeField] private GameObject planeFired;

    void OnCollisionEnter2D(Collision2D col) {
        dealDamage(col);
    }

    void dealDamage(Collision2D col) {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position - (Vector3) col.relativeVelocity.normalized * Random.Range(0f, maxFlyPastDist), explosionRad == 0 ? transform.localScale.x : explosionRad, -col.relativeVelocity, penetrationVal);
        foreach (RaycastHit2D hit in hits) {
            if (initSpeed != 0 && hit.transform.gameObject != col.gameObject) continue;
            if (hit.collider.transform.GetComponent<DamageModel>() != null) {
                hit.collider.transform.GetComponent<DamageModel>().hit(Random.Range(damage / 2f, damage), explosionRad < explosiveRangeOfCertainHit);
            }
        }
        makeEffectAndDestroyObj();
    }

    void dealDamage() {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position + (Vector3) GetComponent<Rigidbody2D>().velocity.normalized * Random.Range(0f, maxFlyPastDist), explosionRad == 0 ? transform.localScale.x : explosionRad, GetComponent<Rigidbody2D>().velocity.normalized, penetrationVal);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.transform.GetComponent<DamageModel>() != null) {
                hit.collider.transform.GetComponent<DamageModel>().hit(Random.Range(damage / 2f, damage), explosionRad < explosiveRangeOfCertainHit);
            }
        }
        makeEffectAndDestroyObj();
    }

    private void makeEffectAndDestroyObj() {
        GameObject effect = Instantiate(explosiveEffect, transform.position, transform.rotation);
        var mainModule = effect.GetComponent<ParticleSystem>().main;
        if (mainModule.startSpeed.constantMax == 0) mainModule.startSpeed = new ParticleSystem.MinMaxCurve(0, explosionRad / mainModule.startLifetime.constant);
        Destroy(gameObject);
    }

    public void setPlaneFired(GameObject plane) {
        planeFired = plane;
    }

    void Start() {
        collisionToPlaneFired(false);
    }

    public void setFuseTime(float sec) {
        fuseTimeSec = sec;
    }

    void collisionToPlaneFired(bool collide) {
        Physics2D.IgnoreCollision(planeFired.GetComponent<Collider2D>(), GetComponent<Collider2D>(), !collide);
        for (int i = 0; i < planeFired.transform.childCount; i++) {
            if (planeFired.transform.GetChild(i).GetComponent<Collider2D>() != null) Physics2D.IgnoreCollision(planeFired.transform.GetChild(i).GetComponent<Collider2D>(), GetComponent<Collider2D>(), !collide);
        }
    }

    void Update() {
        if ((transform.position - planeFired.transform.position).magnitude > armingDist) {
            collisionToPlaneFired(true);
        }

        timer += Time.deltaTime;
        if (fuseTimeSec > 0 && timer > fuseTimeSec) dealDamage();

        transform.right = GetComponent<Rigidbody2D>().velocity.normalized;
    }

    public float getInitSpeed() {
        return initSpeed;
    }
}
