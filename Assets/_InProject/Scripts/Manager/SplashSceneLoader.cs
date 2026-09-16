using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashSceneLoader : MonoBehaviour
{
    [SerializeField, Min(0f)] private float splashDuration = 5f;
    [SerializeField] private string nextSceneName = "TestScene";

    private void OnGUI()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 42,
            fontStyle = FontStyle.Bold
        };
        titleStyle.normal.textColor = Color.white;

        GUIStyle loadingStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18
        };
        loadingStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(0f, Screen.height * 0.35f, Screen.width, 70f), "SB RPG", titleStyle);
        GUI.Label(new Rect(0f, Screen.height * 0.55f, Screen.width, 40f), "Loading...", loadingStyle);
    }

    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(splashDuration);
        SceneManager.LoadScene(nextSceneName);
    }
}