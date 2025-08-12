using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AAGunnerScript : GunnerScript {

    [SerializeField] private string alliance;

    protected override void Update() {
        if (GetComponent<DamageModel>().isAlive()) {
            findTarget();
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
    }

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
}
