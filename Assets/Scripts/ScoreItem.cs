using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreItem : MonoBehaviour
{
    [Header("加算するスコア")] public int myScore;
    [Header("プレイヤーの判定")] public PlayerTriggerCheck playerCheck;

    // Update is called once per frame
    void Update()
    {
        if(playerCheck.isOn)
        {
            if(GManager.instance != null)
            {
                GManager.instance.score += myScore;
                Destroy(this.gameObject);
            }
        }
    }
}
