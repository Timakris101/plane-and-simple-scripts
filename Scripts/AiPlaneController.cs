using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlaneController : PlaneController {

    [SerializeField] private GameObject targetedObj;
    [SerializeField] private float angularThreshForGuns;
    [SerializeField] private float sixAngle;
    [SerializeField] private string mode;
    [SerializeField] private float minAltitude;
    [SerializeField] private float minSpeed;
    [SerializeField] private float gunRange;
    private GameObject primaryBullet;

    new void Start() {
        base.Start();
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunScript>() != null) {
                primaryBullet = transform.GetChild(i).GetComponent<GunScript>().getBullet();
            }
        }
        mode = "pursuit";
    }

    protected override int wantedDir() {
        if (angleTo(targetedObj.transform.position) > 180f - sixAngle || angleTo(targetedObj.transform.position) < -(180f - sixAngle)) {
            mode = "defensive";
        } else {
            mode = "pursuit";
        }

        if ((mode == "defensive" || mode == "overshoot") && Vector3.Project(targetedObj.transform.position - transform.position, Vector3.up).y < 0 && GetComponent<Rigidbody2D>().velocity.magnitude > targetedObj.GetComponent<Rigidbody2D>().velocity.magnitude) mode = "hammerhead";

        if ((mode == "defensive" || mode == "hammerhead") && GetComponent<Rigidbody2D>().velocity.magnitude < targetedObj.GetComponent<Rigidbody2D>().velocity.magnitude) mode = "overshoot";

        if (transform.position.y < minAltitude) return pointTowards(transform.position + Vector3.up);

        if (targetedObj == null || targetedObj.GetComponent<Rigidbody2D>().velocity.magnitude < 1f) return pointTowards(transform.position + Vector3.Project(transform.right, Vector3.right));

        if (mode == "pursuit" || mode == "overshoot" || mode == "defensive") return pointTowards(positionToTarget(primaryBullet));

        if (mode == "hammerhead") {
            if (GetComponent<Rigidbody2D>().velocity.magnitude < 1f) {
                mode = "pursuit";
            } else {
                return pointTowards(transform.position + Vector3.up);
            }
        }

        return 0;
    }

    private int pointTowards(Vector3 pos) {
        return  angleTo(pos) > 0 ? -1 : 1;
    }

    private float angleTo(Vector3 pos) {
        return Vector3.SignedAngle((pos - transform.position).normalized, transform.right, transform.forward);
    }

    protected override void handleControls() {
        throttle = 1f;
        if (mode == "overshoot") throttle = 0f;
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunScript>() != null) {
                if (targetInSights(transform.GetChild(i).GetComponent<GunScript>().getBullet()) && (transform.position - positionToTarget(transform.GetChild(i).GetComponent<GunScript>().getBullet())).magnitude < gunRange) {
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
