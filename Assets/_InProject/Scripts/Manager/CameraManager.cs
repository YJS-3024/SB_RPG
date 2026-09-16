using DevelopKit;
using UnityEngine;
using yjs.DevKit;

/// <summary>
/// 현재 씬의 Main Camera와 추적 대상을 연결합니다.
/// </summary>
public class CameraManager : MonoSingleton<CameraManager>
{
    private Camera _camera;
    private TopDownCameraFollow _follow;

    public override bool Initialize()
    {
        _camera = Camera.main;
        if (_camera == null)
        {
            RuntimeLog.Warning("CameraManager: Main Camera was not found.");
            return false;
        }

        _follow = _camera.GetComponent<TopDownCameraFollow>();
        if (_follow == null)
            _follow = _camera.gameObject.AddComponent<TopDownCameraFollow>();

        PlayerUnit player = FindAnyObjectByType<PlayerUnit>();
        if (player != null)
            SetTarget(player.transform);

        return true;
    }

    public void SetTarget(Transform target)
    {
        if (_follow == null && !Initialize())
            return;

        _follow.SetTarget(target);
    }

    protected override void Destroy()
    {
        _camera = null;
        _follow = null;
    }
}