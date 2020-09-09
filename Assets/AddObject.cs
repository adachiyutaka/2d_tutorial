using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // ResourcesフォルダにあるNew SpriteプレハブをGameObject型で取得
        GameObject obj = (GameObject)Resources.Load("New Sprite");
        
        // New Spriteプレハブを元に、インスタンスを生成、
        Instantiate(obj, new Vector3(0.0f, 2.0f, 0.0f), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}