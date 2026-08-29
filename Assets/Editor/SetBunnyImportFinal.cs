using UnityEditor;using UnityEngine;
public static class SetBunnyImportFinal{
[MenuItem("Tools/Character/Set Bunny Import Final")]
static void Run(){var i=AssetImporter.GetAtPath("Assets/_InProject/Resources/CharacterParts/body_BunnyGirl_BaseRig.fbx") as ModelImporter;i.globalScale=0.01f;i.useFileScale=true;i.SaveAndReimport();Debug.Log("BaseRig final Scale Factor=0.01");}}