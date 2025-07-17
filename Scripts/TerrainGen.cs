using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class TerrainGen : MonoBehaviour {
    [SerializeField] private float terrainPointAmt;
    [SerializeField] private float terrainLength;
    [SerializeField] private float maxHeight;
    void Start() {
        Vector2[] terrainVecs = new Vector2[(int) terrainPointAmt];
        GetComponent<SpriteShapeController>().spline.Clear();
        for (int i = 0; i < terrainPointAmt; i++) {
            terrainVecs[i] = new Vector2(-(terrainLength / 2) + i * (terrainLength / terrainPointAmt), Random.Range(0f, maxHeight));
            GetComponent<SpriteShapeController>().spline.InsertPointAt(i, new Vector2(terrainVecs[i].x, i == 0 || i == terrainPointAmt - 1 ? 0f : terrainVecs[i].y));
        }
        terrainVecs[0] = new Vector2(-terrainLength / 2, 0f);

        terrainVecs[(int) terrainPointAmt - 1] = new Vector2(terrainLength / 2, 0f);

        GetComponent<PolygonCollider2D>().SetPath(0, terrainVecs);
    }
}
