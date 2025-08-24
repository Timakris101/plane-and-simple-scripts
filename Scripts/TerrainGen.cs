using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class TerrainGen : MonoBehaviour {
    [SerializeField] private float terrainPointAmt;
    [SerializeField] private float terrainLength;
    [SerializeField] private float maxHeight;
    [SerializeField] private AnimationCurve baseTerrHeight;
    [SerializeField] private float waterLvl;
    void Start() {
        Vector2[] terrainVecs = new Vector2[(int) terrainPointAmt];
        GetComponent<SpriteShapeController>().spline.Clear();
        for (int i = 0; i < terrainPointAmt; i++) {
            terrainVecs[i] = new Vector2(-(terrainLength / 2) + i * (terrainLength / terrainPointAmt), Random.Range(maxHeight / 2f, maxHeight) + baseTerrHeight.Evaluate(i / terrainPointAmt));
            GetComponent<SpriteShapeController>().spline.InsertPointAt(i, new Vector2(terrainVecs[i].x, i == 0 || i == terrainPointAmt - 1 ? 0f : terrainVecs[i].y));
        }
        terrainVecs[0] = new Vector2(-terrainLength / 2, 0f);

        terrainVecs[(int) terrainPointAmt - 1] = new Vector2(terrainLength / 2, 0f);

        GetComponent<PolygonCollider2D>().SetPath(0, terrainVecs);

        GetComponent<SpriteShapeRenderer>().localBounds = new Bounds(Vector3.zero, new Vector3(terrainLength * transform.localScale.x, maxHeight * transform.localScale.y, 0)); 

        if (waterLvl != 0) transform.GetChild(0).localScale = new Vector3(terrainLength, waterLvl * 2f, 1f);
    }
}
