using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerScript : MonoBehaviour {

    [SerializeField] private bool manualControl;
    [SerializeField] private GameObject targetedObj;
    [SerializeField] private float angularThreshForGuns;
    [SerializeField] private float minDeflection;
    [SerializeField] private float maxDeflection;

    void Update() {
        if (transform.parent.gameObject.layer == LayerMask.NameToLayer("Vehicle")) { //if in plane
            if (GetComponent<DamageModel>().isAlive()) {
                setTargetedObj(transform.parent.GetComponent<AiPlaneController>().getTargetedObj());

                if (!manualControl) {
                    if (targetedObj != null) {
                        pointGunAt(positionToTarget());
                        attemptToShoot(targetInSights());
                    } else {
                        attemptToShoot(false);
                    }
                } else {
                    Vector3 screenToWorld = transform.parent.Find("Camera").GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -transform.parent.Find("Camera").position.z));
                    pointGunAt(new Vector3(screenToWorld.x, screenToWorld.y, 0));
                    attemptToShoot(Input.GetMouseButton(0));
                }
            }
        } else {
            if (transform.childCount != 0) {
                Destroy(transform.GetChild(0).gameObject);
            }
        }
    }

    private void attemptToShoot(bool b) {
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

    private void pointGunAt(Vector3 pos) {
        transform.GetChild(0).right = (pos - transform.GetChild(0).position).normalized;
        transform.GetChild(0).localEulerAngles = new Vector3(0, 0, Mathf.Clamp(transform.GetChild(0).localEulerAngles.z, minDeflection, maxDeflection));
    }

    private bool targetInSights() {
        GameObject bullet = transform.GetChild(0).GetComponent<GunScript>().getBullet();
        return Mathf.Abs(Vector3.SignedAngle((positionToTarget() - transform.GetChild(0).position).normalized, transform.GetChild(0).right, transform.GetChild(0).forward)) < angularThreshForGuns;
    }

    private Vector3 positionToTarget() {
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
