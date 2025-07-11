using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour {

    private Vector3 offset;

    [SerializeField] private int minP = 10; //min for perspective
    [SerializeField] private int maxP = 90; //max for perspective

    [SerializeField] private float freeCamSpeed;

    void Start() {
        offset = transform.position - transform.parent.position;
        offset = new Vector3(0, 0, offset.z);

        Camera camera = gameObject.GetComponent<Camera>();
    }

    void Update() {
        if (transform.parent != null) {
            transform.position = transform.parent.position + offset;
        } else {
            if (Input.GetKey("w")) {
                transform.position += new Vector3(0, 1, 0) * freeCamSpeed * Time.deltaTime;
            }
            if (Input.GetKey("a")) {
                transform.position += new Vector3(-1, 0, 0) * freeCamSpeed * Time.deltaTime;
            }
            if (Input.GetKey("s")) {
                transform.position += new Vector3(0, -1, 0) * freeCamSpeed * Time.deltaTime;
            }
            if (Input.GetKey("d")) {
                transform.position += new Vector3(1, 0, 0) * freeCamSpeed * Time.deltaTime;
            }
        }
        transform.eulerAngles = new Vector3(0, 0, 0);

        Camera camera = gameObject.GetComponent<Camera>();
        
        camera.fieldOfView -= 2 * Mathf.Atan(Input.mouseScrollDelta.y); //adds mouse scroll to cam fov, tangent for smooth cam movement

        if (camera.fieldOfView > maxP) { //makes cam size unable to go above max
            camera.fieldOfView = maxP;
        }
        if (camera.fieldOfView < minP) { //makes cam size unable to go below min
            camera.fieldOfView = minP;
        }
    }
}
