using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundVehicleGunnerScript : GunnerScript {

    [SerializeField] private bool rotatableTurret;
    [SerializeField] private bool ammoHasFuse;

    protected override void Start() {}

    protected override void Update() {
        if (GetComponent<DamageModel>().isAlive()) {
            setTargetedObj(transform.parent.GetComponent<AiGroundVehicleController>().getTargetedObj());
            if (!manualControl) {
                bool inRange = false;
                if (targetedObj != null) {
                    inRange = (transform.position - targetedObj.transform.position).magnitude < maxRange;
                }
                if (targetedObj != null && inRange) {
                    pointGunAt(positionToTarget());
                    attemptToShoot(positionToTarget(), targetInSights());
                    if (ammoHasFuse) transform.GetChild(0).GetComponent<GunScript>().setFuseOfBullets(positionToTarget());
                } else {
                    attemptToShoot(false);
                }
            } else {
                if (transform.parent.Find("Camera") != null) {
                    Vector3 screenToWorld = transform.parent.Find("Camera").GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -transform.parent.Find("Camera").position.z));
                    pointGunAt(new Vector3(screenToWorld.x, screenToWorld.y, 0));
                    attemptToShoot(new Vector3(screenToWorld.x, screenToWorld.y, 0), Input.GetMouseButton(0));
                    if (ammoHasFuse) transform.GetChild(0).GetComponent<GunScript>().setFuseOfBullets(new Vector3(screenToWorld.x, screenToWorld.y, 0));
                }
            }
        } else {
            attemptToShoot(false);
        }
    }

    protected override void pointGunAt(Vector3 pos) {
        bool posToTheFrontSide = Vector3.Project(pos - transform.position, transform.right).x / transform.right.x > 0f;
        
        Vector3 reflectedPos = new Vector3(2 * transform.position.x - pos.x, pos.y, 0);

        Transform gunPivot = transform.GetChild(0).Find("GunPivotPoint");
        Vector3 gunPivotPrevPos = gunPivot.position;

        if (rotatableTurret) {
            float yAnglePrev = transform.GetChild(0).localEulerAngles.y;
            transform.GetChild(0).localEulerAngles = new Vector3(0, (posToTheFrontSide ? 0 : 180f), transform.GetChild(0).localEulerAngles.z);
            float newYAngle = transform.GetChild(0).localEulerAngles.y;
            transform.Find("TurretCenter").localPosition = new Vector3(transform.Find("TurretCenter").localPosition.x, transform.GetChild(0).localPosition.y, 0f);
            if (yAnglePrev != newYAngle) {
                transform.GetChild(0).localPosition = new Vector3(2f * transform.Find("TurretCenter").localPosition.x - transform.GetChild(0).localPosition.x, transform.GetChild(0).localPosition.y, 0f);
                return;
            }

            float eulersZ = 0f;
            if (posToTheFrontSide || !rotatableTurret) {
                eulersZ = Mathf.Atan2((pos - transform.GetChild(0).position).y, (pos - transform.GetChild(0).position).x) * Mathf.Rad2Deg - (transform.GetChild(0).localEulerAngles.y == 0 ? 1f : -1f) * rotOfBase();
                if (eulersZ < 0) eulersZ += 360f;
            }
            if (!posToTheFrontSide && rotatableTurret) {
                eulersZ = Mathf.Atan2((reflectedPos - transform.GetChild(0).position).y, (reflectedPos - transform.GetChild(0).position).x) * Mathf.Rad2Deg - (transform.GetChild(0).localEulerAngles.y == 0 ? 1f : -1f) * rotOfBase();
                if (eulersZ < 0) eulersZ += 360f;
            }
            transform.GetChild(0).localEulerAngles = new Vector3(0, transform.GetChild(0).localEulerAngles.y, 0f);
            transform.GetChild(0).eulerAngles = new Vector3(0, transform.GetChild(0).eulerAngles.y, boundedGunAngle(eulersZ, minDeflection, maxDeflection) + (transform.GetChild(0).localEulerAngles.y == 0 ? 1f : -1f) * rotOfBase());
        } else {
            base.pointGunAt(pos);
        }

        Vector3 newGunPivotPos = gunPivot.position;

        transform.GetChild(0).position += gunPivotPrevPos - newGunPivotPos;
    }
}
