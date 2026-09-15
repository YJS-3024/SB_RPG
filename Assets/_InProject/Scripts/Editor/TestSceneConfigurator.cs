#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class TestSceneConfigurator
{
    private const string ScenePath = "Assets/_InProject/Scenes/TestScene.unity";
    private const string SetupRootName = "TestScene_Setup";
    private const string LegacyRootName = "Legacy_Disabled";

    static TestSceneConfigurator()
    {
        EditorApplication.delayCall += ConfigureIfNeeded;
    }

    [MenuItem("Tools/SB RPG/Configure Player Test Scene")]
    public static void Configure()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            Debug.LogWarning("Open TestScene before configuring the player test scene.");
            return;
        }

        GameObject legacyRoot = GameObject.Find(LegacyRootName);
        if (legacyRoot == null)
            legacyRoot = new GameObject(LegacyRootName);

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root == legacyRoot || root.name == SetupRootName)
                continue;

            root.transform.SetParent(legacyRoot.transform, true);
        }
        legacyRoot.SetActive(false);

        GameObject setupRoot = GameObject.Find(SetupRootName);
        if (setupRoot == null)
            setupRoot = new GameObject(SetupRootName);

        CreateGround(setupRoot.transform);
        CreateCamera(setupRoot.transform);
        CreateLight(setupRoot.transform);
        if (setupRoot.GetComponent<TestScript>() == null)
            setupRoot.AddComponent<TestScript>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddToBuildSettings();
        Debug.Log("TestScene configured for PlayerUnit movement testing.");
    }

    private static void ConfigureIfNeeded()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path == ScenePath && GameObject.Find(SetupRootName) == null)
            Configure();
    }

    private static void CreateGround(Transform parent)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(parent);
        ground.transform.position = new Vector3(0f, -0.1f, 0f);
        ground.transform.localScale = new Vector3(20f, 0.2f, 20f);
    }

    private static void CreateCamera(Transform parent)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.transform.SetParent(parent);
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<TopDownCameraFollow>();
        cameraObject.transform.position = new Vector3(0f, 8f, -6f);
        cameraObject.transform.LookAt(new Vector3(0f, 1f, 0f));
        camera.clearFlags = CameraClearFlags.Skybox;
    }

    private static void CreateLight(Transform parent)
    {
        GameObject lightObject = new GameObject("Directional Light");
        lightObject.transform.SetParent(parent);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void AddToBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        if (scenes.Any(scene => scene.path == ScenePath))
            return;

        EditorBuildSettings.scenes = scenes
            .Append(new EditorBuildSettingsScene(ScenePath, true))
            .ToArray();
    }
}
#endif