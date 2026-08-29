using UnityEditor;using UnityEngine;
public static class SetFinalBunnyScale{
[MenuItem("Tools/Character/Set Final Bunny Scale")]
static void Run(){var i=AssetImporter.GetAtPath("Assets/_InProject/Resources/CharacterParts/body_BunnyGirl_BaseRig.fbx") as ModelImporter;i.globalScale=0.00001f;i.useFileScale=true;i.SaveAndReimport();Debug.Log("Final Bunny Scale Factor=0.00001");}}