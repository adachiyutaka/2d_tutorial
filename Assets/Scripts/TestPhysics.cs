using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPhysics : MonoBehaviour
{
    private Rigidbody2D rb = null;

    public bool is_force = true;
    public float forceY = 10.0f;
    public float speedY = 10.0f;
    public float timePeriod = 1.0f;
    private float time;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timePeriod < time)
        {
            if (is_force == true)
            {
                Vector2 force = new Vector2 (0.0f, forceY);    // 力を設定
                rb.AddForce (force, ForceMode2D.Force);          // 力を加える
                Debug.Log($"force: {rb.velocity}");
            }
            else
            {
                Vector2 speed = new Vector2 (0.0f, speedY);    // 力を設定
                rb.velocity = speed;
                Debug.Log($"velocity: {rb.velocity}");
            }
            time = 0.0f;
        }

        time += Time.deltaTime;
    }
}
