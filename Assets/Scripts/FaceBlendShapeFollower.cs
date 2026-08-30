using UnityEngine;

public sealed class FaceBlendShapeFollower : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer source;
    [SerializeField] private SkinnedMeshRenderer target;

    private void Update()
    {
        if (source == null || target == null || source.sharedMesh == null || target.sharedMesh == null) return;
        var count = Mathf.Min(source.sharedMesh.blendShapeCount, target.sharedMesh.blendShapeCount);
        for (var index = 0; index < count; index++)
            target.SetBlendShapeWeight(index, source.GetBlendShapeWeight(index));
    }
}