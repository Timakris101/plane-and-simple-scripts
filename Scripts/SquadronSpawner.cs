using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SquadronSpawner : MonoBehaviour {  
    [Header("Mode")]
    [SerializeField] private bool arcade;
    private bool on;

    [Header("SelectionSpawner")]
    [SerializeField] private bool selectionSpawner;
    [SerializeField] private GameObject curSelected;
    [SerializeField] private GameObject[] planes;
    [SerializeField] private GameObject baseSpawner;
    [SerializeField] private Sprite unselected;
    [SerializeField] private Sprite selected;

    [Header("Stats")]
    [SerializeField] private GameObject plane;
    [SerializeField] private bool containsPlayer;
    [SerializeField] private int amt;
    [SerializeField] private string alliance;
    [SerializeField] private Vector2 offset;
    [SerializeField] private GameObject camera;

    [Header("InputAreas")]
    [SerializeField] private GameObject amountTextField;
    [SerializeField] private GameObject selectorDropdown;
    [SerializeField] private GameObject containsPlayerToggle;
    [SerializeField] private GameObject allianceDropdown;

    void Start() {
        if (selectionSpawner) {
            for (int i = 0; i < planes.Length; i++) {
                selectorDropdown.GetComponent<TMP_Dropdown>().options.Add(new TMP_Dropdown.OptionData(planes[i].name));
            }
            selectorDropdown.transform.Find("Label").GetComponent<TMP_Text>().text = baseSpawner.GetComponent<SquadronSpawner>().plane.name;
            for (int i = 0; i < selectorDropdown.GetComponent<TMP_Dropdown>().options.Count; i++) {
                if (selectorDropdown.GetComponent<TMP_Dropdown>().options[i].text == selectorDropdown.transform.Find("Label").GetComponent<TMP_Text>().text) {
                    selectorDropdown.GetComponent<TMP_Dropdown>().value = i;
                }
            }
            allianceDropdown.transform.Find("Label").GetComponent<TMP_Text>().text = baseSpawner.GetComponent<SquadronSpawner>().plane.GetComponent<AiPlaneController>().getAlliance();
            for (int i = 0; i < allianceDropdown.GetComponent<TMP_Dropdown>().options.Count; i++) {
                if (allianceDropdown.GetComponent<TMP_Dropdown>().options[i].text == allianceDropdown.transform.Find("Label").GetComponent<TMP_Text>().text) {
                    allianceDropdown.GetComponent<TMP_Dropdown>().value = i;
                }
            }
            amountTextField.GetComponent<TMP_InputField>().text = baseSpawner.GetComponent<SquadronSpawner>().amt.ToString();
            containsPlayerToggle.GetComponent<Toggle>().isOn = baseSpawner.GetComponent<SquadronSpawner>().containsPlayer;
        }
        camera = GameObject.Find("Camera");
    }

    void Update() {
        if (camera != null && selectionSpawner) {
            if (Input.GetMouseButtonDown(0)) {
                foreach (Collider2D col in Physics2D.OverlapCircleAll(camera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camera.transform.position.z)), .1f)) {
                    if (col.transform.GetComponent<SquadronSpawner>() != null) setCurrentSelectedObj(col.gameObject != curSelected ? col.gameObject : curSelected);
                }
            }
            if (Input.GetKey(KeyCode.Escape)) {
                if (curSelected != null) containsPlayerToggle.GetComponent<Toggle>().isOn = false;
                setCurrentSelectedObj(null);
            }
            if (Input.GetKey(KeyCode.Backspace)) Destroy(curSelected);
            if (curSelected != null) editSpawner();
            if (Input.GetMouseButtonDown(1) && curSelected == null) {
                GameObject newSpawner = Instantiate(baseSpawner, camera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camera.transform.position.z)), Quaternion.identity);
                editSpawner(newSpawner);
                setCurrentSelectedObj(newSpawner);
            }
        }
        if (arcade && on) {
            if (!anyPlanesLeft(plane.GetComponent<AiPlaneController>().getAlliance())) {
                spawnPlanes();
                GameObject.Find("Score").GetComponent<TMP_Text>().text = (int.Parse(GameObject.Find("Score").GetComponent<TMP_Text>().text) + (containsPlayer ? -1 : 1)).ToString();
            }
        }
    }

    public bool anyPlanesLeft(string alliance) {
        foreach (GameObject plane in GameObject.FindGameObjectsWithTag("Plane")) {
            if (plane.GetComponent<AiPlaneController>().getAlliance() == alliance) {
                if (!plane.GetComponent<PlaneController>().allCrewGoneFromPlane()) {
                    return true;
                }
            }
        }
        return false;
    }

    public void editSpawner(GameObject spawnerToEdit) {
        if (spawnerToEdit != null) {
            int ignore;
            if (int.TryParse(amountTextField.GetComponent<TMP_InputField>().text, out ignore)) spawnerToEdit.GetComponent<SquadronSpawner>().amt = int.Parse(amountTextField.GetComponent<TMP_InputField>().text);
            spawnerToEdit.GetComponent<SquadronSpawner>().containsPlayer = containsPlayerToggle.GetComponent<Toggle>().isOn;
            foreach (GameObject spawner in GameObject.FindGameObjectsWithTag("Spawner")) {
                if (spawner != spawnerToEdit && spawnerToEdit.GetComponent<SquadronSpawner>().containsPlayer) {
                    spawner.GetComponent<SquadronSpawner>().setContainsPlayer(false);
                }
            }
            if (Input.GetMouseButton(0)) {
                Vector3 dir = camera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camera.transform.position.z)) - curSelected.transform.position;
                if (dir.magnitude < curSelected.GetComponent<CircleCollider2D>().radius * 2f) {
                    spawnerToEdit.transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.normalized.y, dir.normalized.x) * 180f / 3.14f);
                }
            }
            if (Input.GetMouseButton(1)) {
                spawnerToEdit.transform.position = camera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camera.transform.position.z));
            }

            spawnerToEdit.GetComponent<SquadronSpawner>().plane = planes[selectorDropdown.GetComponent<TMP_Dropdown>().value];
            spawnerToEdit.GetComponent<SquadronSpawner>().alliance = allianceDropdown.GetComponent<TMP_Dropdown>().options[allianceDropdown.GetComponent<TMP_Dropdown>().value].text;
        }
    }

    public void editSpawner() {
        editSpawner(curSelected);      
    }

    public void setContainsPlayer(bool b) {
        containsPlayer = b;
    }

    public static void activateSquadronSpawners() {
        foreach (GameObject spawner in GameObject.FindGameObjectsWithTag("Spawner")) {
            spawner.GetComponent<SquadronSpawner>().spawnPlanes();
            spawner.GetComponent<SquadronSpawner>().on = true;
            spawner.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public void spawnPlanes() {
        for (int i = 0; i < amt; i++) {
            GameObject newPlane = Instantiate(plane, transform.position + (Vector3) offset * i, transform.rotation);
            newPlane.GetComponent<AiPlaneController>().setAlliance(alliance);
            if (containsPlayer && i == 0) {
                camera.GetComponent<CamScript>().takeControlOfPlane(newPlane);
                continue;
            }
            foreach (PlaneController pc in newPlane.GetComponents<PlaneController>()) {
                if (pc == newPlane.GetComponent<AiPlaneController>()) {
                    pc.enabled = true;
                } else {
                    pc.enabled = false;
                }
            }
        }
    }

    public void setCurrentSelectedObj(GameObject obj) {
        foreach (GameObject spawner in GameObject.FindGameObjectsWithTag("Spawner")) {
            spawner.GetComponent<SpriteRenderer>().sprite = unselected;
        }
        if (obj != null) {
            curSelected = obj;
            amountTextField.GetComponent<TMP_InputField>().text = curSelected.GetComponent<SquadronSpawner>().amt.ToString();
            containsPlayerToggle.GetComponent<Toggle>().isOn = curSelected.GetComponent<SquadronSpawner>().containsPlayer;
            for (int i = 0; i < selectorDropdown.GetComponent<TMP_Dropdown>().options.Count; i++) {
                if (selectorDropdown.GetComponent<TMP_Dropdown>().options[i].text == curSelected.GetComponent<SquadronSpawner>().plane.name) {
                    selectorDropdown.GetComponent<TMP_Dropdown>().value = i;
                }
            }
            selectorDropdown.transform.Find("Label").GetComponent<TMP_Text>().text = selectorDropdown.GetComponent<TMP_Dropdown>().options[selectorDropdown.GetComponent<TMP_Dropdown>().value].text;

            for (int i = 0; i < allianceDropdown.GetComponent<TMP_Dropdown>().options.Count; i++) {
                if (allianceDropdown.GetComponent<TMP_Dropdown>().options[i].text == curSelected.GetComponent<SquadronSpawner>().alliance) {
                    allianceDropdown.GetComponent<TMP_Dropdown>().value = i;
                }
            }
            allianceDropdown.transform.Find("Label").GetComponent<TMP_Text>().text = allianceDropdown.GetComponent<TMP_Dropdown>().options[allianceDropdown.GetComponent<TMP_Dropdown>().value].text;

            curSelected.GetComponent<SpriteRenderer>().sprite = selected;
        } else {
            curSelected = null;
        }
    }
}
