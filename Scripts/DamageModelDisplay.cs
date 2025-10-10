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

    public bool equals(CoupledModule c) {
        return coupledModule == c.getCoupledModule();
    }
}

public class DamageModelDisplay : MonoBehaviour {
    [SerializeField] private GameObject vehicle;
    [SerializeField] private GameObject moduleImage;
    [SerializeField] private Gradient healthDispGradient;
    [SerializeField] private GameObject camera;
    private List<CoupledModule> coupledModules;
    private List<GameObject> spriteDisps;

    void Start() {
        coupledModules = new List<CoupledModule>();
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = null;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = false;
    }

    public void displayVehicle(GameObject vehicle) {
        this.vehicle = vehicle;
        foreach (CoupledModule c in coupledModules) {
            Destroy(c.getDisp());
        }
        coupledModules = new List<CoupledModule>();
        spriteDisps = new List<GameObject>();
        if (vehicle == null) {
            return;
        }
        float sizeMultiplier = transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x / vehicle.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        foreach (GameObject objectWithSprite in allObjectsInTreeWith("SpriteRenderer", vehicle)) {
            GameObject newSpriteDisp = Instantiate(transform.GetChild(0).gameObject, transform);
            newSpriteDisp.GetComponent<UnityEngine.UI.Image>().enabled = true;
            newSpriteDisp.GetComponent<UnityEngine.UI.Image>().sprite = objectWithSprite.GetComponent<SpriteRenderer>().sprite;
            newSpriteDisp.transform.localPosition = sizeMultiplier * (localPositionFrom(vehicle, objectWithSprite));
            newSpriteDisp.transform.localScale = objectWithSprite.transform.localScale;
            newSpriteDisp.transform.eulerAngles = objectWithSprite.transform.eulerAngles - vehicle.transform.eulerAngles;
            spriteDisps.Add(newSpriteDisp);
            Destroy(newSpriteDisp, Time.deltaTime * 2f);
        }
        foreach (GameObject damageModel in progenyWithScript("DamageModel", vehicle)) {
            GameObject newModuleDisp = Instantiate(moduleImage, transform);
            newModuleDisp.GetComponent<RectTransform>().sizeDelta = damageModel.GetComponent<BoxCollider2D>().size * sizeMultiplier; //size delta also changes position so it is done first
            newModuleDisp.transform.localPosition = (damageModel.transform.localPosition + (Vector3) damageModel.GetComponent<BoxCollider2D>().offset) * sizeMultiplier;
            newModuleDisp.transform.eulerAngles = damageModel.transform.eulerAngles - vehicle.transform.eulerAngles;

            if (!coupledModules.Contains(new CoupledModule(newModuleDisp, damageModel))) {
                coupledModules.Add(new CoupledModule(newModuleDisp, damageModel));
            } else {
                Destroy(newModuleDisp);
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
