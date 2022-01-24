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
    [Header("ジャンプのSE")] public AudioClip jumpSE;
    [Header("やられたときのSE")] public AudioClip downSE;
    [Header("コンティニューのSE")] public AudioClip continueSE;
    [Header("横幅")] public float width;
    [Header("高さ")] public float height;

    //  TODO:もっと綺麗な形に変更

    static Keyframe dStartKeyframe = new Keyframe(0.0f, 0.0f);
    static Keyframe dEndKeyframe = new Keyframe(0.5f, 1.0f);
    static Keyframe jStartKeyframe = new Keyframe(0.0f, 1.0f);
    static Keyframe jEndKeyframe = new Keyframe(1.3f, 0.5f);
    [Header("ダッシュの速さ表現")] public AnimationCurve dashCurve = new AnimationCurve(dStartKeyframe, dEndKeyframe);
    [Header("ジャンプの速さ表現")] public AnimationCurve jumpCurve = new AnimationCurve(jStartKeyframe, jEndKeyframe);
    [Header("踏みつけ判定の高さの割合(%)")] public float stepOnRate = 10;

    // TODO:Animatorを設定
    //private Animator anim = null;
    private Rigidbody2D rb = null;

    //  TODO:変数名を変更
    private PolygonCollider2D polycol2d = null;
    private MoveObject moveObj = null;
    private bool isGround = false;
    private bool isHead = false;
    private bool isRun = false;
    private bool isJump = false;
    private bool isDown = false; 
    private bool isOtherJump = false;
    private bool isClearMotion = false;
    private float jumpPos = 0.0f;
    private float otherJumpHeight = 0.0f;
    private float dashTime,jumpTime;
    private float beforeKey;
    private string enemyTag = "Enemy";
    private string moveFloorTag = "MoveFloor";
    private string hitAreaTag = "HitArea";
    private float downTime = 0.0f;
    private bool isContinue = false; 
    private float continueTime = 0.0f;
    private float blinkTime = 0.0f; 
    private SpriteRenderer spriteRenderer = null;

    // private Animator anim = null; 


    // Start is called before the first frame update
    void Start()
    {
        //anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        polycol2d = GetComponent<PolygonCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        // anim = GetComponent<Animator>();            rb.velocity = new Vector2 (0.0f, 7.0f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // ダウンしている場合
        if (isDown && GManager.instance.isGameOver)
        {
            rb.velocity = new Vector2(0, -gravity);
        }
        // ゲームクリアーの場合
        else if(GManager.instance.isStageClear)
        {
            rb.velocity = new Vector2(0, -gravity);
        }
        // 操作を受け付けている状態
        else
        {
            //接地判定を得る
            isGround = ground.IsGround();
            isHead = head.IsGround();
    
            //各種座標軸の速度を求める
            float xSpeed = GetXSpeed();
            float ySpeed = GetYSpeed();

            //動く床の移動速度を設定
            Vector2 addVelocity = Vector2.zero;
            if (moveObj != null)
            {
                addVelocity = moveObj.GetVelocity();
            }
            rb.velocity = new Vector2(xSpeed, ySpeed) + addVelocity;
        }

        SetAnimation();
    }

    private float GetXSpeed()
    {
        float horizontalKey = Input.GetAxis("Horizontal");
        float xSpeed = 0.0f;
        if (horizontalKey > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            isRun = true;
            dashTime += Time.deltaTime;
            xSpeed = speed;
        }
        else if (horizontalKey < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
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
        float ySpeed = -gravity;

        //通常のジャンプ
        if (isGround)
        {
            if (verticalKey > 0)
            {
                if(!isJump)
                {
                    GManager.instance.PlaySE(jumpSE);
                }
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

            if (pushUpKey && canHeight && canTime && !isHead)
            {
                ySpeed = jumpSpeed;
                jumpTime += Time.deltaTime;
            }
            else
            {
                isJump = false;
                jumpTime = 0.0f;
            }

            // if (pushUpKey && !isHead)
            // {
            //     ySpeed = rb.velocity.y;
            //     jumpTime += Time.deltaTime;
            // }
            // else
            // {
            //     isJump = false;
            //     jumpTime = 0.0f;
            // }
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
            ySpeed *= jumpCurve.Evaluate(jumpTime);
        }

        return ySpeed;
    }

    private void Update()
    {  
        if (isContinue)
        {
            //明滅 ついている時に戻る
            if(blinkTime > 0.2f)
            {
                spriteRenderer.enabled = true;
                blinkTime = 0.0f;
            }
            //明滅 消えているとき
            else if (blinkTime > 0.1f)
            {
                spriteRenderer.enabled = false;
            }
            //明滅 ついているとき
            else
            {
                spriteRenderer.enabled = true;
            }

            //1秒たったら明滅終わり
            if(continueTime > 1.0f)
            {
                isContinue = false;
                blinkTime = 0f;
                continueTime = 0f;
                spriteRenderer.enabled = true;
            }
            else
            {
                blinkTime += Time.deltaTime;
                continueTime += Time.deltaTime;
            }
        }
    }

    // private void SetAnimation()
    // {
    //     anim.SetBool("jump", isJump);
    //     anim.SetBool("ground", isGround);
    //     anim.SetBool("run", isRun);
    // }
    private void SetAnimation()
    {
        Vector3 localAngle = transform.localEulerAngles;
        if(isDown)
        {   
            if(localAngle.z < 90.0f)
            transform.Rotate(new Vector3(0, 0, 5));
        }
    }

    //敵との接触判定
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == enemyTag)
        {
            //踏みつけ判定になる高さ
            float stepOnHeight = (height * (stepOnRate / 100f));

            //踏みつけ判定のワールド座標
            float judgePos = transform.position.y - (height / 2f) + stepOnHeight;

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
                    ReceiveDamage();
                    break;
                }
            }
        }
        //動く床
        else if (collision.collider.tag == moveFloorTag)
        {
            //踏みつけ判定になる高さ
            float stepOnHeight = (1 * (stepOnRate / 100f));
            //踏みつけ判定のワールド座標
            float judgePos = transform.position.y - (1 / 2f) + stepOnHeight;
            foreach (ContactPoint2D p in collision.contacts)
            {
                //動く床に乗っている
                if (p.point.y < judgePos)
                {
                    moveObj = collision.gameObject.GetComponent<MoveObject>();
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.tag == moveFloorTag)
        {
            //動く床から離れた
            moveObj = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == hitAreaTag)
        {
            ReceiveDamage();
        }
    }

    private void ReceiveDamage()
    {
        if(isDown)
        {
            return;
        }
        isDown = true;
        GManager.instance.PlaySE(downSE);
        GManager.instance.SubHeartNum();
    }

    public bool IsContinueWaiting()
    {
    return IsDownAnimEnd();
    }

    private bool IsDownAnimEnd()
    {
        if(isDown)
        {
            downTime += Time.deltaTime;
            if(downTime > 0.3f)
            {
                downTime = 0;
                return true;
            }
        }
        return false;
    }

    public void ContinuePlayer()
    {
        GManager.instance.PlaySE(continueSE);
        transform.eulerAngles = new Vector3(0, 0, 0);
        polycol2d.enabled = true;
        isDown = false;
        isJump = false;
        isOtherJump = false;
        isRun = false;
        isContinue = true;
    }
}