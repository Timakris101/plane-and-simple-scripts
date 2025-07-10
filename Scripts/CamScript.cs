using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour {

    private Vector3 offset;

    [SerializeField] private int minP = 10; //min for perspective
    [SerializeField] private int maxP = 90; //max for perspective

    [SerializeField] private GameObject otherObjToKeepInFrame;

    [SerializeField] private bool spectator;

    void Start() {
        offset = transform.position - transform.parent.position;
        offset = new Vector3(0, 0, offset.z);

        Camera camera = gameObject.GetComponent<Camera>();
    }

    void Update() {
        transform.position = transform.parent.position + offset;
        transform.eulerAngles = new Vector3(0, 0, 0);

        Camera camera = gameObject.GetComponent<Camera>();
        
        if (otherObjToKeepInFrame != null && spectator) {
            camera.fieldOfView = 2 * Mathf.Atan((transform.position - otherObjToKeepInFrame.transform.position).magnitude / offset.magnitude) * 180f / 3.14f;
        } else {
            camera.fieldOfView -= 2 * Mathf.Atan(Input.mouseScrollDelta.y); //adds mouse scroll to cam fov, tangent for smooth cam movement
        }
        if (camera.fieldOfView > maxP) { //makes cam size unable to go above max
            camera.fieldOfView = maxP;
        }
        if (camera.fieldOfView < minP) { //makes cam size unable to go below min
            camera.fieldOfView = minP;
        }
    }
}
