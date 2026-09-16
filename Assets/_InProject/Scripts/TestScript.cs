using UnityEngine;

public class TestScript : MonoBehaviour
{
    private void Start()
    {
        ResourceManager.Instance.Initialize();
        InputManager.Instance.Initialize();
        UnitManager.Instance.Initialize();
        CameraManager.Instance.Initialize();
    }
}