using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Life : MonoBehaviour
{
    private Text lifeText = null;
    private int nowLife;
    private int oldLife;

    // Start is called before the first frame update
    void Start()
    {
        lifeText = GetComponent<Text>();
        if(GManager.instance != null)
        {
            lifeText.text = "❤︎ × " + GManager.instance.life;
        }
        else
        {
            Debug.Log("ゲームマネージャーが配置されていません");
        }
    }

    // Update is called once per frame
    void Update()
    {
        nowLife = GManager.instance.life;
        if(oldLife != nowLife)
        {
            lifeText.text = "❤︎ × " + nowLife;
            oldLife = GManager.instance.score;
        }
    }
}
