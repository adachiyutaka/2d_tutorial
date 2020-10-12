using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikyPile : MonoBehaviour
{
    [Header("移動速度")] public float speed = 3;
    [Header("移動距離")] public float range = 1;

    private Vector3 floorDefaultPos;
    private PolygonCollider2D col;
    private Rigidbody2D rb;
    private float timer = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<PolygonCollider2D>();
        if (col != null && rb != null)
        {
            floorDefaultPos = gameObject.transform.position;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        col.transform.position = floorDefaultPos + new Vector3(Mathf.Sin(timer * speed) * range, 0, 0);
        timer += Time.deltaTime;
    }
}
