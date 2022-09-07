using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testLookAt : MonoBehaviour
{
    public GameObject target;
    public GameObject child;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {   
        // Quaternion q1 = Quaternion.Euler(45f, 0f, 0f);
        // Quaternion q2 = Quaternion.Euler(0f, 45f, 0f);
        // target.transform.localRotation = q1 * q2;
        // child.transform.localRotation = q2;
        // 補完スピードを決める
        float speed = 0.1f;
        // ターゲット方向のベクトルを取得
        Vector3 relativePos = target.transform.position - this.transform.position;
        // 方向を、回転情報に変換
        Quaternion rotation = Quaternion.LookRotation (relativePos);
        rotation = rotation * Quaternion.Euler(90f, 0f, 0f);
        // 現在の回転情報と、ターゲット方向の回転情報を補完する
        transform.rotation = Quaternion.Slerp (this.transform.rotation, rotation, speed);
     }
}