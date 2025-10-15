using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlaneController : PlaneController {

    [SerializeField] private GameObject targetedObj;
    [SerializeField] private float angularThreshForGuns;
    [SerializeField] private float sixAngle;
    [SerializeField] private string mode;
    [SerializeField] private float minAltitude;
    [SerializeField] private float gunRange;
    private GameObject primaryBullet;

    private void findTarget() {
        GameObject[] allPlanes = GameObject.FindGameObjectsWithTag("Plane");
        targetedObj = null;
        foreach (GameObject plane in allPlanes) {
            if (plane == gameObject) continue;
            
            if (plane.GetComponent<AllianceHolder>().getAlliance() != GetComponent<AllianceHolder>().getAlliance() && !plane.GetComponent<PlaneController>().vehicleDead()) {
                if (targetedObj == null) targetedObj = plane;

                if (Vector3.Distance(plane.transform.position, transform.position) < Vector3.Distance(targetedObj.transform.position, transform.position)) {
                    targetedObj = plane;
                }
            }
        }
    }


    protected override float wantedDir() {
        findTarget();
        
        primaryBullet = null;
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunScript>() != null && transform.GetChild(i).GetComponent<BombHolderScript>() == null) {
                primaryBullet = transform.GetChild(i).GetComponent<GunScript>().getBullet();
            }
        }

        if (primaryBullet == null) return pointTowards(transform.position + Vector3.Project(transform.right, Vector3.right));
        
        if (transform.position.y < minAltitude) return pointTowards(transform.position + Vector3.up);

        if (targetedObj == null || targetedObj.GetComponent<Rigidbody2D>().linearVelocity.magnitude < 1f) return pointTowards(transform.position + Vector3.Project(transform.right, Vector3.right));

        if (Mathf.Abs(angleTo(targetedObj.transform.position)) > 180f - sixAngle && Mathf.Abs(Vector2.SignedAngle(targetedObj.transform.right, transform.right)) < 90f) {
            mode = "defensive";
        } else {
            mode = "pursuit";
        }

        if (Mathf.Abs(Vector2.SignedAngle(targetedObj.transform.right, transform.right)) > 135f && Mathf.Abs(angleTo(targetedObj.transform.position)) < sixAngle) {
            mode = "headon";
        }

        if (mode == "defensive" && Vector3.Project(targetedObj.transform.position - transform.position, Vector3.up).y < 0 && GetComponent<Rigidbody2D>().linearVelocity.magnitude > targetedObj.GetComponent<Rigidbody2D>().linearVelocity.magnitude) mode = "hammerhead";

        if ((mode == "defensive" || mode == "hammerhead") && GetComponent<Rigidbody2D>().linearVelocity.magnitude < targetedObj.GetComponent<Rigidbody2D>().linearVelocity.magnitude) mode = "overshoot";

        if (mode == "hammerhead") {
            if (targetedObj.GetComponent<Rigidbody2D>().linearVelocity.magnitude < 15f || GetComponent<Rigidbody2D>().linearVelocity.magnitude < 10f || Mathf.Abs(Vector2.SignedAngle(targetedObj.transform.right, transform.right)) > 45f || targetedObj.GetComponent<Rigidbody2D>().linearVelocity.magnitude > GetComponent<Rigidbody2D>().linearVelocity.magnitude) {
                mode = "pursuit";
            } else {
                return pointTowards(transform.position + Vector3.up);
            }
        }

        if (mode == "pursuit" || mode == "overshoot" || mode == "defensive" || mode == "headon") return pointTowards(positionToTarget(primaryBullet, transform.right));

        return 0;
    }

    private float pointTowards(Vector3 pos) {
        return Mathf.Clamp(-angleTo(pos), -1, 1);
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
        if (transform.Find("PilotHitbox") == null) return; //already bailed out
        if (criticalSystemDestroyed || GetComponent<Rigidbody2D>().linearVelocity.magnitude <= .1f || !transform.Find("PilotHitbox").GetComponent<DamageModel>().isAlive()) GetComponent<BailoutHandler>().callBailOut();
    }

    protected override void handleControls() {
        findTarget();
        setThrottle(1f);
        if (mode == "overshoot") setThrottle(0f);
        if (targetedObj != null && primaryBullet != null) {
            if (targetInSights(primaryBullet) && (transform.position - positionToTarget(primaryBullet, transform.right)).magnitude < gunRange && mode != "headon") {
                setGuns(true);
            } else {
                setGuns(false);
            }
        } else {
            setGuns(false);
        }
    }

    private bool targetInSights(GameObject bullet) {
        return Mathf.Abs(Vector3.SignedAngle((positionToTarget(bullet, transform.right) - transform.position).normalized, transform.right, transform.forward)) < angularThreshForGuns;
    }

    private Vector3 positionToTarget(GameObject bullet, Vector3 gunDir) {
        return targetedObj.transform.position + (Vector3) (targetedObj.GetComponent<Rigidbody2D>().linearVelocity) * (targetedObj.transform.position - transform.position).magnitude / (bullet.GetComponent<BulletScript>().getInitSpeed() * gunDir + (Vector3) GetComponent<Rigidbody2D>().linearVelocity).magnitude;
    }

    public GameObject getTargetedObj() {
        findTarget();
        return targetedObj;
    }
}
