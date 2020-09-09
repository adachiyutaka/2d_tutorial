using UnityEngine;
using System.Runtime.InteropServices;

public class JsTest : MonoBehaviour
{
    public string URI;

    [DllImport("__Internal")]
    private static extern void Hello();

    [DllImport("__Internal")]
    private static extern void HelloString(string str);

    [DllImport("__Internal")]
    private static extern void PrintFloatArray(float[] array, int size);

    [DllImport("__Internal")]
    private static extern int AddNumbers(int x, int y);

    [DllImport("__Internal")]
    private static extern string StringReturnValueFunction();

    // スタート時に呼ばれる
    void Start()
    {
        // 関数呼び出し
        Hello();

        // 数値型の引数と戻り値
        int result = AddNumbers(5, 7);
        Debug.Log(result);

        // 数値型以外の型の引数
        float[] myArray = new float[10];
        PrintFloatArray(myArray, myArray.Length);

        // 文字列型の引数
        HelloString("This is a string.");

        // 文字列の戻り値
        URI = StringReturnValueFunction();
    }
}