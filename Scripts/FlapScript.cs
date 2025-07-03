using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlapScript : MonoBehaviour {
    [SerializeField] private float maxDeflection; //note: includes base 90 deg offset
    [SerializeField] private float flapSpeed;
    [SerializeField] private bool flapsDown;
    [SerializeField] private float flapEffectiveness;
    [SerializeField] private float flapDrag;

    void Update() {
        handleFlaps();
    }

    private void handleFlaps() {
        if (flapsDown) {
            if (transform.localEulerAngles.z - 90f < maxDeflection) {
                transform.RotateAround(transform.Find("Rp").position, transform.forward, flapSpeed * Time.deltaTime);
            }
        } else {
            if (transform.localEulerAngles.z - 90f > 0f) {
                transform.RotateAround(transform.Find("Rp").position, transform.forward, -flapSpeed * Time.deltaTime);
            }
        }
    }

    public void hideFlaps() {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    public void unhideFlaps() {
        GetComponent<SpriteRenderer>().enabled = true;
    }

    public void toggleFlaps() {
        flapsDown = !flapsDown;
    }

    public float getFlapEffectiveness() {
        return flapEffectiveness;
    }

    public float getFlapDrag() {
        return flapDrag;
    }

    public float getMaxDeflection() {
        return maxDeflection;
    }
}
