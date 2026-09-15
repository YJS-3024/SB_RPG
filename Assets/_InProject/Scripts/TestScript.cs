using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResourceManager.Instance.Initialize();
        InputManager.Instance.Initialize();
        UnitManager.Instance.Initialize();

        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.GetComponent<TopDownCameraFollow>() == null)
            mainCamera.gameObject.AddComponent<TopDownCameraFollow>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}