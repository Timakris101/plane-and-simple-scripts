using UnityEngine;

public class AiGroundVehicleController : GroundVehicleController {

    [SerializeField] private GameObject targetedObj;

    void Start() {
        
    }

    new void Update() { 
        setGunnersToManual(false);
    }

    private void findTarget() {
        GameObject[] allPlanes = GameObject.FindGameObjectsWithTag("Plane");
        targetedObj = null;
        foreach (GameObject plane in allPlanes) {
            if (plane == gameObject) continue;
            
            if (plane.GetComponent<AllianceHolder>().getAlliance() != GetComponent<AllianceHolder>().getAlliance() && !plane.GetComponent<VehicleController>().vehicleDead()) {
                if (targetedObj == null) targetedObj = plane;

                if (Vector3.Distance(plane.transform.position, transform.position) < Vector3.Distance(targetedObj.transform.position, transform.position)) {
                    targetedObj = plane;
                }
            }
        }
    }

    public GameObject getTargetedObj() {
        findTarget();
        return targetedObj;
    }
}
