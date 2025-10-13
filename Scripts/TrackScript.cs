using UnityEngine;

public class TrackScript : MonoBehaviour {
    [SerializeField] private bool contactingGround;

    void OnCollisionStay2D(Collision2D other) {
        if (other.transform.tag == "Ground") {
            contactingGround = true;
        }
    }

    void OnCollisionExit2D(Collision2D other) {
        if (other.transform.tag == "Ground") {
            contactingGround = false;
        }
    }

    public bool isContactingGround() {
        return contactingGround;
    }

    public bool usable() {
        return contactingGround && GetComponent<DamageModel>().isAlive();
    }
}
