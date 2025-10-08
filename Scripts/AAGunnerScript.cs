using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AAGunnerScript : GunnerScript {

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
                    transform.GetChild(0).GetComponent<GunScript>().setFuseOfBullets(positionToTarget());
                } else {
                    attemptToShoot(false);
                }
            } else {
                if (transform.parent.Find("Camera") != null) {
                    Vector3 screenToWorld = transform.parent.Find("Camera").GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -transform.parent.Find("Camera").position.z));
                    pointGunAt(new Vector3(screenToWorld.x, screenToWorld.y, 0));
                    attemptToShoot(new Vector3(screenToWorld.x, screenToWorld.y, 0), Input.GetMouseButton(0));
                    transform.GetChild(0).GetComponent<GunScript>().setFuseOfBullets(new Vector3(screenToWorld.x, screenToWorld.y, 0));
                }
            }
        } else {
            attemptToShoot(false);
        }
    }
}
