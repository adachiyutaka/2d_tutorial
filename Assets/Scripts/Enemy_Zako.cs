using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Zako : MonoBehaviour
{
    [Header("加算スコア")]public int myScore;
    [Header("移動速度")]public float speed;
    [Header("重力")]public float gravity;
    [Header("画面外でも行動する")] public bool nonVisibleAct;
    [Header("接触判定")]public EnemyCollisionCheck checkCollision;
    [Header("やられた時に鳴らすSE")] public AudioClip deadSE;



     private Rigidbody2D rb = null;
     private SpriteRenderer sr = null;
     private Animator anim = null;
     private ObjectCollision oc = null;
    //  private BoxCollider2D col = null;
     private PolygonCollider2D col = null;
     private bool rightTleftF = false;
     private bool isDead = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>(); 
        anim = GetComponent<Animator>();
        oc = GetComponent<ObjectCollision>();
        col = GetComponent<PolygonCollider2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (oc.playerStepOn)
        {
            if (!isDead)
            {
                //踏まれたときの処理（一回目しか通過しない）
                //anim.Play("dead");
                if (GManager.instance != null)
                {
                    GManager.instance.score += myScore;
                    GManager.instance.PlaySE(deadSE);
                }
                rb.velocity = new Vector2(0, -gravity); 
                isDead = true;
                col.enabled = false;
                Destroy(gameObject,3f);
            }
            else
            {
                //踏まれた後の処理
                transform.Rotate(new Vector3(0,0,5));
            }
        }
        else
        {
            if (sr.isVisible || nonVisibleAct)
            {
                if (checkCollision.isOn)
                {
                    rightTleftF = !rightTleftF;
                }

                int xVector = -1;
                if (rightTleftF)
                {
                    xVector = 1;
                    transform.localScale = new Vector3(0.1f, 0.1f, 1);
                }
                else
                {
                    transform.localScale = new Vector3(-0.1f, 0.1f, 1);
                }
                rb.velocity = new Vector2(xVector * speed, -gravity);
            } 
            else
            {
                rb.Sleep();
            }
        }

    }
}
