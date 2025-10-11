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
        spriteDisps = new List<GameObject>();
        moduleImage.GetComponent<RectTransform>().sizeDelta = transform.GetChild(0).GetComponent<RectTransform>().sizeDelta;
    }

    public void displayVehicle(GameObject vehicle) {
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = null;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = false;
        this.vehicle = vehicle;
        
        foreach (CoupledModule c in coupledModules) {
            Destroy(c.getDisp());
        }
        coupledModules = new List<CoupledModule>();
        foreach (GameObject g in spriteDisps) {
            if (g != transform.GetChild(0).gameObject) Destroy(g.getDisp());
        }
        spriteDisps = new List<CoupledModule>();
        if (vehicle == null) {
            return;
        }
        
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = null;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = true;

        makeImages();
        
        foreach (CoupledModule c in coupledModules) {
            c.getDisp().GetComponent<UnityEngine.UI.Image>().color = healthDispGradient.Evaluate(Mathf.Max(c.getCoupledModule().GetComponent<DamageModel>().getHealth(), 0f) / c.getCoupledModule().GetComponent<DamageModel>().getMaxHealth());
        }
    }

    void makeImages() {
        if (vehicle != null) makeImagesFor(vehicle);
    }

    void makeImagesFor(GameObject obj) {
        float sizeMultiplier = transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x / obj.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        GameObject img = transform.GetChild(0).gameObject;
        makeImagesFor(obj, img, sizeMultiplier, 0, new Vector3(0,0,0));
    }

    void makeImagesFor(GameObject obj, GameObject img, float sizeMultiplier, int depth, Vector3 prevAdditionalOffset) {
        if (depth == 0) {
            img.GetComponent<UnityEngine.UI.Image>().enabled = true;

            if (obj.GetComponent<SpriteRenderer>() != null) {
                img.GetComponent<UnityEngine.UI.Image>().sprite = obj.GetComponent<VehicleController>() != null ? obj.GetComponent<VehicleController>().getOrigSprite() : obj.GetComponent<SpriteRenderer>().sprite;
                if (img.GetComponent<UnityEngine.UI.Image>().sprite != null) {
                    img.GetComponent<UnityEngine.UI.Image>().enabled = true;
                    spriteDisps.Add(new CoupledModule(img, obj));
                }
            } else {
                img.GetComponent<UnityEngine.UI.Image>().enabled = false;
            }
            if (obj.GetComponent<DamageModel>() != null) img.GetComponent<UnityEngine.UI.Image>().enabled = true;

            img.transform.localEulerAngles = new Vector3(0,0,0);
        }
        for (int i = 0; i < obj.transform.childCount; i++) {
            if (obj.transform.GetChild(i).GetComponent<Camera>() != null) return;

            GameObject newImg = Instantiate(moduleImage, img.transform);
            GameObject cObj = obj.transform.GetChild(i).gameObject;
    
            if (cObj.GetComponent<DamageModel>() != null) {
                newImg.GetComponent<RectTransform>().sizeDelta = cObj.GetComponent<BoxCollider2D>().size * sizeMultiplier; //size delta also changes position so it is done first
            }

            Vector3 additionalOffset = cObj.GetComponent<DamageModel>() != null ? (Vector3) cObj.GetComponent<BoxCollider2D>().offset : new Vector3(0,0,0) - prevAdditionalOffset;
            newImg.transform.localPosition = (cObj.transform.localPosition + additionalOffset) * sizeMultiplier;

            newImg.transform.localEulerAngles = cObj.transform.localEulerAngles;

            if (cObj.GetComponent<SpriteRenderer>() != null) newImg.transform.localScale = (cObj == vehicle ? new Vector3(1f,1f,1f) : cObj.transform.localScale * vehicle.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit / cObj.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit);

            if (cObj.GetComponent<SpriteRenderer>() != null) {
                newImg.GetComponent<UnityEngine.UI.Image>().enabled = true;
                newImg.GetComponent<UnityEngine.UI.Image>().sprite = cObj.GetComponent<SpriteRenderer>().sprite;
                if (newImg.GetComponent<UnityEngine.UI.Image>().sprite != null) {
                    newImg.GetComponent<UnityEngine.UI.Image>().enabled = true;
                    spriteDisps.Add(new CoupledModule(newImg, cObj));
                }
            } else {
                newImg.GetComponent<UnityEngine.UI.Image>().enabled = false;
            }
            if (cObj.GetComponent<DamageModel>() != null) {
                newImg.GetComponent<UnityEngine.UI.Image>().enabled = true;
                coupledModules.Add(new CoupledModule(newImg, cObj));
            }

            makeImagesFor(cObj, newImg, sizeMultiplier, depth + 1, additionalOffset);
        }
    }

    void Update() {
        displayVehicle(camera.GetComponent<CamScript>().getControlledOrSpectatedVehicle());
    }
}
