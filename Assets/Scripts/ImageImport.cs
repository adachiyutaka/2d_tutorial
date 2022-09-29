using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
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
        string url = "http://localhost:3000/games/31/unity/";
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
            // GameJson game = JsonUtility.FromJson<GameJson>(jsonStr);
            GameJson game = JsonMapper.ToObject<GameJson>(jsonStr);

            // デバッグ用 unityアクションから送られてきたデータを一括表示
            // Debug.Log("yield");
            // Debug.Log($"jsonStr {jsonStr}");
            // // Debug.Log($"stage.width: {game.stage.width}, stage.height{game.stage.height}");
            // Debug.Log($"gameData {game.objects[0].role}");

            foreach (var obj in game.objects)
            {
                var meshData = obj.meshData;
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

                int armatureId = 0;
                foreach(var points in meshData.armature)
                {
                    if(points != null){
                        Debug.Log($"armature {armatureId}:{points.x}, {points.y}");
                    }else{
                        Debug.Log($"armature {armatureId}: null");
                    }
                    armatureId ++;
                }

                foreach(var boneIds in meshData.boneIdOnVertices)
                {
                    string boneIdString = "";
                    foreach(var id in boneIds)
                    {
                        boneIdString += $"{id}, ";
                    }
                    Debug.Log($"boneIds:{boneIdString}");
                }

                foreach(var boneWeights in meshData.boneWeightOnVertices)
                {
                    string boneWeightString = "";
                    foreach(var weight in boneWeights)
                    {
                        boneWeightString += $"{weight}, ";
                    }
                    Debug.Log($"boneWeights:{boneWeightString}");
                }
                
            }
            // foreach (var position in game.positions)
            // {
            //     Debug.Log($"symbol:{position.symbol}");
            //     Debug.Log($"height:{position.height}, width:{position.width}, x:{position.y}, x:{position.y}");
            // }
            // foreach (var objectPosition in game.objectPositions)
            // {
            //     Debug.Log($"objectId:{objectPosition.objectId}, positionId:{objectPosition.positionId}");
            // }
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

                    // プレハブを元にオブジェクトを生成する                    
                    GameObject player = (GameObject)Instantiate((GameObject)Resources.Load("Body"), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
                    GameObject genericMan = player.transform.GetChild(0).Find("Generic Man").gameObject;
                    player.AddComponent<SkinnedMeshRenderer>();
                    SkinnedMeshRenderer rend = player.GetComponent<SkinnedMeshRenderer>();

                    // Meshを作成し、登録
                    CreateHumanMesh(rend, player, obj, pos);

                    // Playerスクリプトを設定
                    player.AddComponent<Player>();
                    // Rigidbody2D、回転の無効化を設定
                    player.AddComponent<Rigidbody2D>();
                    player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
                    // Capsule Collider を設定
                    player.AddComponent<CapsuleCollider2D>();
                    CapsuleCollider2D capsuleCollider2D = player.GetComponent<CapsuleCollider2D>();
                    player.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    capsuleCollider2D.direction = CapsuleDirection2D.Vertical;
                    capsuleCollider2D.size = new Vector2(pos.width/2, pos.height);
                    capsuleCollider2D.offset = new Vector2(0, capsuleCollider2D.offset.y);
                    Debug.Log($"pos.height {pos.height}, pos.width {pos.width}, capsuleCollider2D.size {capsuleCollider2D.size}");
                    // 足元と頭の接地判定を設定
                    //  相対位置を指定、pixel単位のheightをそのまま使うと大きすぎるので、100分の一にする
                    Vector3 playerPos = player.transform.position;
                    Vector3 groundPos = new Vector3(0, 0, 0);
                    Vector3 headPos = new Vector3(0, (float)pos.height/100, 0);

                    // Vector3 groundPos = new Vector3(playerPos.x, playerPos.y - (float)pos.height/200, playerPos.z);
                    // Vector3 headPos = new Vector3(playerPos.x, playerPos.y + (float)pos.height/200, playerPos.z);
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

    private void CreateHumanMesh(SkinnedMeshRenderer rend, GameObject player, ObjectJson obj, PositionJson pos){
        GameObject transforms = player.transform.GetChild(0).Find("Generic Man").gameObject;
        var mesh = new Mesh();
        var bonesPerVertex = new List<byte>();
        var weights = new List<BoneWeight1>();
        int size = 15;
        var meshData = obj.meshData;
        var armature = meshData.armature;
        var boneIdOnVertices = meshData.boneIdOnVertices;
        var boneWeightOnVertices = meshData.boneWeightOnVertices;
        var triangles = meshData.triangles;
        var vertices = meshData.vertices;
        Animator animator = player.GetComponent<Animator>();

        transforms = transforms.transform.GetChild(0).gameObject;

        // メッシュに頂点、三角形の順番を登録し、
        // 法線とバウンディングボリュームを再計算する
        // mesh.vertices = vertices.ToArray();
        // mesh.triangles = triangles.ToArray();
        // y座標はwebのcanvasとunityで異なるので変換する
        // mesh.vertices = vertices.Select(point => new Vector3(point.x/100, (pos.height - point.y)/100, 0)).ToArray();
        mesh.vertices = vertices.Select(point => new Vector3(point.x, pos.height - point.y, 0)).ToArray();

        // JSで作成した三角形はunityと逆順（反時計回り）で、裏向き（-z方向）にテクスチャが描画されてしまうため、三角形の並びを反転させる
        mesh.triangles = triangles.Reverse().ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < uvs.Length; i++)
        {
            // unityとJSのcanvasではy座標の向きが違うため、変換する
            uvs[i] = new Vector2((float)vertices[i].x/pos.width, pos.height - (float)vertices[i].y/pos.height);
        }
        mesh.uv = uvs;

        // ボーン（Humanoid Avatarが適応されるTransform群）の全階層のtransformを配列にコピーする
        var transformList = new List<Transform>();
        GetAllChildren(transforms, transformList);
        Debug.Log($"transformList.Count {transformList.Count}");
        foreach(var transform in transformList){
            Debug.Log($"transformList name {transform.name}");
        }
        Debug.Log("old transform -----------------");
        CheckTransform(transforms);
        MakeArmature(transformList, armature, pos.height);
        Debug.Log("new transform -----------------");
        CheckTransform(transforms);

        Dictionary<string, string> humanBoneNameDict = new Dictionary<string, string>();
        humanBoneNameDict.Add("Hips", "hips");
        humanBoneNameDict.Add("LeftUpperLeg", "upperLeg.L");
        humanBoneNameDict.Add("RightUpperLeg", "upperLeg.R");
        humanBoneNameDict.Add("LeftLowerLeg", "lowerLeg.L");
        humanBoneNameDict.Add("RightLowerLeg", "lowerLeg.R");
        humanBoneNameDict.Add("LeftFoot", "foot.L");
        humanBoneNameDict.Add("RightFoot", "foot.R");
        humanBoneNameDict.Add("Spine", "spine");
        humanBoneNameDict.Add("Chest", "chest");
        humanBoneNameDict.Add("Neck", "neck");
        humanBoneNameDict.Add("Head", "head");
        humanBoneNameDict.Add("LeftUpperArm", "upperArm.L");
        humanBoneNameDict.Add("RightUpperArm", "upperArm.R");
        humanBoneNameDict.Add("LeftLowerArm", "lowerArm.L");
        humanBoneNameDict.Add("RightLowerArm", "lowerArm.R");
        humanBoneNameDict.Add("LeftHand", "hand.L");
        humanBoneNameDict.Add("RightHand", "hand.R");

        // モデルのボーン名とUnityデフォルトのボーン名を対応させる
        string[] humanTraitBoneNames = HumanTrait.BoneName;
        List<HumanBone> humanBones = new List<HumanBone>(humanTraitBoneNames.Length);
        for (int i = 0; i < humanTraitBoneNames.Length; i++)
        {
            string humanBoneName = humanTraitBoneNames[i];
            string boneName;
            if (humanBoneNameDict.TryGetValue(humanBoneName, out boneName))
            {
                HumanBone humanBone = new HumanBone();
                humanBone.humanName = humanBoneName;
                humanBone.boneName = boneName;
                humanBone.limit.useDefaultValues = true;

                humanBones.Add(humanBone);
            }
        }
        
        var skeletonList = new List<Transform>();
        GetAllChildren(player, skeletonList);

        List<SkeletonBone> skeletonBones = new List<SkeletonBone>(skeletonList.Count);
        for (int i = 0; i < skeletonList.Count; i++)
        {
            Transform bone = skeletonList[i];

            SkeletonBone skelBone = new SkeletonBone();
            skelBone.name = bone.name;
            skelBone.position = bone.localPosition;
            skelBone.rotation = bone.localRotation;
            skelBone.scale = Vector3.one;

            skeletonBones.Add(skelBone);
        }

        // アバターにどういう構造、特性の人型のモデル化を教えるクラス
        HumanDescription humanDesc = new HumanDescription();

        humanDesc.human = humanBones.ToArray();
        humanDesc.skeleton = skeletonBones.ToArray();

        humanDesc.upperArmTwist = 0.5f;
        humanDesc.lowerArmTwist = 0.5f;
        humanDesc.upperLegTwist = 0.5f;
        humanDesc.lowerLegTwist = 0.5f;
        humanDesc.armStretch = 0.05f;
        humanDesc.legStretch = 0.05f;
        humanDesc.feetSpacing = 0.0f;
        humanDesc.hasTranslationDoF = false;

        Avatar avatar = AvatarBuilder.BuildHumanAvatar(player, humanDesc);
       
        if (!avatar.isValid || !avatar.isHuman)
        {
            Debug.LogError("setup error");
            return;
        }

        animator.avatar = avatar;

        // ボーン名の配列を作成
        // ウェイト作成でボーンを指定する際に、この配列のインデックスを使用する
        string[] boneNameIndex = transformList.Select(bone => bone.name).ToArray();

        Debug.Log("boneIndex, boneName");
        var boneId = 0;
        foreach(var boneName in boneNameIndex){
            Debug.Log($"{boneId}, {boneName}");
            boneId ++;
        };

        // Mesh作成に必要な情報（各頂点が関連するボーンの数(byte型)、ウェイト）を作成
        // bonesPerVertexは頂点の数と同じ、weighsは頂点数 x 関連するボーンの数の合計の長さ
        for(int vertex_i = 0; vertex_i < vertices.Length; vertex_i++ )
        {
            var boneIds = boneIdOnVertices[vertex_i];
            // 1頂点に関連するボーンの数
            int boneCount = boneIds.Length;

            // byte型で追加
            bonesPerVertex.Add(Convert.ToByte(boneCount));

            // 登録されているボーンの数だけウェイトを作成する
            for(int bone_i = 0; bone_i < boneIds.Length; bone_i++){
                var weight = new BoneWeight1();

                // ボーン（Humanoid Avatarが適応されるTransform群）のIndexを登録する
                weight.boneIndex = boneIds[bone_i];

                // それぞのweightを登録する
                // weight.weight = (float)Math.Round(boneWeightOnVertices[vertex_i][bone_i], 1);
                weight.weight = (float)boneWeightOnVertices[vertex_i][bone_i];

                // ウェイトのリスト（weights）に追加する
                weights.Add(weight);
            }
        }
        Debug.Log("weights");

        foreach(var weight in weights){
            Debug.Log(weight.weight);
        }
        
        // 各頂点が関連するボーンの数(byte型)のリスト、ウェイトのリストをそれぞれNativeArray型に変換する
        var bonesPerVertexArray = new NativeArray<byte>(bonesPerVertex.ToArray(), Allocator.Temp);
        var weightsArray = new NativeArray<BoneWeight1>(weights.ToArray(), Allocator.Temp);

        // メッシュに各頂点が関連するボーンの数(byte型)、ウェイトを設定する
        mesh.SetBoneWeights(bonesPerVertexArray, weightsArray);
        // ボーントランスフォームとバインドポーズ(デフォルト位置)を作成する
        Matrix4x4[] bindPoses = new Matrix4x4[transformList.Count];
        for(int pose_i = 0; pose_i < bindPoses.Length; pose_i ++){
            bindPoses[pose_i] = transformList[pose_i].worldToLocalMatrix * transform.localToWorldMatrix;
        }

        // bindPoses 配列を、メッシュの bindposes 配列に割り当て、デフォルトポーズをバインドする
        mesh.bindposes = bindPoses;

        // ボーンのリストを配列に変換し、スキンメッシュレンダラーに登録
        rend.bones = transformList.ToArray();

        // メッシュをスキンメッシュレンダラーに登録
        rend.sharedMesh = mesh;

        // 画像からテクスチャーを作成し、スキンメッシュレンダラーに登録
        Texture2D texture = CreateTextureFromBytes(Convert.FromBase64String(obj.image));
        rend.material.mainTexture = texture;

        // ルートボーンをスキンメッシュレンダラーに登録
        // GameObject hips = transforms.transform.Find("hips").gameObject;
        // rend.rootBone = hips.transform;
        rend.rootBone = transforms.transform;
    }


    static void GetAllChildren(GameObject parent, List<Transform> transformList) {
        transformList.Add(parent.transform);
        //子要素がいなければ終了
        int childCount = parent.transform.childCount;
        if (childCount > 0) {
            for(int i = 0; i < childCount; i++){
                GetAllChildren(parent.transform.GetChild(i).gameObject, transformList);
            }
        }
    }

    static void YAxisReverseArmature(PointJson[] armature, int height){
        foreach(PointJson point in armature){
            if(point != null){
                point.y = height - point.y;
            }
        }
    }

    static void PositionBones(List<Transform> transformList, PointJson[] armature){
        for(int bone_i = 0; bone_i < transformList.Count; bone_i ++){
            Transform bone = transformList[bone_i];
            var armaturePos = armature[bone_i];
            var bonePos = new Vector3(0, 0, 0);
            if(armaturePos != null){
                bonePos.x = armaturePos.x;
                bonePos.y = armaturePos.y;

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(armaturePos.x * 0.01f, armaturePos.y * 0.01f, 0);
                sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                
                bone.position = bonePos;
            }else{
                bone.localPosition = bonePos;
            }
        }
    }

    static void RotateBones(List<Transform> transformList, PointJson[] armature){
        foreach(Transform bone in transformList){
            int childCount = bone.childCount;
            if(childCount > 0){
                Transform lookedChild = null;
                Transform[] children = new Transform[childCount];
                bool centerChildFound = false;
                
                if(bone.name == "hips"){
                    var hipsPosition = armature[FindBonePosition("hips")];
                    bone.position = new Vector3(hipsPosition.x, hipsPosition.y, 0);
                }
                
                for(int i = 0; i < childCount; i++){
                    Transform child = bone.GetChild(i);
                    children[i] = child;
                    var armaturePos = armature[FindBonePosition(child.name)];
                    var bonePos = new Vector3(0, 0, 0);

                    if(armaturePos != null){
                        bonePos.x = armaturePos.x;
                        bonePos.y = armaturePos.y;
                        // bonePos.x = armaturePos.x * 0.001f;
                        // bonePos.y = armaturePos.y * 0.001f;
                        child.position = bonePos;
                    }else{
                        child.localPosition = bonePos;
                    }

                    // 子のlocalPositionに値があるかの判定
                    bool isArmaturedBone = child.localPosition.magnitude > 0;
                    Debug.Log($"isArmaturedBone :{isArmaturedBone}, child.localPosition.magnitude: {child.localPosition.magnitude}");

                    if(!centerChildFound){
                        // 子の名前でL, Rがついていないもの（hip〜head.end）かどうかを判定
                        if(!Regex.IsMatch(child.name, ".L|.R")){
                            centerChildFound = true;
                            if(isArmaturedBone){
                                lookedChild = child;
                            }else{
                                lookedChild = null;
                            }
                        }
                        
                        // 子のlocalPositionが(0, 0, 0)かどうかを判定
                        if(isArmaturedBone){
                            lookedChild = child;
                        }
                    }
                }

                if(bone.name == "hips"){
                    Quaternion hipsRotation = Quaternion.Euler(90f, 0f, 0f);
                    bone.localRotation = hipsRotation;
                }
                else if(lookedChild == null){
                    Vector3[] childrenPosition = GetChildrenPosition(children); 
                    bone.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    SetChildrenPosition(children, childrenPosition);
                }else{
                    Debug.Log($"parent: {bone.name}, lookedChild: {lookedChild.name}");

                    // ベクトルを回転情報に変換
                    Vector3[] childrenPosition = GetChildrenPosition(children); 
                    foreach(var child in children){
                        Debug.Log($"LookRotation old {child.name}: {child.position.x}, {child.position.y}, {child.position.z}");
                    }
                    // Quaternion rotation = Quaternion.LookRotation(lookedChild.localPosition);
                    // LookRotationではZ軸+が正面となるので、Y軸+が正面になるように回転する
                    // rotation = rotation * Quaternion.Euler(90f, 0f, 0f);
                    // bone.rotation = rotation;
                    bone.LookAt(lookedChild);
                    bone.rotation *= Quaternion.Euler(90f, 0f, 0f);
                    bone.rotation *= Quaternion.Euler(0f, 180f, 0f);
                    foreach(var child in children){
                        Debug.Log($"LookRotation rot {child.name}: {child.position.x}, {child.position.y}, {child.position.z}");
                    }
                    SetChildrenPosition(children, childrenPosition);
                    foreach(var child in children){
                        Debug.Log($"LookRotation new {child.name}: {child.position.x}, {child.position.y}, {child.position.z}");
                    }
                }
            }
        }

        // ボーン名からarmatureのidを返すローカル関数
        int FindBonePosition(string boneName) {
            var id = transformList.FindIndex(transform => transform.name == boneName);
            return id;
            // return armature[transformList.FindIndex(transform => transform.name == boneName)];
        }

        Vector3[] GetChildrenPosition(Transform[] children){
            Vector3[] childrenPosition = new Vector3[children.Length];
            for(int i = 0; i < children.Length; i++){
                childrenPosition[i] = children[i].position;
            }
            return childrenPosition;
        }

        void SetChildrenPosition(Transform[] children, Vector3[] childrenPosition){
            for(int i = 0; i < children.Length; i++){
                children[i].position = childrenPosition[i];
            }
        }
    }

    static void MakeArmature(List<Transform> transformList, PointJson[] armature, int height) {
        YAxisReverseArmature(armature, height);
        // PositionBones(transformList, armature);
        RotateBones(transformList, armature);
    }

    static void CheckTransform(GameObject parent) {
        //子要素がある場合
        int childCount = parent.transform.childCount;
        if (childCount > 0) {
            for(int i = 0; i < childCount; i++){
                Transform child = parent.transform.GetChild(i);
                var pos = child.position;
                var rot = child.rotation;
                Debug.Log($"name: {child.name}| pos:{pos.x},{pos.y},{pos.z} | rot:{rot.x},{rot.y},{rot.z}");
                CheckTransform(child.gameObject);
            }
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
    public List<int[]> boneIdOnVertices;
    public List<float[]> boneWeightOnVertices;
    public PointJson[] armature;
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