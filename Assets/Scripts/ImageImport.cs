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
        string url = "http://localhost:3000/games/9/image/";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Content-Type", "application/json");
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
            string jsonText = request.downloadHandler.text;
            GameJson game = JsonUtility.FromJson<GameJson>(jsonText);
            Debug.Log("yield");

            Debug.Log(game.stage.width);
            Debug.Log(game.stage.height);

            foreach (var obj in game.objects)
            {
                Debug.Log(obj.symbol);
                Debug.Log(obj.image);
                Debug.Log(obj.isObject);
                Debug.Log(obj.isPlayer);
            }
            foreach (var position in game.positions)
            {
                Debug.Log(position.symbol);
                Debug.Log(position.height);
                Debug.Log(position.width);
                Debug.Log(position.x);
                Debug.Log(position.y);
            }
            foreach (var objectPosition in game.objectPositions)
            {
                Debug.Log(objectPosition.objectId);
                Debug.Log(objectPosition.positionId);
            }
            
            // Texture2D stageTexture = CreateTextureFromBytes(Convert.FromBase64String(game.objects[0].image));

            // Texture2D playerTexture = CreateTextureFromBytes(Convert.FromBase64String(imageJson.player));
            // Texture2D objectTexture = CreateTextureFromBytes(Convert.FromBase64String(imageJson.gameObject));

            foreach (var objPos in game.objectPositions)
            {
                // objectPositionに紐付いたobjectとpositionを参照
                ObjectJson obj = game.objects[objPos.objectId];
                PositionJson pos = game.positions[objPos.positionId];
                // GameObjectの役割を与える
                if (obj.isObject)
                {
                    // GameObjectを作成し、スプライトを登録
                    GameObject go = new GameObject();
                    CreateSprite(go, objPos, game);
                    // タグに"Ground"を指定
                    go.tag = "Ground";
                }
                else if (obj.isPlayer)
                {
                    // プレイヤー、接地判定のGameObjectを作成し、スプライトを登録
                    GameObject player = GameObject.Find("Player");
                    GameObject groundCheck = (GameObject)Resources.Load("GroundCheck");
                    GameObject headCheck = (GameObject)Resources.Load("HeadCheck");
                    CreateSprite(player, objPos, game);
                    // Playerスクリプトを設定
                    player.AddComponent<Player>();
                    // Colliderを設定
                    player.AddComponent<Rigidbody2D>();
                    player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
                    // 足元と頭の接地判定を設定
                    Vector3 playerPos = player.transform.position;
                    Debug.Log($"playerPos.y: {playerPos.y}, playerPos.x: {playerPos.x}, pos.height->/200: {(float)pos.height/200} -> {(float)pos.height/200}, pos.width: {pos.width}, pos.x: {pos.x}, pos.y: {pos.y}, pos.x/100: {(float)pos.x/100}, pos.y/10: {pos.y/10f}");
                    Vector3 groundPos = new Vector3(playerPos.x, playerPos.y - (float)pos.height/200, playerPos.z);
                    Vector3 headPos = new Vector3(playerPos.x, playerPos.y + (float)pos.height/200, playerPos.z);
                    groundCheck = (GameObject)Instantiate(groundCheck, groundPos, Quaternion.identity);
                    headCheck = (GameObject)Instantiate(headCheck, headPos, Quaternion.identity);
                    groundCheck.transform.localScale = new Vector2((float)pos.width/100, 1);
                    headCheck.transform.localScale = new Vector2((float)pos.width/100, 1);
                    groundCheck.transform.parent = player.transform;
                    headCheck.transform.parent = player.transform;
                    player.GetComponent<Player>().ground = groundCheck.GetComponent<GroundCheck>();
                    player.GetComponent<Player>().head = headCheck.GetComponent<GroundCheck>();
                    player.GetComponent<Player>().head.checkPlatformGround = false;
                }
            }

            // ステージを作成
            // GameObject stage = new GameObject("Stage");
            // stage.AddComponent<SpriteRenderer>();
            // stage.GetComponent<SpriteRenderer>().sprite = Sprite.Create(stageTexture, new Rect(0, 0, stageTexture.width, stageTexture.height), new Vector2(0.5f, 0.5f));
            // stage.AddComponent<PolygonCollider2D>();
            // stage.tag = "Ground";

            // プレイヤーを作成
            // GameObject player = GameObject.Find("Player");
            // GameObject groundCheck = (GameObject)Resources.Load("GroundCheck");
            // GameObject headCheck = (GameObject)Resources.Load("HeadCheck");
            // SpriteRenderer playerSR = player.GetComponent<SpriteRenderer>();
            // playerSR.sprite = Sprite.Create(playerTexture, new Rect(0, 0, playerTexture.width, playerTexture.height), new Vector2(0.5f, 0.5f));
            // Vector2 startPosition = new Vector2(-10.0f, -3.0f);
            // player.transform.position = startPosition;
            // player.AddComponent<PolygonCollider2D>();
            // player.AddComponent<Rigidbody2D>();
            // player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            // player.AddComponent<Player>();
            // // New Spriteプレハブを元に、インスタンスを生成、
            // // player = (GameObject)Instantiate(player, new Vector3(0.0f, 4.0f, 0.0f), Quaternion.identity);
            // groundCheck = (GameObject)Instantiate(groundCheck, player.transform.position, Quaternion.identity);
            // headCheck = (GameObject)Instantiate(headCheck, player.transform.position, Quaternion.identity);
            // groundCheck.transform.parent = player.transform;
            // headCheck.transform.parent = player.transform;
            // player.GetComponent<Player>().ground = groundCheck.GetComponent<GroundCheck>();
            // player.GetComponent<Player>().head = headCheck.GetComponent<GroundCheck>();
            // player.GetComponent<Player>().head.checkPlatformGround = false;

            //return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            //敵キャラクターを作成
            // GameObject enemy = (GameObject)Resources.Load("Enemy");
            // GameObject enemyColCheck = (GameObject)Resources.Load("enemyCollisionCheck");
            // enemy = (GameObject)Instantiate(enemy, new Vector3(-5.0f,-3.0f,0.0f), Quaternion.identity);
            // enemyColCheck = (GameObject)Instantiate(enemyColCheck, enemy.transform.position, Quaternion.identity);
            // enemyColCheck.transform.parent = enemy.transform;
            // Debug.Log(enemy.GetComponent<Enemy_Zako>().checkCollision);
            // enemy.GetComponent<Enemy_Zako>().checkCollision = enemyColCheck.GetComponent<EnemyCollisionCheck>();
            // Debug.Log(enemy.GetComponent<Enemy_Zako>().checkCollision);
            // enemy.AddComponent<SpriteRenderer>();
            // SpriteRenderer enemySR = enemy.GetComponent<SpriteRenderer>();
            // enemySR.sprite = Sprite.Create(objectTexture, new Rect(0, 0, objectTexture.width, objectTexture.height), new Vector2(0.5f, 0.5f));
            // enemy.AddComponent<PolygonCollider2D>();
            // enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
    
    private void CreateSprite(GameObject go, ObjectPositionJson objPos, GameJson game)
    {
        ObjectJson obj = game.objects[objPos.objectId];
        PositionJson pos = game.positions[objPos.positionId];
        go.AddComponent<SpriteRenderer>();
        Texture2D texture = CreateTextureFromBytes(Convert.FromBase64String(obj.image));
        go.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, 
                                                                 new Rect(0.0f, 0.0f, pos.width, pos.height), 
                                                                 new Vector2(0.5f, 0.5f));
        go.AddComponent<PolygonCollider2D>();
        go.transform.position = new Vector2((float)(pos.x + pos.width / 2)/100, (float)(pos.y + pos.width / 2)/100);
    }

    private Texture2D CreateTextureFromBytes(byte[] bytes)
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
public class GameJson
{
    public StageJson stage;
    public ObjectJson[] objects;
    public PositionJson[] positions;
    public ObjectPositionJson[] objectPositions;
}

[Serializable]
public class StageJson
{
    public int width;
    public int height;
}

[Serializable]
public class ObjectJson
{
    public string symbol;
    public string image;
    public bool isObject;
    public bool isPlayer;
}

[Serializable]
public class PositionJson
{
    public string symbol;
    // x, yはcanvas左上が基準、→↓方向が正の値（Unityの座標とは異なるので注意）
    public int x;
    public int y;
    public int height;
    public int width;
}

[Serializable]
public class ObjectPositionJson
{
    public int objectId;
    public int positionId;
}