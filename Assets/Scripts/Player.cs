using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("移動速度")] public float speed = 7;
    [Header("重力")] public float gravity = 7;
    [Header("ジャンプ速度")] public float jumpSpeed = 7;
    [Header("ジャンプする高さ")] public float jumpHeight = 3;
    [Header("ジャンプする長さ")] public float jumpLimitTime = 5;
    [Header("接地判定")] public GroundCheck ground;
    [Header("天井判定")] public GroundCheck head;

    //  TODO:もっと綺麗な形に変更

    static Keyframe dStartKeyframe = new Keyframe(0.0f, 0.0f);
    static Keyframe dEndKeyframe = new Keyframe(0.5f, 1.0f);
    static Keyframe jStartKeyframe = new Keyframe(0.0f, 1.0f);
    static Keyframe jEndKeyframe = new Keyframe(1.3f, 0.5f);
    [Header("ダッシュの速さ表現")] public AnimationCurve dashCurve = new AnimationCurve(dStartKeyframe, dEndKeyframe);
    [Header("ジャンプの速さ表現")] public AnimationCurve jumpCurve = new AnimationCurve(jStartKeyframe, jEndKeyframe);
    [Header("踏みつけ判定の高さの割合(%)")] public float stepOnRate = 20;

    // TODO:Animatorを設定
    //private Animator anim = null;
    private Rigidbody2D rb = null;

    //  TODO:変数名を変更
    private PolygonCollider2D polycol2d = null;
    private bool isGround = false;
    private bool isHead = false;
    private bool isRun = false;
    private bool isJump = false;
    private bool isDown = false; 
    private bool isOtherJump = false;
    private float jumpPos = 0.0f;
    private float otherJumpHeight = 0.0f;
    private float dashTime,jumpTime;
    private float beforeKey;
    private string enemyTag = "Enemy";

    // private Animator anim = null; 


    // Start is called before the first frame update
    void Start()
    {
        //anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        polycol2d = GetComponent<PolygonCollider2D>();
        // anim = GetComponent<Animator>();            rb.velocity = new Vector2 (0.0f, 7.0f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isDown)
        {
            //接地判定を得る
            isGround = ground.IsGround();
            isHead = head.IsGround(); 
    
            //各種座標軸の速度を求める
            float xSpeed = GetXSpeed();
            float ySpeed = GetYSpeed();

            // TODO:Animationを設定
            //アニメーションを適用
            //SetAnimation();

            //移動速度を設定
            rb.velocity = new Vector2(xSpeed, ySpeed);
        }
        else
        {
            rb.velocity = new Vector2(0, -gravity);
        }
    }

    private float GetXSpeed()
    {
        float horizontalKey = Input.GetAxis("Horizontal");
        float xSpeed = 0.0f;
        if (horizontalKey > 0)
        {
            transform.localScale = new Vector3(0.3f, 0.3f, 1);
            isRun = true;
            dashTime += Time.deltaTime;
            xSpeed = speed;
        }
        else if (horizontalKey < 0)
        {
            transform.localScale = new Vector3(-0.3f, 0.3f, 1);
            isRun = true;
            dashTime += Time.deltaTime;
            xSpeed = -speed;
        }
        else
        {
            isRun = false;
            xSpeed = 0.0f;
            dashTime = 0.0f;
        }

        //前回の入力からダッシュの反転を判断して速度を変える
        if (horizontalKey > 0 && beforeKey < 0)
        {
            dashTime = 0.0f;
        }
        else if (horizontalKey < 0 && beforeKey > 0)
        {
            dashTime = 0.0f;
        }

        beforeKey = horizontalKey;
        xSpeed *= dashCurve.Evaluate(dashTime);
        return xSpeed;
    }
    private float GetYSpeed()
    {
        float verticalKey = Input.GetAxis("Vertical");
        float ySpeed = 0.0f;

        //通常のジャンプ
        if (isGround)
        {
            if (verticalKey > 0)
            {
                ySpeed = jumpSpeed;
                jumpPos = transform.position.y; //ジャンプした位置を記録する
                isJump = true;
                jumpTime = 0.0f;
            }
            else
            {
                isJump = false;
            }
        }
        else if (isJump)
        {
            //上方向キーを押しているか
            bool pushUpKey = verticalKey > 0;
            //現在の高さが飛べる高さより下か
            bool canHeight = jumpPos + jumpHeight > transform.position.y;
            //ジャンプ時間が長くなりすぎてないか
            bool canTime = jumpLimitTime > jumpTime;

            // if (pushUpKey && canHeight && canTime && !isHead)
            // {
            //     ySpeed = jumpSpeed;
            //     jumpTime += Time.deltaTime;
            // }
            // else
            // {
            //     isJump = false;
            //     jumpTime = 0.0f;
            // }

            if (pushUpKey && !isHead)
            {
                ySpeed = rb.velocity.y;
                jumpTime += Time.deltaTime;
            }
            else
            {
                isJump = false;
                jumpTime = 0.0f;
            }
        }
        //何かを踏んだときのジャンプ
        else if (isOtherJump)
        {
            //現在の高さが飛べる高さより下か
            bool canHeight = jumpPos + otherJumpHeight > transform.position.y;
            //ジャンプ時間が長くなりすぎてないか
            bool canTime = jumpLimitTime > jumpTime;

            if (canHeight && canTime && !isHead)
            {
                ySpeed = jumpSpeed;
                jumpTime += Time.deltaTime;
            }
            else
            {
                isOtherJump = false;
                jumpTime = 0.0f;
            }
        }

        if (isJump || isOtherJump)
        {
            // ySpeed *= jumpCurve.Evaluate(jumpTime);
        }

        return ySpeed;
    }

    // private void SetAnimation()
    // {
    //     anim.SetBool("jump", isJump);
    //     anim.SetBool("ground", isGround);
    //     anim.SetBool("run", isRun);
    // }

    //敵との接触判定
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == enemyTag)
        {
            //踏みつけ判定になる高さ
            //float stepOnHeight = (capcol.size.y * (stepOnRate / 100f));

            // TODO:capcol.sizeの代わりになるものを探す
            float stepOnHeight = (1 * (stepOnRate / 100f));

            //踏みつけ判定のワールド座標
            //float judgePos = transform.position.y - (capcol.size.y / 2f) + stepOnHeight;

            // TODO:capcol.sizeの代わりになるものを探す
            float judgePos = transform.position.y - (1 / 2f) + stepOnHeight;

            foreach (ContactPoint2D p in collision.contacts)
            {
                if (p.point.y < judgePos)
                {
                    ObjectCollision o = collision.gameObject.GetComponent<ObjectCollision>();
                    if (o != null)
                    {
                        otherJumpHeight = o.boundHeight;    //踏んづけたものから跳ねる高さを取得する
                        o.playerStepOn = true;        //踏んづけたものに対して踏んづけた事を通知する
                        jumpPos = transform.position.y; //ジャンプした位置を記録する 
                        isOtherJump = true;
                        isJump = false;
                        jumpTime = 0.0f;
                    }
                    else
                    {
                        Debug.Log("ObjectCollisionが付いてないよ!");
                    }
                }
                else
                {
                    //anim.Play("down");
                    isDown = true;
                    break;
                }
            }
        }
    }
}
