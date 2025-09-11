using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour {
    [SerializeField] private float dragForceCoef;
    [SerializeField] private GameObject splashEffect;
    [SerializeField] private float splashCoef;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.transform.tag != "Plane" && other.transform.tag != "Crew" && other.transform.parent == null) {
            Destroy(other.transform.gameObject);
        }
        if (other.transform.parent == null) {
            GameObject newSplash = Instantiate(splashEffect, other.transform.position, Quaternion.identity);
            var mainModule = newSplash.GetComponent<ParticleSystem>().main;
            mainModule.startSpeed = new ParticleSystem.MinMaxCurve(splashSize(other.transform.gameObject) / 2f, splashSize(other.transform.gameObject));
        }
    }

    private float splashSize(GameObject objEntering) {
        return objEntering.GetComponent<Rigidbody2D>().mass * objEntering.GetComponent<Rigidbody2D>().velocity.magnitude * splashCoef;
    }

    void OnTriggerStay2D(Collider2D other) {
        if (other.transform.tag == "Plane") {
            float dragForce = dragForceCoef * Mathf.Pow(other.transform.GetComponent<Rigidbody2D>().velocity.magnitude, 2);

            other.transform.GetComponent<Rigidbody2D>().AddForce(-other.transform.GetComponent<Rigidbody2D>().velocity.normalized * dragForce);
        }
    }
}
