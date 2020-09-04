using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ImageImport : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        StartCoroutine(GetText());
    }
 
    private IEnumerator GetText() {
        UnityWebRequest request = UnityWebRequest.Get("http://127.0.0.1:3000/games/image");
 
        // リクエスト送信
        yield return request.SendWebRequest();
 
        // 通信エラーチェック
        if (request.isError) {
            Debug.Log(request.error);
        } else {
            if (request.responseCode == 200) {
                // UTF8文字列として取得する
                string test = request.downloadHandler.text;
                Debug.Log(test);
                // バイナリデータとして取得する
                // byte[] results = request.downloadHandler.data;
            }
        }
    }
}