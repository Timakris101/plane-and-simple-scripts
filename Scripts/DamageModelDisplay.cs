using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

[System.Serializable]
public class CoupledModule {
    [SerializeField] private GameObject display;
    [SerializeField] private GameObject coupledModule;

    public CoupledModule(GameObject display, GameObject coupledModule) {
        this.display = display;
        this.coupledModule = coupledModule;
    }

    public CoupledModule() {}

    public GameObject getDisp() {
        return display;
    }

    public GameObject getCoupledModule() {
        return coupledModule;
    }

    public void setDisp(GameObject disp) {
        display = disp;
    }

    public void setCoupledModule(GameObject module) {
        coupledModule = module;
    }
}

public class DamageModelDisplay : MonoBehaviour {
    [SerializeField] private GameObject vehicle;
    [SerializeField] private GameObject moduleImage;
    [SerializeField] private Gradient healthDispGradient;
    [SerializeField] private GameObject camera;
    private List<CoupledModule> coupledModules;

    void Start() {
        coupledModules = new List<CoupledModule>();
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = null;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = false;
    }

    public void displayVehicle(GameObject vehicle) {
        this.vehicle = vehicle;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = null;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = false;
        foreach (CoupledModule c in coupledModules) {
            Destroy(c.getDisp());
        }
        coupledModules = new List<CoupledModule>();
        if (vehicle == null) {
            return;
        }
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = true;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = vehicle.GetComponent<VehicleController>().getOrigSprite();
        float sizeMultiplier = GetComponent<RectTransform>().sizeDelta.x / vehicle.GetComponent<VehicleController>().getOrigSprite().bounds.size.x;
        for (int i = 0; i < vehicle.transform.childCount; i++) {
            if (vehicle.transform.GetChild(i).GetComponent<DamageModel>() != null) {
                GameObject newModuleDisp = Instantiate(moduleImage, transform);
                newModuleDisp.GetComponent<RectTransform>().sizeDelta = vehicle.transform.GetChild(i).GetComponent<BoxCollider2D>().size * sizeMultiplier; //size delta also changes position so it is done first
                newModuleDisp.transform.localPosition = (vehicle.transform.GetChild(i).localPosition + (Vector3) vehicle.transform.GetChild(i).GetComponent<BoxCollider2D>().offset) * sizeMultiplier;
                newModuleDisp.transform.localEulerAngles = vehicle.transform.GetChild(i).localEulerAngles;

                coupledModules.Add(new CoupledModule(newModuleDisp, vehicle.transform.GetChild(i).gameObject));
            }
        }
        foreach (CoupledModule c in coupledModules) {
            c.getDisp().GetComponent<UnityEngine.UI.Image>().color = healthDispGradient.Evaluate(Mathf.Max(c.getCoupledModule().GetComponent<DamageModel>().getHealth(), 0f) / c.getCoupledModule().GetComponent<DamageModel>().getMaxHealth());
        }
    }

    void Update() {
        displayVehicle(camera.GetComponent<CamScript>().getControlledOrSpectatedVehicle());
    }
}
