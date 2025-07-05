using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlaneController : PlaneController {

    [SerializeField] private GameObject targetedObj;
    [SerializeField] private float angularThreshForGuns;
    [SerializeField] private string mode;
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
        mode = "pursuit";
        if ((positionToTarget(primaryBullet) - transform.position).magnitude / (GetComponent<Rigidbody2D>().velocity - targetedObj.GetComponent<Rigidbody2D>().velocity).magnitude < 1f) mode = "abort";
        if (transform.position.y < 100f) return Mathf.Abs(Vector3.SignedAngle(Vector3.up, transform.right, transform.forward)) < 1f ? 0 : 1;
        if (mode == "pursuit") return Vector3.SignedAngle((positionToTarget(primaryBullet) - transform.position).normalized, transform.right, transform.forward) > 0 ? -1 : 1;
        if (mode == "abort") return 1;
        return 0;
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
