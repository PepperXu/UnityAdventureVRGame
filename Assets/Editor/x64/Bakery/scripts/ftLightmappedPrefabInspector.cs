using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BakeryLightmappedPrefab))]
[CanEditMultipleObjects]
public class ftLightmappedPrefabInspector : UnityEditor.Editor
{
    bool allPrefabsGood = true;
    SerializedProperty isEnabled;

    void Refresh(BakeryLightmappedPrefab selected)
    {
        allPrefabsGood = selected.IsValid() && allPrefabsGood;
    }

    void OnPrefabInstanceUpdate(GameObject go)
    {
        allPrefabsGood = true;
        foreach(BakeryLightmappedPrefab selected in targets)
        {
            //if (go != selected.gameObject) continue;
            Refresh(selected);
        }
    }

    void OnEnable()
    {
        allPrefabsGood = true;
        foreach(BakeryLightmappedPrefab selected in targets)
        {
            Refresh(selected);
        }
        PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdate;
        isEnabled = serializedObject.FindProperty("enableBaking");
    }

    void OnDisable()
    {
        PrefabUtility.prefabInstanceUpdated -= OnPrefabInstanceUpdate;
    }

    public override void OnInspectorGUI() {

        serializedObject.Update();
        var prev = isEnabled.boolValue;
        EditorGUILayout.PropertyField(isEnabled, new GUIContent("Enable baking", "Prefab contents will be patched after baking if this checkbox is on. Patched prefab will be lightmapped when instantiated in any scene."));
        serializedObject.ApplyModifiedProperties();

        if (isEnabled.boolValue != prev)
        {
            allPrefabsGood = true;
            foreach(BakeryLightmappedPrefab selected in targets)
            {
                selected.enableBaking = isEnabled.boolValue;
                Refresh(selected);
            }
        }

        if (allPrefabsGood)
        {
            EditorGUILayout.LabelField("Prefab connection: OK");
        }
        else
        {
            foreach(BakeryLightmappedPrefab selected in targets)
            {
                if (selected.errorMessage.Length > 0) EditorGUILayout.LabelField("Error: " + selected.errorMessage);
            }
        }
    }
}

