using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneModelScript : MonoBehaviour {
    void OnCollisionEnter2D(Collision2D col) {
        if (col.transform.tag == "Plane") {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), col.transform.GetComponent<Collider2D>(), true);
        }
    }
}
