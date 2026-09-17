#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class SDAnimControllerConfigurator
{
    private const string ControllerPath = "Assets/_InProject/Resources/Anim/SD_AnimController.controller";
    private const string GreatswordPath = "Assets/Haons SD series Pack/Animation/WeaponMaster Greatsword(WGS)/";
    private const string TwinDaggerPath = "Assets/Haons SD series Pack/Animation/WeaponMaster Twin dagger(WTD)/";
    private const string CommonPath = "Assets/Haons SD series Pack/__Haon SD oldVersion (ver3 - 2020year)/Animations/Common/";
    private const string JumpAssetPath = "Assets/Haons SD series Pack/Animation/Action Adventure(Adv)/Jump_One-cycle-Type(root).FBX";

    static SDAnimControllerConfigurator()
    {
        EditorApplication.delayCall += ConfigureIfNeeded;
    }

    [MenuItem("Tools/SB RPG/Configure SD Animator Controller")]
    public static void Configure()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        AnimationClip[] unarmed =
        {
            LoadClip("Assets/Haons SD series Pack/Animation/Common/StandA.FBX", "StandA@loop"),
            LoadClip(CommonPath + "WalkA_Front (Ver3).FBX", "Walk_front@loop"),
            LoadClip(CommonPath + "RunA_Front (Ver3).FBX", "RunA_front@loop")
        };
        AnimationClip[] greatsword =
        {
            LoadClip(GreatswordPath + "WGS_Stand.FBX", "WGS_Stand@loop"),
            LoadClip(GreatswordPath + "WGS_Walk_Front.FBX", "WGS_Walk_Front@Loop"),
            LoadClip(GreatswordPath + "WGS_Run_Front.FBX", "WGS_Run_Front@Loop")
        };
        AnimationClip[] twinDagger =
        {
            LoadClip(TwinDaggerPath + "WTD_Stand.FBX", "WTD_Stand@loop"),
            LoadClip(TwinDaggerPath + "WTD_Walk_Front.FBX", "WTD_Walk_Front@Loop"),
            LoadClip(TwinDaggerPath + "WTD_Run_Front.FBX", "WTD_Run_Front@Loop")
        };
        AnimationClip jump = LoadClip(JumpAssetPath, "Jump_Small root");

        if (controller == null || controller.layers.Length == 0 ||
            unarmed.Any(clip => clip == null) || greatsword.Any(clip => clip == null) || twinDagger.Any(clip => clip == null) || jump == null)
        {
            Debug.LogError("SD Animator configuration failed: controller or movement clip was not found.");
            return;
        }

        EnsureParameter(controller, "Speed", AnimatorControllerParameterType.Float);
        EnsureParameter(controller, "WeaponStyle", AnimatorControllerParameterType.Float);
        EnsureParameter(controller, "Jump", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState locomotion = FindState(stateMachine, "Locomotion");
        if (locomotion == null)
            locomotion = stateMachine.AddState("Locomotion", new Vector3(320f, 110f));

        AnimatorState jumpState = FindState(stateMachine, "Jump");
        if (jumpState == null)
            jumpState = stateMachine.AddState("Jump", new Vector3(560f, 110f));

        if (stateMachine.defaultState == null)
            stateMachine.defaultState = locomotion;

        BlendTree blendTree = locomotion.motion as BlendTree;
        if (blendTree == null)
        {
            blendTree = new BlendTree { name = "Locomotion" };
            AssetDatabase.AddObjectToAsset(blendTree, controller);
            locomotion.motion = blendTree;
        }

        blendTree.blendType = BlendTreeType.FreeformCartesian2D;
        blendTree.blendParameter = "Speed";
        blendTree.blendParameterY = "WeaponStyle";
        blendTree.useAutomaticThresholds = false;
        blendTree.children = new ChildMotion[0];
        for (int i = 0; i < 3; i++)
        {
            blendTree.AddChild(unarmed[i], new Vector2(i * 0.5f, -1f));
            blendTree.AddChild(greatsword[i], new Vector2(i * 0.5f, 0f));
            blendTree.AddChild(twinDagger[i], new Vector2(i * 0.5f, 1f));
        }

        jumpState.motion = jump;
        if (!stateMachine.anyStateTransitions.Any(transition => transition.destinationState == jumpState))
        {
            AnimatorStateTransition toJump = stateMachine.AddAnyStateTransition(jumpState);
            toJump.AddCondition(AnimatorConditionMode.If, 0f, "Jump");
            toJump.hasExitTime = false;
            toJump.duration = 0.1f;
            toJump.canTransitionToSelf = false;
        }

        if (!jumpState.transitions.Any(transition => transition.destinationState == locomotion))
        {
            AnimatorStateTransition toLocomotion = jumpState.AddTransition(locomotion);
            toLocomotion.hasExitTime = true;
            toLocomotion.exitTime = 0.9f;
            toLocomotion.duration = 0.1f;
        }

        EditorUtility.SetDirty(blendTree);
        EditorUtility.SetDirty(stateMachine);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("SD_AnimController configured: unarmed, greatsword and twin dagger locomotion.");
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
        bool hasWeaponStyle = controller.parameters.Any(parameter =>
            parameter.name == "WeaponStyle" && parameter.type == AnimatorControllerParameterType.Float);
        bool hasJump = controller.parameters.Any(parameter =>
            parameter.name == "Jump" && parameter.type == AnimatorControllerParameterType.Trigger);
        AnimatorState locomotion = FindState(controller.layers[0].stateMachine, "Locomotion");
        AnimatorState jump = FindState(controller.layers[0].stateMachine, "Jump");
        BlendTree tree = locomotion?.motion as BlendTree;

        return hasSpeed && hasWeaponStyle && hasJump && jump != null &&
               tree != null && tree.blendType == BlendTreeType.FreeformCartesian2D &&
               tree.children.Length == 9;
    }

    private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        if (!controller.parameters.Any(parameter => parameter.name == name && parameter.type == type))
            controller.AddParameter(name, type);
    }

    private static AnimatorState FindState(AnimatorStateMachine stateMachine, string name)
    {
        return stateMachine.states
            .Select(childState => childState.state)
            .FirstOrDefault(state => state.name == name);
    }

    private static AnimationClip LoadClip(string assetPath, string clipName)
    {
        return AssetDatabase.LoadAllAssetsAtPath(assetPath)
            .OfType<AnimationClip>()
            .FirstOrDefault(clip => clip.name == clipName);
    }
}
#endif
