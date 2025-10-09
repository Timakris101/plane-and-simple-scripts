using UnityEngine;

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
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;
            
            if (!transform.GetChild(i).GetComponent<DamageModel>().isCrewRole() && transform.GetChild(i).GetComponent<DamageModel>().isCritical()) {
                if (!transform.GetChild(i).GetComponent<DamageModel>().isAlive()) {
                    criticalSystemDamage = true;
                    break;
                }
            }
        }
        if (allCrewGoneFromVehicle()) return true;
        return criticalSystemDamage;
    }

    public bool allCrewGoneFromVehicle() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;
            
            if (transform.GetChild(i).GetComponent<DamageModel>().isCrewRole()) {
                if (transform.GetChild(i).GetComponent<DamageModel>().isAlive()) {
                    return false;
                }
            }
        }
        return true;
    }

    public virtual bool whenToRemoveCamera() {return true;}
    
    protected void setGunnersToManual(bool manual) {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunnerScript>() != null) transform.GetChild(i).GetComponent<GunnerScript>().setManualControl(manual);
        }
    }

    protected void toggleGunners() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunnerScript>() != null) {
                transform.GetChild(i).GetComponent<GunnerScript>().setManualControl(!transform.GetChild(i).GetComponent<GunnerScript>().getManualControl());
            }
        }
    }

    public bool gunnersAreManual() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<GunnerScript>() != null) return transform.GetChild(i).GetComponent<GunnerScript>().getManualControl();
        }
        return false;
    }
}
