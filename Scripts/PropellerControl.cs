using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerControl : MonoBehaviour {
    void Update() {
        GetComponent<Animator>().speed = transform.parent.GetComponent<PlaneController>().getThrottle();
    }
}
