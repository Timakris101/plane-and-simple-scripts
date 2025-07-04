using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneInit : MonoBehaviour {

    [SerializeField] private float speed;

    void Start() {
        GetComponent<Rigidbody2D>().velocity = transform.right * speed;
    }
}
