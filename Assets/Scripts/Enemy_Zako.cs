using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Zako : MonoBehaviour
{
    private SpriteRenderer sr = null; 

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (sr.isVisible)
        {
            Debug.Log("画面に見えている");
        } 
    }
}
