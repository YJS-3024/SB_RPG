using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResourceManager.Instance.Initialize();
        InputManager.Instance.Initialize();
        UnitManager.Instance.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}