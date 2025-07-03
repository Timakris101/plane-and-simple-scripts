using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunScript : MonoBehaviour {

    [SerializeField] private GameObject bullet;
    [SerializeField] private float fireRate;
    private float timer;
    [SerializeField] private float maxAmmunition;
    [SerializeField] private float ammunition;

    void Start() {
        ammunition = maxAmmunition;
    }

    private void shoot() {
        ammunition--;
        GameObject newBullet = Instantiate(bullet, transform.position, Quaternion.identity);
        newBullet.GetComponent<Rigidbody2D>().velocity = (newBullet.GetComponent<BulletScript>().getInitSpeed() + transform.parent.GetComponent<Rigidbody2D>().velocity.magnitude) * transform.parent.right;
    }

    void Update() {
        timer += Time.deltaTime;
        if (timer > fireRate && Input.GetMouseButton(0)) {
            timer = 0;
            shoot();
        }
    }
}
