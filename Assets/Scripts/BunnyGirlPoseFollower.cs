using System.Collections.Generic;
using UnityEngine;

public sealed class BunnyGirlPoseFollower : MonoBehaviour
{
    [SerializeField] private Transform sourceRoot;

    private readonly Dictionary<Transform, Quaternion> sourceStart = new();
    private readonly Dictionary<Transform, Quaternion> targetStart = new();
    private readonly List<(Transform source, Transform target)> bindings = new();
    private Vector3 sourceRootStart;
    private Vector3 targetRootStart;
    private bool ready;

    private static readonly (string source, string target)[] BoneMap =
    {
        ("Hips", "Character1_Hips"), ("Spine", "Character1_Spine"), ("Chest", "Character1_Spine1"),
        ("Upper_Chest", "Character1_Spine2"), ("Neck", "Character1_Neck"), ("Head", "Character1_Head"),
        ("Clavicle_L", "Character1_LeftShoulder"), ("Upper_Arm_L", "Character1_LeftArm"),
        ("Lower_Arm_L", "Character1_LeftForeArm"), ("Hand_L", "Character1_LeftHand"),
        ("Clavicle_R", "Character1_RightShoulder"), ("Upper_Arm_R", "Character1_RightArm"),
        ("Lower_Arm_R", "Character1_RightForeArm"), ("Hand_R", "Character1_RightHand"),
        ("Upper_Leg_L", "Character1_LeftUpLeg"), ("Lower_Leg_L", "Character1_LeftLeg"), ("Foot_L", "Character1_LeftFoot"),
        ("Upper_Leg_R", "Character1_RightUpLeg"), ("Lower_Leg_R", "Character1_RightLeg"), ("Foot_R", "Character1_RightFoot")
    };

    private void Start() => Cache();
private void Update() => ApplyPose();
    private void LateUpdate() => ApplyPose();
    private void ApplyPose()
    {
        if (!ready) Cache();
        if (!ready) return;
        foreach (var binding in bindings)
        {
            var delta = binding.source.rotation * Quaternion.Inverse(sourceStart[binding.source]);
            binding.target.rotation = delta * targetStart[binding.target];
        }
        transform.position = targetRootStart + (sourceRoot.position - sourceRootStart);
    }

private void Cache()
    {
        if (sourceRoot == null && transform.parent != null)
        {
            var parentAnimator = transform.parent.GetComponentInParent<Animator>();
            if (parentAnimator != null) sourceRoot = parentAnimator.transform;
        }
        if (sourceRoot == null) return;
        bindings.Clear();
        sourceStart.Clear();
        targetStart.Clear();
        foreach (var pair in BoneMap)
        {
            var source = FindDeep(sourceRoot, pair.source);
            var target = FindDeep(transform, pair.target);
            if (source == null || target == null) continue;
            bindings.Add((source, target));
            sourceStart[source] = source.rotation;
            targetStart[target] = target.rotation;
        }
        sourceRootStart = sourceRoot.position;
        targetRootStart = transform.position;
        ready = bindings.Count > 0;
    }

    private static Transform FindDeep(Transform root, string name)
    {
        foreach (var item in root.GetComponentsInChildren<Transform>(true))
            if (item.name == name) return item;
        return null;
    }
}