using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadronSpawner : MonoBehaviour {
    
    [SerializeField] private GameObject plane;
    [SerializeField] private bool containsPlayer;
    [SerializeField] private int amt;
    [SerializeField] private float rotation;
    [SerializeField] private Vector2 offset;
    [SerializeField] private GameObject camera;

    void Update() {
        foreach (GameObject spawner in GameObject.FindGameObjectsWithTag("Spawner")) {
            if (spawner != gameObject && containsPlayer) {
                spawner.GetComponent<SquadronSpawner>().setContainsPlayer(false);
            }
        }
    }

    public void setContainsPlayer(bool b) {
        containsPlayer = b;
    }

    public static void activateSquadronSpawners() {
        foreach (GameObject spawner in GameObject.FindGameObjectsWithTag("Spawner")) {
            spawner.GetComponent<SquadronSpawner>().spawnPlanes();
        }
    }

    public void spawnPlanes() {
        for (int i = 0; i < amt; i++) {
            GameObject newPlane = Instantiate(plane, transform.position + (Vector3) offset * i, Quaternion.Euler(0, 0, rotation));
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
}
