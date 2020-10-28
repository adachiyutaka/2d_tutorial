using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageCtrl : MonoBehaviour
{
    [Header("PlayerのGameObject")] public GameObject playerGO;
    [Header("コンティニュー位置")] public GameObject[] continuePoint;
    [Header("ゲームオーバー")] public GameObject gameOverObj;
    [Header("フェード")] public FadeScreen fade;
    [Header("ゲームオーバーのSE")] public AudioClip gameOverSE;
    [Header("リトライのSE")] public AudioClip retrySE;


    private Player p;
    private int nextStageNum;
    private bool startFade = false;
    private bool doGameOver = false;
    private bool retryGame = false;
    private bool doSceneChange = false;

    // Start is called before the first frame update
    void Start()
    {
        if(playerGO != null && continuePoint != null && continuePoint.Length > 0 && gameOverObj != null && fade != null)
        {
            playerGO.transform.position = continuePoint[0].transform.position;
            gameOverObj.SetActive(false);
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

        // ゲームオーバー時の処理
        if(GManager.instance.isGameOver && !doGameOver)
        {
            gameOverObj.SetActive(true);
            GManager.instance.PlaySE(gameOverSE);
            doGameOver = true;
        }

        // プレイヤーがやられた時の処理
        if(p != null && p.IsContinueWaiting())
        {
            playerGO.transform.position = continuePoint[0].transform.position;
            p.ContinuePlayer();
        }

        if(fade != null && startFade && !doSceneChange)
        {
            if(fade.IsFadeOutComplete())
            {
                // ゲームリトライ
                if(retryGame)
                {
                    GManager.instance.RetryGame();
                }
                // 次のステージ
                else
                {
                    GManager.instance.stageNum = nextStageNum;
                }
                SceneManager.LoadScene("stage" + nextStageNum);
                doSceneChange = true;
            }
        }
    }

    public void Retry()
    {
        GManager.instance.PlaySE(retrySE);
        ChangeScene(1);
        retryGame = true;
    }

    public void ChangeScene(int num)
    {
        if(fade != null)
        {
            nextStageNum = num;
            fade.StartFadeOut();
            startFade = true;
        }
    }
}