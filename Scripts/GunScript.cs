using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunScript : MonoBehaviour {

    [SerializeField] protected GameObject bullet;
    [SerializeField] private float fireRate;
    private float timer;
    [SerializeField] private float maxAmmunition;
    [SerializeField] protected float ammunition;
    private bool shooting;

    protected void Start() {
        ammunition = maxAmmunition;
    }

    protected virtual void shoot() {
        ammunition--;
        GameObject newBullet = Instantiate(bullet, transform.position, transform.rotation);
        Vector3 baseVel = transform.parent.GetComponent<Rigidbody2D>() == null ? transform.parent.parent.GetComponent<Rigidbody2D>().velocity : transform.parent.GetComponent<Rigidbody2D>().velocity; //looks up hierarchy by 2
        newBullet.GetComponent<Rigidbody2D>().velocity = newBullet.GetComponent<BulletScript>().getInitSpeed() * transform.right + baseVel;
        newBullet.GetComponent<BulletScript>().setPlaneFired(transform.parent.GetComponent<Aerodynamics>() == null ? transform.parent.parent.gameObject : transform.parent.gameObject);
    }

    protected void Update() {
        timer += Time.deltaTime;
        if (timer > fireRate && shooting && ammunition > 0) {
            timer = 0;
            shoot();
        }
    }

    public void setShooting(bool b) {
        shooting = b;
    }

    public GameObject getBullet() {
        return bullet;
    }
}
