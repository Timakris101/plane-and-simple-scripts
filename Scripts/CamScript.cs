using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour {

    private Vector3 offset;

    [SerializeField] private int minP; //min for perspective
    [SerializeField] private int maxP; //max for perspective

    [SerializeField] private float freeCamSpeedScaler;

    void Start() {
        if (transform.parent != null) {
            offset = transform.position - transform.parent.position;
            offset = new Vector3(0, 0, offset.z);
        }

        Camera camera = gameObject.GetComponent<Camera>();
    }

    void Update() {
        Camera camera = gameObject.GetComponent<Camera>();

        if (transform.parent != null) {
            transform.position = transform.parent.position + offset;
        } else {
            Vector3 movementVec = new Vector3(0, 0, 0);
            if (Input.GetKey("w")) {
                movementVec += new Vector3(0, 1, 0);
            }
            if (Input.GetKey("a")) {
                movementVec += new Vector3(-1, 0, 0);
            }
            if (Input.GetKey("s")) {
                movementVec += new Vector3(0, -1, 0);
            }
            if (Input.GetKey("d")) {
                movementVec += new Vector3(1, 0, 0);
            }
            transform.position += movementVec * freeCamSpeedScaler * Mathf.Tan(camera.fieldOfView / 2f / 180f * 3.14f) * Time.deltaTime;
        }
        transform.eulerAngles = new Vector3(0, 0, 0);
        
        camera.fieldOfView -= 2 * Mathf.Atan(Input.mouseScrollDelta.y); //adds mouse scroll to cam fov, tangent for smooth cam movement

        if (camera.fieldOfView > maxP) { //makes cam size unable to go above max
            camera.fieldOfView = maxP;
        }
        if (camera.fieldOfView < minP) { //makes cam size unable to go below min
            camera.fieldOfView = minP;
        }
    }
}
