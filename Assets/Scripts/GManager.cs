using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GManager : MonoBehaviour
{
    [Header("デフォルトの残機")] public int defaultLife;
    public static GManager instance = null;
    public int score;
    public int life;
    public int stageNum;
    public int continueNum;
    [HideInInspector] public bool isGameOver = false;
    [HideInInspector] public bool isStageClear = false;
    private AudioSource audioSource = null;

    private void Awake()
    {
      if (instance == null)
      {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
      }
      else
      {
        Destroy(this.gameObject);
      }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Debug.Log($"audioSource: {audioSource}");
    }

    public void AddHeartNum()
    {
        if(life < 99)
        {
            ++life;
        }
    }
    
    public void SubHeartNum()
    {
        if(life > 0)
        {
            --life;
        }
        else
        {
            isGameOver = true;
        }
    }

    public void RetryGame()
    {
        isGameOver = false;
        life = defaultLife;
        score = 0;
        stageNum = 1;
    }

    // Audio関連
    public void PlaySE(AudioClip clip)
    {
        if(audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.Log("AudioSourceが設定されていません。");
        }
    }
}
