using UnityEngine;
using static Utils;

public class VehicleController : MonoBehaviour {
    
    private Sprite origSprite;

    void Awake() {
        origSprite = GetComponent<SpriteRenderer>().sprite;
    }

    public Sprite getOrigSprite() {
        return origSprite;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual bool vehicleDead() {
        bool criticalSystemDamage = false;
        foreach (GameObject damageModel in progenyWithScript("DamageModel", gameObject)) {
            if (!damageModel.GetComponent<DamageModel>().isCrewRole() && damageModel.GetComponent<DamageModel>().isCritical()) {
                if (!damageModel.GetComponent<DamageModel>().isAlive()) {
                    criticalSystemDamage = true;
                    break;
                }
            }
        }
        if (allCrewGoneFromVehicle()) return true;
        return criticalSystemDamage;
    }

    public bool allCrewGoneFromVehicle() {
        foreach (GameObject damageModel in progenyWithScript("DamageModel", gameObject)) {
            if (damageModel.GetComponent<DamageModel>().isCrewRole()) {
                if (damageModel.GetComponent<DamageModel>().isAlive()) {
                    return false;
                }
            }
        }
        return true;
    }

    public virtual bool whenToRemoveCamera() {return true;}
    
    protected void setGunnersToManual(bool manual) {
        foreach(GameObject gunner in progenyWithScript("GunnerScript", gameObject)) {
            gunner.GetComponent<GunnerScript>().setManualControl(manual);
        }
    }

    protected void toggleGunners() {
        foreach(GameObject gunner in progenyWithScript("GunnerScript", gameObject)) {
            gunner.GetComponent<GunnerScript>().setManualControl(!gunner.GetComponent<GunnerScript>().getManualControl());
        }
    }

    public bool gunnersAreManual() {
        foreach(GameObject gunner in progenyWithScript("GunnerScript", gameObject)) {
            return gunner.GetComponent<GunnerScript>().getManualControl();
        }
        return false;
    }
}
