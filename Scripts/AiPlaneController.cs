using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlaneController : PlaneController {

    [SerializeField] private GameObject targetedObj;
    [SerializeField] private float angularThreshForGuns;

    public override int getWantedDir() {
        return 0;
    }

    public override void handleControls() {
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
        return Mathf.Abs(Vector2.Angle((positionToTarget(bullet) - transform.position).normalized, transform.right)) < angularThreshForGuns;
    }

    private Vector3 positionToTarget(GameObject bullet) {
        return targetedObj.transform.position + (Vector3) targetedObj.GetComponent<Rigidbody2D>().velocity * (targetedObj.transform.position / (bullet.GetComponent<BulletScript>().getInitSpeed() + GetComponent<Rigidbody2D>().velocity.magnitude)).magnitude;
    }
}
