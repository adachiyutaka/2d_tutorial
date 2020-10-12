using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("エフェクトがついた床を判定するか")] public bool checkPlatformGround = true;

    private string groundTag = "Ground";
    private string platformTag = "GroundPlatform";
    private bool isGround = false; 
    private bool isGroundEnter, isGroundStay, isGroundExit;

    //接地判定を返すメソッド
    //物理判定の更新毎に呼ぶ必要がある
    public bool IsGround()
    {
        if(isGroundEnter || isGroundStay)
        {
            isGround = true;
        }
        else if(isGroundExit)
        {
            isGround = false;
        } 

        isGroundEnter = false;
        isGroundStay = false;
        isGroundExit = false;
        return isGround;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
    if (collision.tag == groundTag)
        {
            isGroundEnter = true;
        }
        else if(checkPlatformGround && collision.tag == platformTag)
        {
            isGroundEnter = true;
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
    if (collision.tag == groundTag)
        {
            isGroundStay = true;
        }
        else if(checkPlatformGround && collision.tag == platformTag)
        {
            isGroundEnter = true;
        }
    }
        
    private void OnTriggerExit2D(Collider2D collision)
    {
    if (collision.tag == groundTag)
        {
            isGroundExit = true;
        }
        else if(checkPlatformGround && collision.tag == platformTag)
        {
            isGroundEnter = true;
        }
    }
}
