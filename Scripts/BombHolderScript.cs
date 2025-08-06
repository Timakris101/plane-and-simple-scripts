using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombHolderScript : GunScript {
    void Start() {
        base.Start();
        transform.parent.GetComponent<Rigidbody2D>().mass += bullet.GetComponent<Rigidbody2D>().mass * ammunition;
    }

    void Update() {
        base.Update();
        GetComponent<SpriteRenderer>().sprite = (ammunition != 0 ? bullet.GetComponent<SpriteRenderer>().sprite : null);
    }

    protected override void shoot() {
        base.shoot();
        transform.parent.GetComponent<Rigidbody2D>().mass -= bullet.GetComponent<Rigidbody2D>().mass;
    }
}
