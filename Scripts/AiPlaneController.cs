using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlaneController : PlaneController {

    [SerializeField] private GameObject targetedObj;
    [SerializeField] private float angularThreshForGuns;
    private GameObject primaryBullet;

    void Start() {
        base.Start();
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunScript>() != null) {
                primaryBullet = transform.GetChild(i).GetComponent<GunScript>().getBullet();
            }
        }
    }

    public override int getWantedDir() {
        return Vector3.SignedAngle((positionToTarget(primaryBullet) - transform.position).normalized, transform.right, transform.forward) > 0 ? -1 : 1;
    }

    public override void handleControls() {
        throttle = 1f;
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunScript>() != null) {
                if (targetInSights(transform.GetChild(i).GetComponent<GunScript>().getBullet())) {
                    transform.GetChild(i).GetComponent<GunScript>().setShooting(true);
                } else {
                    transform.GetChild(i).GetComponent<GunScript>().setShooting(false);
                }
            }
        }
    }

    private bool targetInSights(GameObject bullet) {
        return Mathf.Abs(Vector3.SignedAngle((positionToTarget(bullet) - transform.position).normalized, transform.right, transform.forward)) < angularThreshForGuns;
    }

    private Vector3 positionToTarget(GameObject bullet) {
        return targetedObj.transform.position + (Vector3) targetedObj.GetComponent<Rigidbody2D>().velocity * ((targetedObj.transform.position - transform.position) / (bullet.GetComponent<BulletScript>().getInitSpeed() + GetComponent<Rigidbody2D>().velocity.magnitude)).magnitude;
    }
}
