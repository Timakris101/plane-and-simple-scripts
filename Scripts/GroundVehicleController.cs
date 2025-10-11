using UnityEngine;

public class GroundVehicleController : VehicleController
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        setGunnersToManual(true);
        base.Update();
    }
    
    public override bool whenToRemoveCamera() {return allCrewGoneFromVehicle();}
}
