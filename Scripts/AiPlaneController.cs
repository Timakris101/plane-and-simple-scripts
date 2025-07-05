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
    }

    protected override int wantedDir() {
        mode = "pursuit";
        
        if (angleTo(targetedObj.transform.position) > sixAngle || angleTo(targetedObj.transform.position) < -sixAngle) mode = "defensive";

        if ((positionToTarget(primaryBullet) - transform.position).magnitude / (GetComponent<Rigidbody2D>().velocity - targetedObj.GetComponent<Rigidbody2D>().velocity).magnitude < 1f) mode = "abort";

        if (transform.position.y < minAltitude) return pointTowards(transform.position + Vector3.up);

        if (GetComponent<Rigidbody2D>().velocity.magnitude < minSpeed) return pointTowards(transform.position + Vector3.Project(transform.right, Vector3.right));

        if (targetedObj == null || targetedObj.GetComponent<Rigidbody2D>().velocity.magnitude < 1f) return pointTowards(transform.position + Vector3.Project(transform.right, Vector3.right));

        if (mode == "pursuit" || mode == "defensive") return pointTowards(positionToTarget(primaryBullet));

        if (mode == "abort") return pointTowards(transform.position * 2 - positionToTarget(primaryBullet));

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
        if (mode == "defensive") throttle = 0;

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
