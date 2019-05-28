using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CorrectMetalicMap : ScriptableWizard
{
    public Texture2D[] textures;

    void OnWizardCreate()
    {
        foreach(Texture2D texture in textures)
        {
            Texture2D newTexture = new Texture2D(texture.width, texture.height);

            for(int i = 0; i < texture.width; i++)
            {
                for(int j = 0; j < texture.height; j++)
                {
                    Color color = texture.GetPixel(i, j);
                    newTexture.SetPixel(i, j, new Color(color.b, color.r, color.g, color.g));
                }
            }
            //newTexture.EncodeToPNG();
            //AssetDatabase.CreateAsset(newTexture, "Assets/Models/CorrectedTextures/" + texture.name +"_Modified.png");
            File.WriteAllBytes(Application.dataPath + "/Models/CorrectedTextures/" + texture.name + "_Modified.png", newTexture.EncodeToPNG());
        }
    }

    [MenuItem("GameObject/Correct Metalic Map For Unity")]
    static void RenderCubemap()
    {
        ScriptableWizard.DisplayWizard<CorrectMetalicMap>(
            "Correct Metalic Map", "Correct");
    }
}
