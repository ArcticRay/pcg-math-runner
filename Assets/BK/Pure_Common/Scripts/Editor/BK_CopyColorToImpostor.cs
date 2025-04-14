using UnityEngine;
using UnityEditor;

namespace BKPureNature
{
    public class CopyMaterialToLOD
{
    [MenuItem("Tools/BK/Copy colors to impostors")]
    public static void CopyParametersToSelected()
    {
        foreach (var gameObject in Selection.gameObjects)
        {
            LODGroup lodGroup = gameObject.GetComponent<LODGroup>();

            if (lodGroup == null)
            {
                Debug.LogError($"GameObject {gameObject.name} does not have an LODGroup component.");
                continue;
            }

            LOD[] lods = lodGroup.GetLODs();

            if (lods.Length < 4)
            {
                Debug.LogError($"GameObject {gameObject.name} does not have enough LODs (requires at least 4).");
                continue;
            }

            Renderer lod0Renderer = lods[0].renderers[0];
            Renderer lodLastRenderer = lods[3].renderers[0];

            string baseAssetName = System.Text.RegularExpressions.Regex.Match(gameObject.name, @"^([a-zA-Z]+)").Groups[1].Value;
            int lod0MatIndex = FindMaterialIndex(lod0Renderer.sharedMaterials, baseAssetName + "Leaves");

            if (lod0MatIndex == -1)
            {
                Debug.LogError($"GameObject {gameObject.name} does not have the required material '{baseAssetName}Leaves' in the first LOD.");
                continue;
            }

            Material lod0Mat = lod0Renderer.sharedMaterials[lod0MatIndex];
            Material lodLastMat = lodLastRenderer.sharedMaterial;

            // Copy parameters
            lodLastMat.SetVector("_MainColor", lod0Mat.GetVector("_MainColor"));
            lodLastMat.SetVector("_GradientColor", lod0Mat.GetVector("_GradientColor"));

            if (lodLastMat.HasProperty("_GradientFaloff"))
                lodLastMat.SetFloat("_GradientFaloff", lod0Mat.GetFloat("_GradientFaloff"));

            if (lodLastMat.HasProperty("_GradientPosition"))
                lodLastMat.SetFloat("_GradientPosition", lod0Mat.GetFloat("_GradientPosition"));

            if (lodLastMat.HasProperty("_InvertGradient"))
                lodLastMat.SetInt("_InvertGradient", lod0Mat.GetInt("_InvertGradient"));

            lodLastMat.SetVector("_ColorVariation", lod0Mat.GetVector("_ColorVariation"));
            lodLastMat.SetFloat("_ColorVariationPower", lod0Mat.GetFloat("_ColorVariationPower"));
            lodLastMat.SetTexture("_ColorVariationNoise", lod0Mat.GetTexture("_ColorVariationNoise"));
            lodLastMat.SetFloat("_NoiseScale", lod0Mat.GetFloat("_NoiseScale"));
        }
    }

    private static int FindMaterialIndex(Material[] materials, string materialName)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i].name.Contains(materialName))
            {
                return i;
            }
        }
        return -1;
    }
}
}
