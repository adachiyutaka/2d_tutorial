using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    private Text scoreText = null;
    private int nowScore;
    private int oldScore;

    // Start is called before the first frame update
    void Start()
    {
        scoreText = GetComponent<Text>();
        if(GManager.instance != null)
        {
            scoreText.text = "スコア " + GManager.instance.score;
        }
        else
        {
            Debug.Log("ゲームマネージャーが配置されていません");
        }
    }

    // Update is called once per frame
    void Update()
    {
        nowScore = GManager.instance.score;
        if(oldScore != nowScore)
        {
            scoreText.text = "スコア " + nowScore;
            oldScore = GManager.instance.score;
        }
    }
}
