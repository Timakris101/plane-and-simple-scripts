using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerScript : MonoBehaviour {

    [SerializeField] protected bool manualControl;
    [SerializeField] protected GameObject targetedObj;
    [SerializeField] protected float angularThreshForGuns;
    [SerializeField] protected float minDeflection;
    [SerializeField] protected float maxDeflection;
    [SerializeField] protected float maxRange;

    protected virtual void Update() {
        if (transform.parent.gameObject.layer == LayerMask.NameToLayer("Vehicle") && transform.parent.Find("WingHitbox").GetComponent<DamageModel>().isAlive()) { //if in plane and plane is not spinning out
            if (GetComponent<DamageModel>().isAlive() && !transform.parent.GetComponent<GForcesScript>().overGPerson()) { //if concious and alive
                setTargetedObj(transform.parent.GetComponent<AiPlaneController>().getTargetedObj());

                if (!manualControl) {
                    bool inRange = false;
                    if (targetedObj != null) {
                        inRange = (transform.position - targetedObj.transform.position).magnitude < maxRange;
                    }
                    if (targetedObj != null && inRange) {
                        pointGunAt(positionToTarget());
                        attemptToShoot(positionToTarget(), targetInSights());
                    } else {
                        attemptToShoot(false);
                    }
                } else {
                    if (transform.parent.Find("Camera") != null) {
                        Vector3 screenToWorld = transform.parent.Find("Camera").GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -transform.parent.Find("Camera").position.z));
                        pointGunAt(new Vector3(screenToWorld.x, screenToWorld.y, 0));
                        attemptToShoot(new Vector3(screenToWorld.x, screenToWorld.y, 0), Input.GetMouseButton(0));
                    }
                }
            }
        } else {
            if (transform.childCount != 0) {
                Destroy(transform.GetChild(0).gameObject);
            }
        }
    }

    protected void attemptToShoot(Vector3 posToShoot, bool b) {
        bool tooFarFromSight = Mathf.Abs(Vector2.SignedAngle(posToShoot - transform.GetChild(0).position, transform.GetChild(0).right)) > angularThreshForGuns;
        bool hitsOwnPlane = false;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.GetChild(0).position, transform.GetChild(0).right);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.transform == transform.parent) { //checks if the specific collider is of the parent and not of the children. Hit.transform would return parent transform
                hitsOwnPlane = true;
                break;
            }
        }
        transform.GetChild(0).GetComponent<GunScript>().setShooting(b && !hitsOwnPlane && !tooFarFromSight);
    }

    protected void attemptToShoot(bool b) {
        bool hitsOwnPlane = false;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.GetChild(0).position, transform.GetChild(0).right);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.transform == transform.parent) { //checks if the specific collider is of the parent and not of the children. Hit.transform would return parent transform
                hitsOwnPlane = true;
                break;
            }
        }
        transform.GetChild(0).GetComponent<GunScript>().setShooting(b && !hitsOwnPlane);
    }

    protected void pointGunAt(Vector3 pos) {
        transform.GetChild(0).eulerAngles = new Vector3(0, 0, Mathf.Atan2((pos - transform.GetChild(0).position).y, (pos - transform.GetChild(0).position).x) * Mathf.Rad2Deg);
        transform.GetChild(0).localEulerAngles = new Vector3(0, 0, boundedGunAngle(transform.GetChild(0).localEulerAngles.z, minDeflection, maxDeflection));
    }

    protected float boundedGunAngle(float unboundedAngle, float minDeflection, float maxDeflection) {
        if (minDeflection < maxDeflection) {
            return Mathf.Clamp(unboundedAngle + (unboundedAngle < 0f ? 360f : 0f) + rotOfBase(), minDeflection + rotOfBase(), maxDeflection + rotOfBase()) - rotOfBase();
        } else {
            return (Mathf.Clamp(unboundedAngle + (unboundedAngle < maxDeflection ? 360f : 0f) + rotOfBase(), minDeflection + rotOfBase(), maxDeflection + 360f + rotOfBase()) - rotOfBase()) - 360f;
        }
    }

    private float rotOfBase() {
        return transform.eulerAngles.z - transform.localEulerAngles.z;
    }

    protected bool targetInSights() {
        GameObject bullet = transform.GetChild(0).GetComponent<GunScript>().getBullet();
        return Mathf.Abs(Vector3.SignedAngle((positionToTarget() - transform.GetChild(0).position).normalized, transform.GetChild(0).right, transform.GetChild(0).forward)) < angularThreshForGuns;
    }

    protected virtual Vector3 positionToTarget() {
        GameObject bullet = transform.GetChild(0).GetComponent<GunScript>().getBullet();
        return targetedObj.transform.position + (Vector3) (targetedObj.GetComponent<Rigidbody2D>().velocity - transform.parent.GetComponent<Rigidbody2D>().velocity) * (targetedObj.transform.position - transform.GetChild(0).position).magnitude / (bullet.GetComponent<BulletScript>().getInitSpeed());
    }

    public void setManualControl(bool b) {
        manualControl = b;
    }

    public bool getManualControl() {
        return manualControl;
    }

    public void setTargetedObj(GameObject obj) {
        targetedObj = obj;
    }
}
