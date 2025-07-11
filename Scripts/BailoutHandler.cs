using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BailoutHandler : MonoBehaviour {

    [SerializeField] private GameObject crew;
    [SerializeField] private string following;

    public void bailOut() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<DamageModel>() == null) continue;
            if (!transform.GetChild(i).GetComponent<DamageModel>().isCrewRole()) continue;

            if (transform.GetChild(i).GetComponent<DamageModel>().isAlive()) {
                GameObject hitbox = transform.GetChild(i).gameObject;

                GameObject newCrew = Instantiate(crew, hitbox.transform.position, Quaternion.identity);
                newCrew.GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity;

                if (hitbox.name.Contains(following)) {
                    transform.Find("Camera").parent = newCrew.transform;
                }

                hitbox.GetComponent<BoxCollider2D>().size = newCrew.GetComponent<BoxCollider2D>().size;
                hitbox.GetComponent<BoxCollider2D>().offset = newCrew.GetComponent<BoxCollider2D>().offset;
                hitbox.transform.rotation = newCrew.transform.rotation;
                hitbox.transform.parent = newCrew.transform;
                
                i--;
            }
        }
    }
}
