using UnityEditor;using UnityEngine;
public static class SetBunnyRigScale{
[MenuItem("Tools/Character/Set BunnyRig Scale 0.001")]
static void Run(){var i=AssetImporter.GetAtPath("Assets/_InProject/Resources/CharacterParts/body_BunnyGirl_BaseRig.fbx") as ModelImporter;if(i==null){Debug.LogError("Importer not found");return;}i.globalScale=0.001f;i.useFileScale=true;i.SaveAndReimport();Debug.Log("BaseRig Scale Factor set to 0.001");}}