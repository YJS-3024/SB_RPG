#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class SDAnimControllerConfigurator
{
    private const string ControllerPath = "Assets/_InProject/Resources/Anim/SD_AnimController.controller";
    private const string IdleAssetPath = "Assets/Haons SD series Pack/Animation/Common/StandA.FBX";
    private const string WalkAssetPath = "Assets/Haons SD series Pack/__Haon SD oldVersion (ver3 - 2020year)/Animations/Common/WalkA_Front (Ver3).FBX";
    private const string RunAssetPath = "Assets/Haons SD series Pack/__Haon SD oldVersion (ver3 - 2020year)/Animations/Common/RunA_Front (Ver3).FBX";
    private const string JumpAssetPath = "Assets/Haons SD series Pack/Animation/Action Adventure(Adv)/Jump_One-cycle-Type(root).FBX";

    static SDAnimControllerConfigurator()
    {
        EditorApplication.delayCall += ConfigureIfNeeded;
    }

    [MenuItem("Tools/SB RPG/Configure SD Animator Controller")]
    public static void Configure()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        AnimationClip idle = LoadClip(IdleAssetPath, "StandA@loop");
        AnimationClip walk = LoadClip(WalkAssetPath, "Walk_front@loop");
        AnimationClip run = LoadClip(RunAssetPath, "RunA_front@loop");
        AnimationClip jump = LoadClip(JumpAssetPath, "Jump_Small root");

        if (controller == null || idle == null || walk == null || run == null || jump == null)
        {
            Debug.LogError("SD Animator configuration failed: controller or movement clip was not found.");
            return;
        }

        while (controller.parameters.Length > 0)
            controller.RemoveParameter(0);

        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState childState in stateMachine.states)
            stateMachine.RemoveState(childState.state);

        foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
            stateMachine.RemoveAnyStateTransition(transition);

        foreach (BlendTree oldTree in AssetDatabase.LoadAllAssetsAtPath(ControllerPath).OfType<BlendTree>())
            Object.DestroyImmediate(oldTree, true);

        AnimatorState locomotion = stateMachine.AddState("Locomotion", new Vector3(320f, 110f));
        AnimatorState jumpState = stateMachine.AddState("Jump", new Vector3(560f, 110f));
        stateMachine.defaultState = locomotion;

        BlendTree blendTree = new BlendTree
        {
            name = "Locomotion",
            blendType = BlendTreeType.Simple1D,
            blendParameter = "Speed",
            useAutomaticThresholds = false
        };

        AssetDatabase.AddObjectToAsset(blendTree, controller);
        blendTree.AddChild(idle, 0f);
        blendTree.AddChild(walk, 0.5f);
        blendTree.AddChild(run, 1f);
        locomotion.motion = blendTree;
        jumpState.motion = jump;

        AnimatorStateTransition toJump = stateMachine.AddAnyStateTransition(jumpState);
        toJump.AddCondition(AnimatorConditionMode.If, 0f, "Jump");
        toJump.hasExitTime = false;
        toJump.duration = 0.1f;
        toJump.canTransitionToSelf = false;

        AnimatorStateTransition toLocomotion = jumpState.AddTransition(locomotion);
        toLocomotion.hasExitTime = true;
        toLocomotion.exitTime = 0.9f;
        toLocomotion.duration = 0.1f;

        EditorUtility.SetDirty(blendTree);
        EditorUtility.SetDirty(stateMachine);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(ControllerPath, ImportAssetOptions.ForceUpdate);
        Debug.Log("SD_AnimController configured: Idle / Walk / Run / Jump.");
    }

    private static void ConfigureIfNeeded()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (!IsConfigured(controller))
            Configure();
    }

    private static bool IsConfigured(AnimatorController controller)
    {
        if (controller == null || controller.layers.Length == 0)
            return false;

        bool hasSpeed = controller.parameters.Any(parameter =>
            parameter.name == "Speed" && parameter.type == AnimatorControllerParameterType.Float);
        bool hasJump = controller.parameters.Any(parameter =>
            parameter.name == "Jump" && parameter.type == AnimatorControllerParameterType.Trigger);

        AnimatorState[] states = controller.layers[0].stateMachine.states
            .Select(childState => childState.state)
            .ToArray();
        AnimatorState locomotion = states.FirstOrDefault(state => state.name == "Locomotion");
        AnimatorState jump = states.FirstOrDefault(state => state.name == "Jump");

        return hasSpeed && hasJump && jump != null && locomotion != null &&
               locomotion.motion is BlendTree tree && tree.children.Length == 3;
    }

    private static AnimationClip LoadClip(string assetPath, string clipName)
    {
        return AssetDatabase.LoadAllAssetsAtPath(assetPath)
            .OfType<AnimationClip>()
            .FirstOrDefault(clip => clip.name == clipName);
    }
}
#endif