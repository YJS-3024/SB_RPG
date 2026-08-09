using System.Linq;
using UnityEditor;
using UnityEngine;

public static class UnityChanToUrpMaterialMigrator
{
    private const string LegacyShaderPrefix = "UnityChanToonShader/";
    private const string TargetShaderName = "Universal Render Pipeline/Simple Lit";

    [MenuItem("Tools/Migration/Migrate UnityChan Toon Materials To URP")]
    public static void Migrate()
    {
        var targetShader = Shader.Find(TargetShaderName);
        if (targetShader == null)
        {
            Debug.LogError($"Target shader not found: {TargetShaderName}");
            return;
        }

        var materialGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
        var changedCount = 0;

        foreach (var guid in materialGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || material.shader == null)
            {
                continue;
            }

            if (!material.shader.name.StartsWith(LegacyShaderPrefix))
            {
                continue;
            }

            var mainTexture = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
            var mainColor = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
            var cutoff = material.HasProperty("_Clipping_Level") ? material.GetFloat("_Clipping_Level") : 0.5f;

            material.shader = targetShader;

            if (mainTexture != null && material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", mainTexture);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", mainColor);
            }

            if (material.HasProperty("_Cutoff"))
            {
                material.SetFloat("_Cutoff", cutoff);
            }

            EditorUtility.SetDirty(material);
            changedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Migrated UnityChan Toon materials to URP: {changedCount}");
    }
}
