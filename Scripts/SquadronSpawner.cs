using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SquadronSpawner : MonoBehaviour {  
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
    [SerializeField] private Vector2 offset;
    [SerializeField] private GameObject camera;

    [Header("InputAreas")]
    [SerializeField] private GameObject amountTextField;
    [SerializeField] private GameObject selectorDropdown;
    [SerializeField] private GameObject containsPlayerToggle;

    void Start() {
        if (selectionSpawner) {
            for (int i = 0; i < planes.Length; i++) {
                selectorDropdown.GetComponent<TMP_Dropdown>().options.Add(new TMP_Dropdown.OptionData(planes[i].name));
            }
        }
        camera = GameObject.Find("Camera");
    }

    void Update() {
        if (camera != null && selectionSpawner) {
            if (Input.GetMouseButtonDown(0)) {
                foreach (Collider2D col in Physics2D.OverlapCircleAll(camera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camera.transform.position.z)), .1f)) {
                    setCurrentSelectedObj(col.gameObject != curSelected ? col.gameObject : curSelected);
                }
            }
            if (Input.GetKey(KeyCode.Escape)) setCurrentSelectedObj(null);
            if (Input.GetKey(KeyCode.Backspace)) Destroy(curSelected);
            if (curSelected != null) editSpawner();
            if (Input.GetMouseButtonDown(1) && curSelected == null) {
                GameObject newSpawner = Instantiate(baseSpawner, camera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camera.transform.position.z)), Quaternion.identity);
                editSpawner(newSpawner);
                setCurrentSelectedObj(newSpawner);
            }
        }
    }

    public void editSpawner(GameObject spawnerToEdit) {
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
            spawner.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public void spawnPlanes() {
        for (int i = 0; i < amt; i++) {
            GameObject newPlane = Instantiate(plane, transform.position + (Vector3) offset * i, transform.rotation);
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
            curSelected.GetComponent<SpriteRenderer>().sprite = selected;
        } else {
            curSelected = null;
        }
    }
}
