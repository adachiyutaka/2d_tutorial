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
        string url = "http://localhost:3000/games/46/image/";
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

            // デバッグ用 imageアクションから送られてきたデータを一括表示
            Debug.Log("yield");
            Debug.Log($"stage.width: {game.stage.width}, stage.height{game.stage.height}");

            foreach (var obj in game.objects)
            {
                Debug.Log($"symbol:{obj.symbol}");
                Debug.Log($"image:{obj.image}");
                Debug.Log($"isObject:{obj.isObject}, isPlayer:{obj.isPlayer}, isEnemy:{obj.isEnemy}");
            }
            foreach (var position in game.positions)
            {
                Debug.Log($"symbol:{position.symbol}");
                Debug.Log($"height:{position.height}, width:{position.width}, x:{position.y}, x:{position.y}");
            }
            foreach (var objectPosition in game.objectPositions)
            {
                Debug.Log($"objectId:{objectPosition.objectId}, positionId:{objectPosition.positionId}");
            }
            // デバッグ用

            foreach (var objPos in game.objectPositions)
            {
                // objectPositionに紐付いたobjectとpositionを参照
                ObjectJson obj = game.objects[objPos.objectId];
                PositionJson pos = game.positions[objPos.positionId];

                // GameObjectに役割を与える
                if (obj.isObject)
                {
                    // Objectの場合
                    // GameObjectを作成し、スプライトを登録
                    GameObject go = new GameObject();
                    CreateSprite(go, objPos, game);
                    // タグに"Ground"を指定
                    go.tag = "Ground";
                }
                else if (obj.isPlayer)
                {
                    // Playerの場合
                    // プレイヤー、接地判定のGameObjectを作成し、スプライトを登録
                    GameObject player = GameObject.Find("Player");
                    GameObject groundCheck = (GameObject)Resources.Load("GroundCheck");
                    GameObject headCheck = (GameObject)Resources.Load("HeadCheck");
                    CreateSprite(player, objPos, game);
                    // Playerスクリプトを設定
                    player.AddComponent<Player>();
                    // Rigidbody2D、回転の無効化を設定
                    player.AddComponent<Rigidbody2D>();
                    player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
                    // 足元と頭の接地判定を設定
                    //  pixel単位のheightをそのまま使うと大きすぎるので、100分の一にする
                    Vector3 playerPos = player.transform.position;
                    Vector3 groundPos = new Vector3(playerPos.x, playerPos.y - (float)pos.height/200, playerPos.z);
                    Vector3 headPos = new Vector3(playerPos.x, playerPos.y + (float)pos.height/200, playerPos.z);
                    //  判定用のGameObjectを生成する
                    groundCheck = (GameObject)Instantiate(groundCheck, groundPos, Quaternion.identity);
                    headCheck = (GameObject)Instantiate(headCheck, headPos, Quaternion.identity);
                    //  判定用GameObjectの横幅を設定し、プレイヤーの位置と親子関係にする
                    groundCheck.transform.localScale = new Vector2((float)pos.width/100, 1);
                    headCheck.transform.localScale = new Vector2((float)pos.width/100, 1);
                    groundCheck.transform.parent = player.transform;
                    headCheck.transform.parent = player.transform;
                    //  プレイヤーのコンポーネントに設定する
                    player.GetComponent<Player>().ground = groundCheck.GetComponent<GroundCheck>();
                    player.GetComponent<Player>().head = headCheck.GetComponent<GroundCheck>();
                    player.GetComponent<Player>().head.checkPlatformGround = false;
                } else if (obj.isEnemy){
                    // Enemyの場合
                    // 敵キャラクター、接触判定のGameObjectを作成し、スプライトを登録
                    GameObject enemy = new GameObject();
                    GameObject enemyColCheck = (GameObject)Resources.Load("enemyCollisionCheck");
                    CreateSprite(enemy, objPos, game);
                    // Enemy_zako, ObjectCollisionスクリプトを設定
                    enemy.AddComponent<Enemy_Zako>();
                    enemy.AddComponent<ObjectCollision>();
                    // Rigidbody2Dと回転の無効化をの設定
                    enemy.AddComponent<Rigidbody2D>();
                    enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
                    // 足元と頭の接地判定を設定
                    //  pixel単位のheightをそのまま使うと大きすぎるので、100分の一にする
                    Vector3 enemyPos = enemy.transform.position;
                    Vector3 frontPos = new Vector3(enemyPos.x + (float)pos.width/200, enemyPos.y, enemyPos.z);
                    //  判定用のGameObjectを生成する
                    enemyColCheck = (GameObject)Instantiate(enemyColCheck, frontPos, Quaternion.identity);
                    //  判定用GameObjectの横幅を設定し、プレイヤーの位置と親子関係にする
                    enemyColCheck.transform.localScale = new Vector2(1, (float)pos.height/110);
                    enemyColCheck.transform.parent = enemy.transform;
                    //  敵キャラクターのコンポーネントに設定する
                    enemy.GetComponent<Enemy_Zako>().checkCollision = enemyColCheck.GetComponent<EnemyCollisionCheck>();
                    // タグを設定する
                    enemy.tag = "Enemy";
                } else if (obj.isItem){
                    // Itemの場合
                    // GameObjectを作成し、スプライトを登録
                    GameObject item = new GameObject();
                    CreateSprite(item, objPos, game);
                    item.AddComponent<ScoreItem>();
                    item.GetComponent<ScoreItem>().myScore = 10;
                    item.AddComponent<PlayerTriggerCheck>();
                    item.GetComponent<ScoreItem>().playerCheck = item.GetComponent<PlayerTriggerCheck>();
                    item.GetComponent<PolygonCollider2D>().isTrigger = true;
                    // タグに"Item"を指定
                    item.tag = "Item";
                } else if (obj.isGoal){
                    // Goalの場合
                    // GameObjectを作成し、スプライトを登録
                    GameObject goal = new GameObject();
                    CreateSprite(goal, objPos, game);
                    goal.AddComponent<PlayerTriggerCheck>();
                    // TODO: ゴール演出を表示するGOを作成し、その表示トリガーにgoalのPlayerTriggerCheckを使用する


                    goal.GetComponent<PolygonCollider2D>().isTrigger = true;
                    // タグに"Goal"を指定
                    goal.tag = "Goal";
                }
            }
        }
    }
    
    private void CreateSprite(GameObject go, ObjectPositionJson objPos, GameJson game)
    {
        // objPosに紐づけられたobj, posインスタンスを取得する
        ObjectJson obj = game.objects[objPos.objectId];
        PositionJson pos = game.positions[objPos.positionId];
        // 画像からテクスチャを生成し、SpriteRendererに登録する
        Texture2D texture = CreateTextureFromBytes(Convert.FromBase64String(obj.image));
        go.AddComponent<SpriteRenderer>();
        go.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, 
                                                                 new Rect(0.0f, 0.0f, pos.width, pos.height), 
                                                                 new Vector2(0.5f, 0.5f));
        // Colliderを設定する
        go.AddComponent<PolygonCollider2D>();
        // オブジェクトの初期位置を設定する
        //  pixel単位のheightをそのまま使うと大きすぎるので、100分の一にする
        go.transform.position = new Vector2((float)(pos.x + pos.width / 2)/100, (float)(game.stage.height - pos.y - pos.height / 2)/100);
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
    public bool isEnemy;
    public bool isItem;
    public bool isGoal;
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