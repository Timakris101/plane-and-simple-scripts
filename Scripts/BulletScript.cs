using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class BulletScript : MonoBehaviour {
    private static float explosiveRangeOfCertainHit = 5f;

    [Header("Base Stats")]
    [SerializeField] private float initSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float explosionRad;
    [SerializeField] private float penetrationVal;
    [SerializeField] private float armorPenMeters;
    [SerializeField] private float maxFlyPastDist;
    [SerializeField] private float armingDist;
    [SerializeField] private float fuseTimeSec;
    [SerializeField] private GameObject effect;
    [SerializeField] private float lifeTime;
    [SerializeField] private float damageVariation;
    private float timer;
    
    [Header("Plane")]
    [SerializeField] private GameObject planeFired;

    void OnCollisionEnter2D(Collision2D col) {
        dealDamage(col);
    }

    void dealDamage(Collision2D col) {
        Vector3 beginningHitPos = transform.position - (Vector3) GetComponent<Rigidbody2D>().linearVelocity * Time.deltaTime;
        RaycastHit2D[] hits = Physics2D.RaycastAll(beginningHitPos, -col.relativeVelocity, penetrationVal);
        float newPenVal = penetrationVal;
        int armorHitCount = 0;
        bool armorHitFirst = false;
        int index = 0;
        float effectiveArmorPen = armorPenMeters * col.relativeVelocity.magnitude / initSpeed;
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.transform.GetComponent<ArmorScript>() != null) {
                float effArmorThickness = hit.collider.transform.GetComponent<BoxCollider2D>().size.x / Mathf.Cos(Mathf.Deg2Rad * Vector3.Angle(col.relativeVelocity, hit.normal));
                armorHitCount++;
                if (effArmorThickness < effectiveArmorPen) {
                    effectiveArmorPen -= effArmorThickness;
                } else {
                    if (index == 0) armorHitFirst = true;
                    effectiveArmorPen = 0f;
                    newPenVal = ((Vector3) hit.point - beginningHitPos).magnitude;
                    break;
                }
            }
            if (hit.collider.transform != transform) index++;
        }
        if (armorHitCount == 1 && effectiveArmorPen <= 0f && armorHitFirst) return;
        hits = Physics2D.CircleCastAll(beginningHitPos - (Vector3) col.relativeVelocity.normalized * Random.Range(0f, maxFlyPastDist), explosionRad == 0 ? transform.localScale.x : explosionRad, -col.relativeVelocity, newPenVal);
        foreach (RaycastHit2D hit in hits) {
            if (hit.transform.gameObject != maxAncestor(col.gameObject)) continue;
            if (hit.collider.transform.GetComponent<DamageModel>() != null) {
                hit.collider.transform.GetComponent<DamageModel>().hit(Random.Range((1f - damageVariation) * damage, (1f + damageVariation) * damage), explosionRad < explosiveRangeOfCertainHit);
            }
        }
        makeEffectAndDestroyObj(beginningHitPos);
    }

    void dealDamage() {
        Vector3 beginningHitPos = transform.position + (Vector3) GetComponent<Rigidbody2D>().linearVelocity.normalized * Random.Range(0f, maxFlyPastDist);
        RaycastHit2D[] hits = Physics2D.CircleCastAll(beginningHitPos, explosionRad == 0 ? transform.localScale.x : explosionRad, GetComponent<Rigidbody2D>().linearVelocity.normalized, penetrationVal);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.transform.GetComponent<DamageModel>() != null) {
                hit.collider.transform.GetComponent<DamageModel>().hit(Random.Range((1f - damageVariation) * damage, (1f + damageVariation) * damage), explosionRad < explosiveRangeOfCertainHit);
            }
        }
        makeEffectAndDestroyObj(beginningHitPos);
    }

    private void makeEffectAndDestroyObj(Vector3 effectPos) {
        GameObject newEffect = Instantiate(effect, effectPos, Quaternion.identity);
        var mainModule = newEffect.GetComponent<ParticleSystem>().main;
        if (mainModule.startSpeed.constantMax == 0) mainModule.startSpeed = new ParticleSystem.MinMaxCurve(0, explosionRad / mainModule.startLifetime.constant);
        Destroy(newEffect, 10f);
        Destroy(gameObject);
    }

    public void setPlaneFired(GameObject plane) {
        planeFired = plane;
    }

    void Start() {
        collisionToPlaneFired(false);
        Destroy(gameObject, lifeTime);
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
        if (planeFired == null) {
            Destroy(gameObject);
            return;
        }

        if ((transform.position - planeFired.transform.position).magnitude > armingDist) {
            collisionToPlaneFired(true);
        }

        timer += Time.deltaTime;
        if (fuseTimeSec > 0 && timer > fuseTimeSec) dealDamage();

        transform.right = GetComponent<Rigidbody2D>().linearVelocity.normalized;
    }

    public float getInitSpeed() {
        return initSpeed;
    }
}
