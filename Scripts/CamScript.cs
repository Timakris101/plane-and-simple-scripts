using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour {

    private Vector3 offset;
    [Header("Mode")]
    [SerializeField] private bool missionEditor;

    [Header("CamStats")]
    [SerializeField] private int minP; //min for perspective
    [SerializeField] private int maxP; //max for perspective
    [SerializeField] private float freeCamSpeedScaler;

    [Header("Plane")]
    [SerializeField] private GameObject planeToControl;
    [SerializeField] private GameObject spectatedPlane;
    [SerializeField] private string startingAlliance;

    void Start() {
        offset = new Vector3(0, 0, transform.position.z);

        if (planeToControl == null) {
            takeControlOfPlane(findNewPlane(startingAlliance));
        } else {
            takeControlOfPlane(planeToControl);
        }
        matchParentToPlane();
    }

    void Update() {
        handleArrow();
        handlePlaneSwitching();
        handleCam();
        handleCrosshair();
    }

    private void handleArrow() {
        if (transform.Find("Canvas") != null) {
            if (nearestEnemy() != null && !planeToControl.GetComponent<PlaneController>().allCrewGoneFromPlane()) {
                Vector3 screenPos = GetComponent<Camera>().WorldToScreenPoint(nearestEnemy().transform.position);
                screenPos = (new Vector3(screenPos.x / Screen.width, screenPos.y / Screen.height, 0));
                if (screenPos.x > 0 && screenPos.x < 1 && screenPos.y > 0 && screenPos.y < 1) {
                    transform.Find("Canvas").Find("ArrowHolder").GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = false;
                } else {
                    transform.Find("Canvas").Find("ArrowHolder").GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = true;
                }
                
                transform.Find("Canvas").Find("ArrowHolder").right = (nearestEnemy().transform.position - planeToControl.transform.position).normalized;
            } else {
                transform.Find("Canvas").Find("ArrowHolder").GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = false;
            }
        }
    }

    private void handleCrosshair() {
        if (transform.Find("Canvas") != null) {
            if (planeToControl != null && !planeToControl.GetComponent<PlaneController>().gunnersAreManual()) {
                transform.Find("Canvas").Find("CrosshairHolder").right = planeToControl.transform.right;
                for (int i = 0; i < transform.Find("Canvas").Find("CrosshairHolder").childCount; i++) {
                    transform.Find("Canvas").Find("CrosshairHolder").GetChild(i).GetComponent<UnityEngine.UI.Image>().enabled = true;
                    transform.Find("Canvas").Find("CrosshairHolder").GetChild(i).eulerAngles = new Vector3(0, 0, 0);
                }
            } else {
                for (int i = 0; i < transform.Find("Canvas").Find("CrosshairHolder").childCount; i++) {
                    transform.Find("Canvas").Find("CrosshairHolder").GetChild(i).GetComponent<UnityEngine.UI.Image>().enabled = false;
                }
            }
        }
    }

    private void matchParentToPlane() {
        if (planeToControl != null) {
            transform.parent = planeToControl.transform;
        } else {
            transform.parent = null;
        }
    }

    private void handlePlaneSwitching() {
        if (missionEditor) {
            if (Input.GetKeyDown("n")) {
                if (Input.GetKey(KeyCode.LeftShift)) {
                    if (planeToControl == null) {
                        scrollSpectatablePlanes();
                    } else {
                        spectatedPlane = planeToControl;
                        planeToControl = null;
                    }
                } else {
                    if (spectatedPlane == null || spectatedPlane.GetComponent<PlaneController>().allCrewGoneFromPlane()) {
                        scrollCrewedPlanes();
                    } else {
                        takeControlOfPlane(spectatedPlane);
                        spectatedPlane = null;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape)) {
                uncoupleCam();
            }
        }
        if (planeToControl != null) {
            if (planeToControl.GetComponent<PlaneController>().pilotDeadOrGone()) {
                GameObject newPlane = findNewPlane(planeToControl.GetComponent<AiPlaneController>().getAlliance());
                if (newPlane != null) {
                    takeControlOfPlane(newPlane);
                } else {
                    planeToControl = null;
                }
            }
        }
        matchParentToPlane();
    }

    public void uncoupleCam() {
        planeToControl = null;
        spectatedPlane = null;
    }

    private void handleCam() {
        Camera camera = gameObject.GetComponent<Camera>();
        if (transform.parent != null) {
            transform.position = transform.parent.position + offset;
        } else {
            if (spectatedPlane != null) transform.position = spectatedPlane.transform.position + offset;

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

    private void scrollSpectatablePlanes() {
        planeToControl = null;

        GameObject[] planes = GameObject.FindGameObjectsWithTag("Plane");
        if (spectatedPlane != null) {
            for (int i = 0; i < planes.Length; i++) {
                if (planes[i] == spectatedPlane) {
                    spectatedPlane = planes[(i + 1) % planes.Length];
                    break;
                }
            }
        } else {
            spectatedPlane = planes[0];
        }
    }

    private void scrollCrewedPlanes() {
        spectatedPlane = null;

        GameObject[] planes = GameObject.FindGameObjectsWithTag("Plane");
        List<GameObject> crewedPlanes = new List<GameObject>(); //Note: does not include planes with dead crew
        for (int i = 0; i < planes.Length; i++) {
            if (!planes[i].GetComponent<PlaneController>().allCrewGoneFromPlane()) {
                crewedPlanes.Add(planes[i]);
            }
        }
        if (crewedPlanes.Count == 0) {
            planeToControl = null;
            return;
        }
        if (planeToControl != null) {
            if (!planeToControl.GetComponent<PlaneController>().allCrewGoneFromPlane()) {
                for (int i = 0; i < crewedPlanes.Count; i++) {
                    if (planeToControl == crewedPlanes[i]) {
                        takeControlOfPlane(crewedPlanes[(i + 1) % crewedPlanes.Count]);
                        break;
                    }
                }
            } else {
                takeControlOfPlane(crewedPlanes[0]);
            }
        } else {  
            takeControlOfPlane(crewedPlanes[0]);
        }
    }

    public void takeControlOfPlane(GameObject plane) {
        if (plane != null) {
            planeToControl = plane;
            foreach (PlaneController controller in plane.GetComponents<PlaneController>()) {
                controller.enabled = controller.GetType() != plane.GetComponent<AiPlaneController>().GetType();
            }
        }
    }

    private GameObject findNewPlane(string alliance) {
        foreach (GameObject plane in GameObject.FindGameObjectsWithTag("Plane")) {
            if (plane == planeToControl) continue;

            if (plane.GetComponent<AiPlaneController>().getAlliance() == alliance) {
                if (!plane.GetComponent<PlaneController>().planeDead() && plane.GetComponent<AiPlaneController>().enabled) {
                    return plane;
                }
            }
        }

        return null;
    }

    private GameObject nearestEnemy() {
        if (planeToControl == null) return null;
        GameObject nearestEnemy = null;
        foreach (GameObject plane in GameObject.FindGameObjectsWithTag("Plane")) {
            if (plane == planeToControl) continue;

            if (plane.GetComponent<AiPlaneController>().getAlliance() != planeToControl.GetComponent<AiPlaneController>().getAlliance()) {
                if (!plane.GetComponent<PlaneController>().planeDead()) {
                    if (nearestEnemy == null) {
                        nearestEnemy = plane;
                        continue;
                    }
                    if ((plane.transform.position - planeToControl.transform.position).magnitude < (nearestEnemy.transform.position - planeToControl.transform.position).magnitude) {
                        nearestEnemy = plane;
                    }
                }
            }
        }
        return nearestEnemy;
    }

    public GameObject getControlledPlane() {
        return planeToControl;
    }

    public GameObject getControlledOrSpectatedPlane() {
        if (planeToControl != null && spectatedPlane == null) {
            return planeToControl;
        }
        if (planeToControl == null && spectatedPlane != null) {
            return spectatedPlane;
        } 
        return null;
    }
}
