using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudScript : MonoBehaviour {
    private float timer;
    [SerializeField] private float lifeTime;
    [SerializeField] private float maxSpeed;
    private float speed;

    void Start() {
        speed = Random.Range(-maxSpeed, maxSpeed);
    }

    void Update() {
        transform.position += Vector3.right * speed * Time.deltaTime;
        
        timer += Time.deltaTime;
        var emissionModule = GetComponent<ParticleSystem>().emission;
        if (timer >= lifeTime) {
            emissionModule.rate = 0f;
        }
        if (GetComponent<ParticleSystem>().particleCount == 0 && emissionModule.rate.constant == 0f) {
            Destroy(gameObject);
        }
    }
}
