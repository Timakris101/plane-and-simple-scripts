using UnityEngine;
using static Utils;

public class AiGroundVehicleController : GroundVehicleController {

    [SerializeField] private GameObject targetedObj;

    void Start() {

    }

    new void Update() { 
        setGunnersToManual(false);
        base.Update();
    }

    private void findTarget() {
        GameObject[] allVehicles = allVehiclesOfTags("Plane", "GroundVehicle");
        targetedObj = null;
        foreach (GameObject vehicle in allVehicles) {
            if (vehicle == gameObject) continue;
            
            if (vehicle.GetComponent<AllianceHolder>().getAlliance() != GetComponent<AllianceHolder>().getAlliance() && !vehicle.GetComponent<VehicleController>().vehicleDead()) {
                if (targetedObj == null) targetedObj = vehicle;

                if (Vector3.Distance(vehicle.transform.position, transform.position) < Vector3.Distance(targetedObj.transform.position, transform.position)) {
                    targetedObj = vehicle;
                }
            }
        }
    }

    protected override void handleFacing() {
        bool goingReverse = moveDir().x / transform.right.x < 0f;
        if (goingReverse) {
            transform.localScale = new Vector3(1f, transform.localScale.y * -1f, 1f);
            transform.localEulerAngles += new Vector3(0f, 0f, 180f);
        }
    }

    protected override Vector3 moveDir() {
        if (targetedObj == null) return new Vector3(0,0,0);
        return Vector3.Project(targetedObj.transform.position - transform.position, transform.right).normalized;
    }

    public GameObject getTargetedObj() {
        findTarget();
        return targetedObj;
    }
}
