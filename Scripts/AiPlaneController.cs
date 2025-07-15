using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlaneController : PlaneController {

    private GameObject targetedObj;
    [SerializeField] private float angularThreshForGuns;
    [SerializeField] private float sixAngle;
    [SerializeField] private string mode;
    [SerializeField] private float minAltitude;
    [SerializeField] private float gunRange;
    private GameObject primaryBullet;

    [Header("Alliance")]
    [SerializeField] private string alliance;

    private void findTarget() {
        GameObject[] allPlanes = GameObject.FindGameObjectsWithTag("Plane");
        targetedObj = null;
        foreach (GameObject plane in allPlanes) {
            if (plane == gameObject) continue;
            
            if (plane.GetComponent<AiPlaneController>().getAlliance() != alliance && !plane.GetComponent<PlaneController>().planeDead()) {
                if (targetedObj == null) targetedObj = plane;

                if (Vector3.Distance(plane.transform.position, transform.position) < Vector3.Distance(targetedObj.transform.position, transform.position)) {
                    targetedObj = plane;
                }
            }
        }
    }

    public string getAlliance() {
        return alliance;
    }

    protected override int wantedDir() {
        findTarget();
        
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunScript>() != null) {
                primaryBullet = transform.GetChild(i).GetComponent<GunScript>().getBullet();
            }
        }

        if (targetedObj == null || targetedObj.GetComponent<Rigidbody2D>().velocity.magnitude < 1f) return pointTowards(transform.position + Vector3.Project(transform.right, Vector3.right));

        if ((angleTo(targetedObj.transform.position) > 180f - sixAngle || angleTo(targetedObj.transform.position) < -(180f - sixAngle)) && targetedObj.transform.right.x / transform.right.x > 0) {
            mode = "defensive";
        } else {
            mode = "pursuit";
        }

        if (targetedObj.transform.right.x / transform.right.x < 0 && angleTo(targetedObj.transform.position) < angularThreshForGuns) {
            mode = "headon";
        }

        if (mode == "defensive" && Vector3.Project(targetedObj.transform.position - transform.position, Vector3.up).y < 0 && GetComponent<Rigidbody2D>().velocity.magnitude > targetedObj.GetComponent<Rigidbody2D>().velocity.magnitude) mode = "hammerhead";

        if ((mode == "defensive" || mode == "hammerhead") && GetComponent<Rigidbody2D>().velocity.magnitude < targetedObj.GetComponent<Rigidbody2D>().velocity.magnitude) mode = "overshoot";

        if (transform.position.y < minAltitude) return pointTowards(transform.position + Vector3.up);

        if (mode == "pursuit" || mode == "overshoot" || mode == "defensive" || mode == "headon") return pointTowards(positionToTarget(primaryBullet));

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

    protected override void handleNonPilotControls() {
        bool criticalSystemDestroyed = false;
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;

            if (!transform.GetChild(i).GetComponent<DamageModel>().isCrewRole()) {
                if (!transform.GetChild(i).GetComponent<DamageModel>().isAlive()) {
                    criticalSystemDestroyed = true;
                    break;
                }
            }
        }
        if (criticalSystemDestroyed) GetComponent<BailoutHandler>().callBailOut();
    }

    protected override void handleControls() {
        findTarget();
        throttle = 1f;
        if (mode == "overshoot") throttle = 0f;
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunScript>() != null) {
                if (targetedObj != null) {
                    if (targetInSights(transform.GetChild(i).GetComponent<GunScript>().getBullet()) && (transform.position - positionToTarget(transform.GetChild(i).GetComponent<GunScript>().getBullet())).magnitude < gunRange && mode != "headon") {
                        transform.GetChild(i).GetComponent<GunScript>().setShooting(true);
                    } else {
                        transform.GetChild(i).GetComponent<GunScript>().setShooting(false);
                    }
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
        return targetedObj.transform.position + (Vector3) (targetedObj.GetComponent<Rigidbody2D>().velocity - GetComponent<Rigidbody2D>().velocity) * (targetedObj.transform.position - transform.position).magnitude / (bullet.GetComponent<BulletScript>().getInitSpeed());
    }

    public GameObject getTargetedObj() {
        return targetedObj;
    }
}
