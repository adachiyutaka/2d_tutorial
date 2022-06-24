using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System;
using Unity.Collections;
using System.Linq;
using LitJson;


public class ImageImport :  MonoBehaviour
{
    public JsTest jstest;
    public GameObject stageCtrl;
    private  Dictionary<string, int> role;
    //[SerializeField] private RawImage _image;
    void Start()
    {
        stageCtrl = GameObject.Find("StageCtrl");
        StartCoroutine(GetImage());
    }

    public class PostData
    {
        public int id;
    }

    IEnumerator GetImage()
    {
        role = new Dictionary<string, int>(){
            {"stage", 0},
            {"player", 1},
            {"enemy", 2},
            {"item", 3},
            {"goal", 4},
        };

        WWWForm form = new WWWForm();
        // string id = jstest.URI;
        //string url = $"http://localhost:3000/games/{id}/unity/";
        string url = "http://localhost:3000/games/3/unity/";
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
            string jsonStr = request.downloadHandler.text;
            // GameJson game = JsonUtility.FromJson<GameJson>(jsonStr);
            GameJson game = JsonUtility.FromJson<GameJson>(jsonStr);
            GameJson gameData = JsonMapper.ToObject<GameJson>(jsonStr);

            // デバッグ用 unityアクションから送られてきたデータを一括表示
            Debug.Log("yield");
            Debug.Log($"jsonStr {jsonStr}");
            // Debug.Log($"stage.width: {game.stage.width}, stage.height{game.stage.height}");
            Debug.Log($"gameData {gameData.objects[0].role}");

            foreach (var obj in gameData.objects)
            {
                var meshData = obj.meshData;
                Debug.Log($"meshData.triangles[0]:{obj.meshData.triangles[0]}");
                Debug.Log($"symbol:{obj.symbol}");
                Debug.Log($"image:{obj.image}");
                Debug.Log($"role:{obj.role}");
                Debug.Log($"meshData:{meshData}");
                Debug.Log($"meshData.triangles[0]:{obj.meshData.triangles[0]}");

                foreach(var vertex in meshData.vertices)
                {
                    Debug.Log($"vertex:{vertex.x}, {vertex.y}");
                }
                foreach(var triangle in meshData.triangles)
                {
                    Debug.Log($"triangle:{triangle}");
                }
                Debug.Log($"boneNames-------------");
                Debug.Log($"boneNamesOnVertices GetType() {meshData.boneNamesOnVertices}");

                Debug.Log($"boneName [0][0] {meshData.boneNamesOnVertices[0][0]}");

                foreach(var boneNames in meshData.boneNamesOnVertices)
                {
                    Debug.Log($"boneNames:{boneNames[0]}");
                    string boneNamesString = "";
                    foreach(var boneName in boneNames)
                    {
                        boneNamesString += $"{boneName}, ";
                    }
                    Debug.Log($"triangle:{boneNamesString}");
                }
                
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
                if (obj.role == role["stage"])
                {
                    Debug.Log("stage");
                    // Objectの場合
                    // GameObjectを作成し、スプライトを登録
                    GameObject go = new GameObject();
                    CreateSprite(go, objPos, game);
                    // タグに"Ground"を指定
                    go.tag = "Ground";
                }
                else if (obj.role == role["player"])
                {
                    Debug.Log("player");
                    // Playerの場合
                    // スタート位置をStageCtrlに登録する
                    GameObject startPoint = new GameObject("StartPoint");
                    startPoint.transform.position = new Vector2((float)(pos.x + pos.width / 2)/100, (float)(game.stage.height - pos.y - pos.height / 2)/100);
                    stageCtrl.GetComponent<StageCtrl>().continuePoint[0] = startPoint;
                    // プレイヤー、接地判定のGameObjectを作成
                    // GameObject player = GameObject.Find("Player");
                    GameObject groundCheck = (GameObject)Resources.Load("GroundCheck");
                    GameObject headCheck = (GameObject)Resources.Load("HeadCheck");

                    // スプライトを登録
                    // CreateSprite(player, objPos, game);
                    // Meshを作成して登録

                    // プレハブを元にオブジェクトを生成する
                    
                    GameObject player = (GameObject)Instantiate((GameObject)Resources.Load("Body"), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
                    GameObject genericMan = player.transform.GetChild(0).Find("Generic Man").gameObject;
                    player.AddComponent<SkinnedMeshRenderer>();
                    SkinnedMeshRenderer rend = player.GetComponent<SkinnedMeshRenderer>();

                    CreateHumanMesh(rend, genericMan, obj.meshData, pos);

                    // Playerスクリプトを設定
                    player.AddComponent<Player>();
                    // Rigidbody2D、回転の無効化を設定
                    player.AddComponent<Rigidbody2D>();
                    player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
                    // 足元と頭の接地判定を設定
                    //  相対位置を指定、pixel単位のheightをそのまま使うと大きすぎるので、100分の一にする
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
                    //  playerスクリプトのコンポーネントに設置判定のオブジェクトを設定する
                    Player playerScript = player.GetComponent<Player>();
                    playerScript.ground = groundCheck.GetComponent<GroundCheck>();
                    playerScript.head = headCheck.GetComponent<GroundCheck>();
                    playerScript.head.checkPlatformGround = false;
                    playerScript.width = pos.width / 100;
                    playerScript.height = pos.height / 100;
                } else if (obj.role == role["enemy"]){
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
                } else if (obj.role == role["item"]){
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
                } else if (obj.role == role["goal"]){
                    // Goalの場合
                    // GameObjectを作成し、スプライトを登録
                    GameObject goal = new GameObject();
                    CreateSprite(goal, objPos, game);
                    goal.AddComponent<PlayerTriggerCheck>();
                    goal.GetComponent<PolygonCollider2D>().isTrigger = true;
                    // StageCtrlのステージクリアトリガーにgoalのPlayerTriggerCheckを使用する
                    stageCtrl.GetComponent<StageCtrl>().stageClearTrigger = goal.GetComponent<PlayerTriggerCheck>();
                    // タグに"Goal"を指定
                    goal.tag = "Goal";
                }
            }
        GManager.instance.isImported = true;
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
        // go.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, 
        //                                                     new Rect(0.0f, 0.0f, pos.width, pos.height), 
        //                                                     new Vector2(0.5f, 0.5f));
        Texture2D resizedTexture = ResizeTexture(texture, pos.width, pos.height);
        go.GetComponent<SpriteRenderer>().sprite = Sprite.Create(resizedTexture, 
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

    private Texture2D ResizeTexture(Texture2D texture, int width, int height)
    {
        // リサイズ後のサイズを持つRenderTextureを作成して書き込む
        var rt = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(texture, rt);

        // リサイズ後のサイズを持つTexture2Dを作成してRenderTextureから書き込む
        var preRT = RenderTexture.active;
        RenderTexture.active = rt;
        var ret = new Texture2D(width, height);
        ret.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        ret.Apply();
        RenderTexture.active = preRT;

        RenderTexture.ReleaseTemporary(rt);
        return ret;
    }

    private void CreateHumanMesh(SkinnedMeshRenderer rend, GameObject humanBones, MeshJson meshData, PositionJson pos){
        var mesh = new Mesh();
        var bonesPerVertex = new List<byte>();
        var weights = new List<BoneWeight1>();
        int size = 15;
        var boneNamesOnVertices = meshData.boneNamesOnVertices;
        var triangles = meshData.triangles;
        var vertices = meshData.vertices;

        // // Mesh作成に必要な情報（頂点、三角形の順番、座標とボーン名の対応）を作成
        // var vertices = new List<Vector3>();
        // var triangles = new List<int>();
        // var boneNamesOnVertices = new Dictionary<Vector3, List<string>>();

        // int[] test_i = {1, 2, 3};
        // Debug.Log(test_i.Where(x => x % 2 == 0).Select(x));


        // Vector2[] humanPoints = {
        //     new Vector2(0, 7), new Vector2(1, 7), new Vector2(2, 7), new Vector2(3, 7), new Vector2(4, 7),
        //     new Vector2(0, 6), new Vector2(1, 6), new Vector2(2, 6), new Vector2(3, 6), new Vector2(4, 6),
        //     new Vector2(0, 5), new Vector2(1, 5), new Vector2(2, 5), new Vector2(3, 5), new Vector2(4, 5),
        //     new Vector2(0, 4), new Vector2(1, 4), new Vector2(2, 4), new Vector2(3, 4), new Vector2(4, 4),
        //     new Vector2(0, 3), new Vector2(1, 3), new Vector2(2, 3), new Vector2(3, 3), new Vector2(4, 3),
        //     new Vector2(0, 2), new Vector2(1, 2), new Vector2(2, 2), new Vector2(3, 2), new Vector2(4, 2),
        //     new Vector2(0, 1), new Vector2(1, 1), new Vector2(2, 1), new Vector2(3, 1), new Vector2(4, 1),
        //     new Vector2(0, 0), new Vector2(1, 0), new Vector2(2, 0), new Vector2(3, 0), new Vector2(4, 0)
        // };

        // ヒト型のMeshの元になる座標のリスト
        // Vector2[] humanPoints = {
        //                                           new Vector2(2, 8),
        //                                           new Vector2(2, 7),
        //     new Vector2(0, 6), new Vector2(1, 6), new Vector2(2, 6), new Vector2(3, 6), new Vector2(4, 6),
        //     new Vector2(0, 5),                    new Vector2(2, 5),                    new Vector2(4, 5),
        //     new Vector2(0, 4),                    new Vector2(2, 4),                    new Vector2(4, 4),
        //                        new Vector2(1, 3), new Vector2(2, 3), new Vector2(3, 3),                   
        //                        new Vector2(1, 2),                    new Vector2(3, 2),                     
        //                        new Vector2(1, 1),                    new Vector2(3, 1),                     
        //                        new Vector2(1, 0),                    new Vector2(3, 0)                   
        // };
        Vector2[] humanPoints = {
                                                  new Vector2(3, 9),
                                                  new Vector2(3, 8),
            new Vector2(1, 7), new Vector2(2, 7), new Vector2(3, 7), new Vector2(4, 7), new Vector2(5, 7),
            new Vector2(1, 6),                    new Vector2(3, 6),                    new Vector2(5, 6),
            new Vector2(1, 5),                    new Vector2(3, 5),                    new Vector2(5, 5),
                               new Vector2(2, 4), new Vector2(3, 4), new Vector2(4, 4),                   
                               new Vector2(2, 3),                    new Vector2(4, 3),                     
                               new Vector2(2, 2),                    new Vector2(4, 2),                     
                               new Vector2(2, 1),                    new Vector2(4, 1)                   
        };
        // humanPoints = humanPoints.Select(point => point + addOne).ToArray();
        // humanPoints = humanPoints.Select(point => new Vector2(point[0] + 1, point[1] + 1)).ToArray();

        // ヒト型のMeshの点とボーン部位の対応
        var boneNamedPoints = new Dictionary<string, Vector2>(){
            {"hips", humanPoints[14]},
            {"spine", humanPoints[11]},
            {"chest", humanPoints[8]},
            {"chest.Upper", humanPoints[4]},
            {"collarbone.L", humanPoints[3]},
            {"upperArm.L", humanPoints[2]},
            {"lowerArm.L", humanPoints[7]},
            {"hand.L", humanPoints[10]},
            {"collarbone.R", humanPoints[5]},
            {"upperArm.R", humanPoints[6]},
            {"lowerArm.R", humanPoints[9]},
            {"hand.R", humanPoints[12]},
            // {"indexProximal.R", null},
            // {"indexIntermediate.R", null},
            // {"indexDistal.R", null},
            // {"indexDistal.R_end", null},
            // {"middleProximal.R", null},
            // {"middleIntermediate.R", null},
            // {"middleDistal.R", null},
            // {"middleDistal.R_end", null},
            // {"thumbProximal.R", null},
            // {"thumbIntermediate.R", null},
            // {"thumbDistal.R", null},
            // {"thumbDistal.R_end", null},
            {"neck", humanPoints[1]},
            {"head", humanPoints[0]},
            // {"eye.L", null},
            // {"eye.L_end", null},
            // {"eye.R", null},
            // {"eye.R_end", null},
            // {"jaw", null},
            // {"jaw_end", null},
            {"upperLeg.L", humanPoints[13]},
            {"lowerLeg.L", humanPoints[16]},
            {"foot.L", humanPoints[18]},
            {"toe.L", humanPoints[20]},
            // {"toe.L_end", null},
            {"upperLeg.R", humanPoints[15]},
            {"lowerLeg.R", humanPoints[17]},
            {"foot.R", humanPoints[19]},
            {"toe.R", humanPoints[21]},
            // {"toe.R_end", null}
        };

        // foreach(Vector2 point in humanPoints){
        //     // Debug.Log(point_i);
        //     // 座標を中心に4点の座標を作成
        //     // Debug.Log(point.x.GetType());
        //     // int x;
        //     // int y;
        //     var x = point.x;
        //     var y = point.y;
        //     var sqrVertices = new Vector3[4];
        //     sqrVertices[0] = new Vector3(x + 1, y, 0);
        //     sqrVertices[1] = new Vector3(x, y, 0);
        //     sqrVertices[2] = new Vector3(x + 1, y + 1, 0);
        //     sqrVertices[3] = new Vector3(x, y + 1, 0);

        //     // 作成した4点とvertices（頂点のリスト）の和集合（同じ値が含まれない）を作成
        //     vertices = vertices.Union(sqrVertices).ToList();
        //         Debug.Log($"vertices.Count {vertices.Count}");
        //     foreach(var vertex in vertices){
        //         // Debug.Log($"{point_i} x: {vertex[0]}, y: {vertex[1]}, z:{vertex[2]}");
        //     }
        //     // verticesにおける4つの頂点のidを取得
        //     var vertexIndexes = new int[4];
        //     for(int ver_i = 0; ver_i < sqrVertices.Length; ver_i ++){
        //         vertexIndexes[ver_i] = vertices.FindIndex(v => v == sqrVertices[ver_i]);
        //     }

        //     // 4頂点をポリゴン化するための三角形の順番を指定（0 -> 1 -> 2, 1 -> 3 -> 2 の二つ）し、
        //     // triangles（三角形のリスト）に追加（同じ値も含まれる）
        //     var sqrTriangles = new int[] { vertexIndexes[0], vertexIndexes[1], vertexIndexes[2], vertexIndexes[1], vertexIndexes[3], vertexIndexes[2] };
        //     triangles.AddRange(sqrTriangles);

        //     // 座標に対応するボーン名を取得
        //     string boneName = boneNamedPoints.FirstOrDefault(bNPoint => bNPoint.Value == point).Key;

        //     // 座標に対応するボーン名を登録する（keyが座標、valueがボーン名のリスト）
        //     // 一つの座標に複数のボーン名が登録される場合もある
        //     foreach(Vector3 sqrVertex in sqrVertices){
        //         // 該当の座標にすでにボーン名が登録されているか判定
        //         if(boneNamesOnVertices.ContainsKey(sqrVertex)){
        //         // キーがある場合（すでに頂点が登録されている場合）
        //             // 頂点に対応するボーン名のリストに新たなボーン名を追加
        //             boneNamesOnVertices[sqrVertex].Add(boneName);
        //         }else{
        //         // キーがない場合（重複する頂点を持たない場合）
        //             // 新たに頂点とboneNameのペアを追加
        //             boneNamesOnVertices.Add(sqrVertex, new List<string>(){boneName});
        //         }
        //     }
        // }

        // int vBN_i = 0;
        // foreach(var vBN in boneNamesOnVertices){
        //     string boneNames = "";
        //     foreach(var name in vBN){
        //         boneNames += " " + name + ",";
        //     }
        //     Debug.Log(vBN_i + " / vertex :x" + vBN.Key.x + ", y" + vBN.Key.y + " /" + boneNames);
        //     vBN_i ++;
        // }

        // メッシュに頂点、三角形の順番を登録し、
        // 法線とバウンディングボリュームを再計算する
        // mesh.vertices = vertices.ToArray();
        // mesh.triangles = triangles.ToArray();
        mesh.vertices = vertices.Select(point => new Vector3(point.x, point.y, 0)).ToArray();
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < uvs.Length; i++)
        {
            Debug.Log($"vertices[i].x {vertices[i].x}, vertices[i].z {vertices[i].y}");
            uvs[i] = new Vector2(vertices[i].x/pos.width, vertices[i].y/pos.height);
        }
        mesh.uv = uvs;

        // ボーン（Humanoid Avatarが適応されるTransform群）の全階層のtransformを配列にコピーする
        var bones = new List<Transform>();
        GetAllChildren(humanBones, bones);

        // ボーン名の配列を作成
        // ウェイト作成でボーンを指定する際に、この配列のインデックスを使用する
        string[] boneNameIndex = bones.Select(bone => bone.name).ToArray();
        int m = 0;
        foreach(string name in boneNameIndex){
            Debug.Log(m + " " + name);
            m++;
        }
        // int i_bNV = 0;
        
        // Mesh作成に必要な情報（各頂点が関連するボーンの数(byte型)、ウェイト）を作成
        // bonesPerVertexは頂点の数と同じ、weighsは頂点数 x 関連するボーンの数の合計の長さ
        // foreach(var boneNamesOnVertex in boneNamesOnVertices){
        //     // foreach(var name in boneNamesOnVertex){
        //     //     Debug.Log($"{i_bNV}: {name}");
        //     // }

        //     // 各頂点が関連するボーンの数を作成
        //     // 各頂点（Key）が持つBoneNameリスト（Value）
        //     var boneNames = boneNamesOnVertex.ToArray();
        //     // BoneNameの数
        //     int boneCount = boneNames;

        //     // BoneNameの数をbyte型で追加
        //     bonesPerVertex.Add(Convert.ToByte(boneCount));

        //     // 登録されているボーンの数だけウェイトを作成する
        //     foreach(var boneName in boneNames){
        //         var weight = new BoneWeight1();

        //         // ボーン（Humanoid Avatarが適応されるTransform群）のIndexを登録する
        //         weight.boneIndex = Array.IndexOf(boneNameIndex, boneName);

        //         // 一つの頂点に複数のボーン名が登録されている場合、ウェイトは等分とする
        //         weight.weight = 1.0f / boneCount;

        //         Debug.Log("boneIndex: " + weight.boneIndex + "boneName: " + boneName + ", weight: " + weight.weight);

        //         // ウェイトのリスト（weights）に追加する
        //         weights.Add(weight);
        //     }

        //     i_bNV ++;
        // }

        // 各頂点が関連するボーンの数(byte型)のリスト、ウェイトのリストをそれぞれNativeArray型に変換する
        var bonesPerVertexArray = new NativeArray<byte>(bonesPerVertex.ToArray(), Allocator.Temp);
        var weightsArray = new NativeArray<BoneWeight1>(weights.ToArray(), Allocator.Temp);

        // メッシュに各頂点が関連するボーンの数(byte型)、ウェイトを設定する
        mesh.SetBoneWeights(bonesPerVertexArray, weightsArray);

        // ボーントランスフォームとバインドポーズ(デフォルト位置)を作成する
        Matrix4x4[] bindPoses = new Matrix4x4[bones.Count];
        for(int pose_i = 0; pose_i < bindPoses.Length; pose_i ++){
            bindPoses[pose_i] = bones[pose_i].worldToLocalMatrix * transform.localToWorldMatrix;
        }

        // bindPoses 配列を、メッシュの bindposes 配列に割り当て、デフォルトポーズをバインドする
        mesh.bindposes = bindPoses;

        // ボーンのリストを配列に変換し、スキンメッシュレンダラーに登録
        rend.bones = bones.ToArray();

        // メッシュをスキンメッシュレンダラーに登録
        rend.sharedMesh = mesh;

        // ルートボーンをスキンメッシュレンダラーに登録
        GameObject hips = humanBones.transform.Find("hips").gameObject;
        rend.rootBone = hips.transform;
    }

    static void GetAllChildren(GameObject parent, List<Transform> bones) {
        Transform children = parent.GetComponentInChildren<Transform>();
        //子要素がいなければ終了
        if (children.childCount == 0) {
            return;
        }
        foreach(Transform child in children) {
            bones.Add(child);
            GetAllChildren(child.gameObject, bones);
        }
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
    public int role;
    public MeshJson meshData;
}

[Serializable]
public class MeshJson
{
    public PointJson[] vertices;
    public int[] triangles;
    public List<string[]> boneNamesOnVertices;
}

[Serializable]
public class PointJson
{
    public int x;
    public int y;
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