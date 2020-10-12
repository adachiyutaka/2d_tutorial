using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System;


public class ImageImport :  MonoBehaviour
{
    public JsTest jstest;

    //[SerializeField] private RawImage _image;
    void Start()
    {
        StartCoroutine(GetImage());
    }

    public class PostData
    {
        public int id;
    }

    IEnumerator GetImage()
    {
        WWWForm form = new WWWForm();
        string id = jstest.URI;
        //string url = $"http://localhost:3000/games/{id}/image/";
        string url = "http://localhost:3000/games/14/image/";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Content-Type", "application/json");
        Debug.Log("yield");
        //画像を取得できるまで待つ
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            //取得した画像のテクスチャ
            // byte[] results = request.downloadHandler.data;
            // CreateSpriteFromBytes(results);
            string json = request.downloadHandler.text;
            ImageJson imageJson = JsonUtility.FromJson<ImageJson>(json);
            Texture2D stageTexture = CreateTextureFromBytes(Convert.FromBase64String(imageJson.stage));
            Texture2D texture = CreateTextureFromBytes(Convert.FromBase64String(imageJson.player));

            // ステージを作成
            GameObject stage = new GameObject("Stage");
            stage.AddComponent<SpriteRenderer>();
            stage.GetComponent<SpriteRenderer>().sprite = Sprite.Create(stageTexture, new Rect(0, 0, stageTexture.width, stageTexture.height), new Vector2(0.5f, 0.5f));
            stage.AddComponent<PolygonCollider2D>();
            stage.tag = "Ground";

            // プレイヤーを作成
            GameObject player = GameObject.Find("Player");
            GameObject groundCheck = (GameObject)Resources.Load("GroundCheck");
            GameObject headCheck = (GameObject)Resources.Load("HeadCheck");
            SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            Vector2 startPosition = new Vector2(-10.0f, -3.0f);
            player.transform.position = startPosition;
            player.AddComponent<PolygonCollider2D>();
            player.AddComponent<Rigidbody2D>();
            player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            player.AddComponent<Player>();
            // New Spriteプレハブを元に、インスタンスを生成、
            // player = (GameObject)Instantiate(player, new Vector3(0.0f, 4.0f, 0.0f), Quaternion.identity);
            groundCheck = (GameObject)Instantiate(groundCheck, player.transform.position, Quaternion.identity);
            headCheck = (GameObject)Instantiate(headCheck, player.transform.position, Quaternion.identity);
            groundCheck.transform.parent = player.transform;
            headCheck.transform.parent = player.transform;
            player.GetComponent<Player>().ground = groundCheck.GetComponent<GroundCheck>();
            player.GetComponent<Player>().head = headCheck.GetComponent<GroundCheck>();
            player.GetComponent<Player>().head.checkPlatformGround = false;

            //return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        }
    }

    public Texture2D CreateTextureFromBytes(byte[] bytes)
    {
        //横サイズの判定
        int pos = 16;
        int width = 0;
        for (int i = 0; i < 4; i++)
        {
            width = width * 256 + bytes[pos++];
        }
        //縦サイズの判定
        int height = 0;
        for (int i = 0; i < 4; i++)
        {
            height = height * 256 + bytes[pos++];
        }

        //byteからTexture2D作成
        Texture2D texture = new Texture2D(width, height);
        texture.LoadImage(bytes);

        return texture;
    }

    
}

[Serializable]
public class ImageJson
{
    public string stage;
    public string player;
}