using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private GameObject plane;
    [SerializeField] private GameObject moduleImage;
    [SerializeField] private Gradient healthDispGradient;
    [SerializeField] private GameObject camera;
    private List<CoupledModule> coupledModules;

    void Start() {
        coupledModules = new List<CoupledModule>();
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = null;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = false;
    }

    public void displayPlane(GameObject plane) {
        this.plane = plane;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = null;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = false;
        foreach (CoupledModule c in coupledModules) {
            Destroy(c.getDisp());
        }
        coupledModules = new List<CoupledModule>();
        if (plane == null) {
            return;
        }
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = true;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = plane.GetComponent<PlaneController>().getOrigSprite();
        float sizeMultiplier = GetComponent<RectTransform>().sizeDelta.x / plane.GetComponent<SpriteRenderer>().bounds.size.x;
        for (int i = 0; i < plane.transform.childCount; i++) {
            if (plane.transform.GetChild(i).GetComponent<DamageModel>() != null) {
                GameObject newModuleDisp = Instantiate(moduleImage, transform);
                newModuleDisp.GetComponent<RectTransform>().sizeDelta = plane.transform.GetChild(i).GetComponent<BoxCollider2D>().size * sizeMultiplier; //size delta also changes position so it is done first
                newModuleDisp.transform.localPosition = (plane.transform.GetChild(i).localPosition + (Vector3) plane.transform.GetChild(i).GetComponent<BoxCollider2D>().offset) * sizeMultiplier;

                coupledModules.Add(new CoupledModule(newModuleDisp, plane.transform.GetChild(i).gameObject));
            }
        }
    }

    void Update() {
        foreach (CoupledModule c in coupledModules) {
            c.getDisp().GetComponent<UnityEngine.UI.Image>().color = healthDispGradient.Evaluate(Mathf.Max(c.getCoupledModule().GetComponent<DamageModel>().getHealth(), 0f) / c.getCoupledModule().GetComponent<DamageModel>().getMaxHealth());
        }
        if (camera.GetComponent<CamScript>().getControlledOrSpectatedPlane() != plane) displayPlane(camera.GetComponent<CamScript>().getControlledOrSpectatedPlane());
    }
}
