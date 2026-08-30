using UnityEditor;using UnityEngine;using UnityEditor.SceneManagement;
public static class AssignBunnyAvatar{
[MenuItem("Tools/Character/Assign Bunny FBX Avatar")]
static void Run(){var go=GameObject.Find("body_BunnyGirl");var av=System.Array.Find(AssetDatabase.LoadAllAssetsAtPath("Assets/_InProject/Resources/CharacterParts/body_BunnyGirl_BaseRig.fbx"),x=>x is Avatar) as Avatar;if(go&&av){go.GetComponent<Animator>().avatar=av;Debug.Log("Assigned Bunny FBX Avatar: "+av.name);EditorSceneManager.MarkSceneDirty(go.scene);}}}