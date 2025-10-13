using UnityEngine;
using static Utils;

public class GroundVehicleController : VehicleController {
    [SerializeField] private PhysicsMaterial2D brakeMat;
    [SerializeField] private PhysicsMaterial2D rollMat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        setGunnersToManual(true);
        base.Update();
    }

    public override void handleFeasibleControls() {
        if (progenyWithScript("TrackScript", gameObject).Count == 0) return;
        if (progenyWithScript("TrackScript", gameObject)[0].GetComponent<TrackScript>().usable()) {
            move();
        } else {
            progenyWithScript("TrackScript", gameObject)[0].GetComponent<BoxCollider2D>().sharedMaterial = brakeMat;
        }
    }

    void move() {
        Vector3 movementDir = new Vector3(0,0,0);
        if (Input.GetKey("a")) {
            movementDir += -transform.right * transform.localScale.y;
        }
        if (Input.GetKey("d")) {
            movementDir += transform.right * transform.localScale.y;
        }
        if (Input.GetKeyDown("q")) {
            if (transform.localScale.y == 1f) {
                transform.localScale = new Vector3(1f, -1f, 1f);
                transform.localEulerAngles += new Vector3(0f, 0f, 180f);
            }
        }
        if (Input.GetKeyDown("e")) {
            if (transform.localScale.y == -1f) {
                transform.localScale = new Vector3(1f, 1f, 1f);
                transform.localEulerAngles += new Vector3(0f, 0f, 180f);
            }
        }
        bool goingReverse = movementDir.x / transform.right.x < 0f;
        if (movementDir.magnitude == 0f) {
            progenyWithScript("TrackScript", gameObject)[0].GetComponent<BoxCollider2D>().sharedMaterial = brakeMat;
        } else {
            progenyWithScript("TrackScript", gameObject)[0].GetComponent<BoxCollider2D>().sharedMaterial = rollMat;
        }
        GetComponent<Rigidbody2D>().AddForce(movementDir * transform.Find("EngineHitbox").GetComponent<EngineScript>().getThrustNewtons(GetComponent<Rigidbody2D>().linearVelocity.magnitude, goingReverse));
    }
    
    public override bool whenToRemoveCamera() {return allCrewGoneFromVehicle();}
}
