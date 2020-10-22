using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageCtrl : MonoBehaviour
{
    [Header("PlayerのGameObject")] public GameObject playerGO;
    [Header("コンティニュー位置")] public GameObject[] continuePoint;
    private Player p;

    // Start is called before the first frame update
    void Start()
    {
        if(playerGO != null && continuePoint != null && continuePoint.Length > 0)
        {
            playerGO.transform.position = continuePoint[0].transform.position;
        }
        else
        {
            Debug.Log("必要な項目が設定されていません。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        p = playerGO.GetComponent<Player>();
        if(p != null && p.IsContinueWaiting())
        {
            playerGO.transform.position = continuePoint[0].transform.position;
            p.ContinuePlayer();
        }
    }
}
