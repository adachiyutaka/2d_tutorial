using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChechAvator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;

        int i = 0;
        foreach(var weight in mesh.boneWeights)
        {
            Debug.Log($"{i} index: {weight.boneIndex0}, weight: {weight.weight0}");
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
