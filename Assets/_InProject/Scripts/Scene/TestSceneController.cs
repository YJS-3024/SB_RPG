using UnityEngine;

public class TestSceneController : MonoBehaviour
{
    private void Start()
    {
        ResourceManager.Instance.Initialize();
        InputManager.Instance.Initialize();
        UnitManager.Instance.Initialize();
        CameraManager.Instance.Initialize();

        PlayerUnit player = FindAnyObjectByType<PlayerUnit>();
        if (player != null)
            player.transform.position = new Vector3(0f, 0f, -4f);
    }
}