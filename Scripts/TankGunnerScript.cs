using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankGunnerScript : GunnerScript {

    [SerializeField] private bool rotatableTurret;

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
        } else {
            attemptToShoot(false);
        }
    }

    protected override void pointGunAt(Vector3 pos) {
        bool posToTheFrontSide = Vector3.Project(pos - transform.position, transform.right).x / transform.right.x > 0f;
        
        Vector3 reflectedPos = new Vector3(2 * transform.position.x - pos.x, pos.y, 0);

        Transform gunPivot = transform.GetChild(0).Find("GunPivotPoint");
        Vector3 gunPivotPrevPos = gunPivot.position;

        float yAnglePrev =  transform.GetChild(0).localEulerAngles.y;

        if (posToTheFrontSide || !rotatableTurret) {
            transform.GetChild(0).eulerAngles = new Vector3(0, 0, Mathf.Atan2((pos - transform.GetChild(0).position).y, (pos - transform.GetChild(0).position).x) * Mathf.Rad2Deg);
        } else if (!rotatableTurret) {
            transform.GetChild(0).eulerAngles = new Vector3(0, 0, Mathf.Atan2((reflectedPos - transform.GetChild(0).position).y, (reflectedPos - transform.GetChild(0).position).x) * Mathf.Rad2Deg);
        }
        transform.GetChild(0).localEulerAngles = new Vector3(0, (posToTheFrontSide || !rotatableTurret ? 0 : 180f), boundedGunAngle(transform.GetChild(0).localEulerAngles.z, minDeflection, maxDeflection));

        Vector3 newGunPivotPos = gunPivot.position;

        float newYAngle = transform.GetChild(0).localEulerAngles.y;

        if (yAnglePrev == newYAngle) transform.GetChild(0).position += gunPivotPrevPos - newGunPivotPos;
    }
}
