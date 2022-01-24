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
    [Header("ステージクリアーSE")] public AudioClip stageClearSE;
    [Header("ステージクリア")] public GameObject stageClearObj;
    [Header("ステージクリア判定")] public PlayerTriggerCheck stageClearTrigger;


    private Player p;
    private int nextStageNum;
    private bool startFade = false;
    private bool doGameOver = false;
    private bool retryGame = false;
    private bool doSceneChange = false;
    private bool doClear = false;
    private bool isInitialized = false;

    // Start is called before the first frame update
    // void Start()
    // {
    //     if(playerGO != null && continuePoint != null && continuePoint.Length > 0 && gameOverObj != null && stageClearObj != null && fade != null)
    //     {
    //         Debug.Log("StageCtrl");
    //         playerGO.transform.position = continuePoint[0].transform.position;
    //         gameOverObj.SetActive(false);
    //         stageClearObj.SetActive(false);
    //     }
    //     else
    //     {
    //         Debug.Log("必要な項目が設定されていません。");
    //     }
    // }

    // Update is called once per frame
    void Update()
    {   
        if(playerGO != null && continuePoint != null && continuePoint.Length > 0 && gameOverObj != null && stageClearObj != null && fade != null && GManager.instance.isImported && !isInitialized)
        {
            playerGO.transform.position = continuePoint[0].transform.position;
            gameOverObj.SetActive(false);
            stageClearObj.SetActive(false);
            isInitialized = true;
        }
        else if (!isInitialized)
        {
            Debug.Log($"GManager.instance.isImported: {GManager.instance.isImported}");
            Debug.Log("必要な項目が設定されていません。");
        }

        p = playerGO.GetComponent<Player>();

        // ゲームオーバー時の処理
        if(GManager.instance.isGameOver && !doGameOver)
        {
            gameOverObj.SetActive(true);
            GManager.instance.PlaySE(gameOverSE);
            doGameOver = true;
            GManager.instance.isGameOver = false;
        }

        // プレイヤーがやられた時の処理
        else if(p != null && p.IsContinueWaiting() && !doGameOver)
        {
            playerGO.transform.position = continuePoint[0].transform.position;
            p.ContinuePlayer();
        }

        // ステージクリア時の処理
        else if(stageClearTrigger != null && stageClearTrigger.isOn && !doGameOver && !doClear)
        {
            StageClear();
            doClear = true;
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
        isInitialized = false;
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

    public void StageClear()
    {
        GManager.instance.isStageClear = true;
        stageClearObj.SetActive(true);
        GManager.instance.PlaySE(stageClearSE);
    }
}