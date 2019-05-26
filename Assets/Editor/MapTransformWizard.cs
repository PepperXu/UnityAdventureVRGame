using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MapTransformWizard : ScriptableWizard
{
    public GameObject objectToCreate;
    public Transform[] transformReferences;

    void OnWizardUpdate()
    {
        
    }

    void OnWizardCreate()
    {
        for(int i = 0; i < transformReferences.Length; i++)
        {
            GameObject obj = Instantiate(objectToCreate) as GameObject;
            obj.transform.position = transformReferences[i].position;
            obj.transform.rotation = transformReferences[i].rotation;
            obj.transform.localScale = transformReferences[i].localScale;
        }
    }

    [MenuItem("GameObject/Map Transforms")]
    static void RenderCubemap()
    {
        ScriptableWizard.DisplayWizard<MapTransformWizard>(
            "Map Transform", "Map!");
    }
}
