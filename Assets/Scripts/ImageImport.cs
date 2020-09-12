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
        string url = "http://localhost:3000/games/8/image/";
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
            Debug.Log("responsed");
            string json = request.downloadHandler.text;
            Debug.Log("json");
            ImageJson imageJson = JsonUtility.FromJson<ImageJson>(json);
            byte[] byteArray = Convert.FromBase64String(imageJson.base64);
            CreateTextureFromBytes(byteArray);
        }
    }

    public void CreateTextureFromBytes(byte[] bytes)
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
        GetComponent<Renderer>().material.mainTexture = texture;
        
        GameObject gameObject = GameObject.Find("Player");
        GameObject groundCheck = (GameObject)Resources.Load("GroundCheck");
        GameObject headCheck = (GameObject)Resources.Load("HeadCheck");
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        gameObject.AddComponent<PolygonCollider2D>();
        gameObject.AddComponent<Rigidbody2D>();
        gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        gameObject.AddComponent<Player>();
        // New Spriteプレハブを元に、インスタンスを生成、
        // gameObject = (GameObject)Instantiate(gameObject, new Vector3(0.0f, 4.0f, 0.0f), Quaternion.identity);
        groundCheck = (GameObject)Instantiate(groundCheck, gameObject.transform.position, Quaternion.identity);
        headCheck = (GameObject)Instantiate(headCheck, gameObject.transform.position, Quaternion.identity);
        groundCheck.transform.parent = gameObject.transform;
        headCheck.transform.parent = gameObject.transform;
        gameObject.GetComponent<Player>().ground = groundCheck.GetComponent<GroundCheck>();
        gameObject.GetComponent<Player>().head = headCheck.GetComponent<GroundCheck>();

        //return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    // byte配列 → イメージ(Texture)
    Texture BytesToTexture(byte[] byteArray, int width, int height) {
        Texture2D texture = new Texture2D(width, height);
        texture.LoadImage(byteArray);
        return texture;
    }

    // Base64 → イメージ(Texture)
    Texture Base64ToImage(string base64Text, int width, int height) {
        byte[] byteArray = Convert.FromBase64String(base64Text);
        return BytesToTexture(byteArray, width, height);
    }
}

[Serializable]
public class ImageJson
{
    public string stage;
    public string player;
}