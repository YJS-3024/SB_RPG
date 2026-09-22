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
    private const string DashAssetPath = "Assets/Haons SD series Pack/Animation/Action Adventure(Adv)/Dash_Front.FBX";
    private const string SideDashAssetPath = "Assets/Haons SD series Pack/Animation/Action Adventure(Adv)/SideStep_Move.fbx";
    private const string HitAssetPath = "Assets/Haons SD series Pack/Animation/Action Adventure(Adv)/Damaged_ToBack.FBX";
    private const string DeathAssetPath = "Assets/Haons SD series Pack/Animation/Action Adventure(Adv)/Die.fbx";

    static SDAnimControllerConfigurator()
    {
        EditorApplication.delayCall += ConfigureIfNeeded;
    }

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
        AnimationClip dash = LoadClip(DashAssetPath, "Dash_Front@loop");
        AnimationClip sideDashLeft = LoadClip(SideDashAssetPath, "SideStep_LeftMove@loop");
        AnimationClip sideDashRight = LoadClip(SideDashAssetPath, "SideStep_RightMove@loop");
        AnimationClip hit = LoadClip(HitAssetPath, "Damaged_ToBack");
        AnimationClip death = LoadClip(DeathAssetPath, "Die");

        if (controller == null || controller.layers.Length == 0 ||
            unarmed.Any(clip => clip == null) || greatsword.Any(clip => clip == null) || twinDagger.Any(clip => clip == null) || jump == null || dash == null || sideDashLeft == null || sideDashRight == null || hit == null || death == null)
        {
            Debug.LogError("SD Animator configuration failed: controller or movement clip was not found.");
            return;
        }

        EnsureParameter(controller, "Speed", AnimatorControllerParameterType.Float);
        EnsureParameter(controller, "WeaponStyle", AnimatorControllerParameterType.Float);
        EnsureParameter(controller, "Jump", AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, "Dash", AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, "DashLeft", AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, "DashRight", AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, "Hit", AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, "Die", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState locomotion = FindState(stateMachine, "Locomotion");
        if (locomotion == null)
            locomotion = stateMachine.AddState("Locomotion", new Vector3(320f, 120f));

        AnimatorState jumpState = FindState(stateMachine, "Jump");
        if (jumpState == null)
            jumpState = stateMachine.AddState("Jump", new Vector3(580f, 120f));

        AnimatorState dashState = FindState(stateMachine, "Dash");
        if (dashState == null)
            dashState = stateMachine.AddState("Dash", new Vector3(840f, 120f));

        AnimatorState sideDashLeftState = FindState(stateMachine, "Dash_Left");
        if (sideDashLeftState == null)
            sideDashLeftState = stateMachine.AddState("Dash_Left", new Vector3(840f, 240f));

        AnimatorState sideDashRightState = FindState(stateMachine, "Dash_Right");
        if (sideDashRightState == null)
            sideDashRightState = stateMachine.AddState("Dash_Right", new Vector3(840f, 360f));

        AnimatorState hitState = FindState(stateMachine, "Hit");
        if (hitState == null)
            hitState = stateMachine.AddState("Hit", new Vector3(1120f, 120f));

        AnimatorState deathState = FindState(stateMachine, "Death");
        if (deathState == null)
            deathState = stateMachine.AddState("Death", new Vector3(1120f, 260f));

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
        dashState.motion = dash;
        sideDashLeftState.motion = sideDashLeft;
        sideDashRightState.motion = sideDashRight;
        hitState.motion = hit;
        deathState.motion = death;
        sideDashLeftState.speed = sideDashLeft.length / 0.267f;
        sideDashRightState.speed = sideDashRight.length / 0.267f;
        if (!stateMachine.anyStateTransitions.Any(transition => transition.destinationState == jumpState))
        {
            AnimatorStateTransition toJump = stateMachine.AddAnyStateTransition(jumpState);
            toJump.AddCondition(AnimatorConditionMode.If, 0f, "Jump");
            toJump.hasExitTime = false;
            toJump.duration = 0.1f;
            toJump.canTransitionToSelf = false;
        }

        if (!stateMachine.anyStateTransitions.Any(transition => transition.destinationState == dashState))
        {
            AnimatorStateTransition toDash = stateMachine.AddAnyStateTransition(dashState);
            toDash.AddCondition(AnimatorConditionMode.If, 0f, "Dash");
            toDash.hasExitTime = false;
            toDash.duration = 0.05f;
            toDash.canTransitionToSelf = false;
        }

        if (!stateMachine.anyStateTransitions.Any(transition => transition.destinationState == sideDashLeftState))
        {
            AnimatorStateTransition toLeft = stateMachine.AddAnyStateTransition(sideDashLeftState);
            toLeft.AddCondition(AnimatorConditionMode.If, 0f, "DashLeft");
            toLeft.hasExitTime = false;
            toLeft.duration = 0.05f;
            toLeft.canTransitionToSelf = false;
        }

        if (!stateMachine.anyStateTransitions.Any(transition => transition.destinationState == sideDashRightState))
        {
            AnimatorStateTransition toRight = stateMachine.AddAnyStateTransition(sideDashRightState);
            toRight.AddCondition(AnimatorConditionMode.If, 0f, "DashRight");
            toRight.hasExitTime = false;
            toRight.duration = 0.05f;
            toRight.canTransitionToSelf = false;
        }

        if (!sideDashLeftState.transitions.Any(transition => transition.destinationState == locomotion))
        {
            AnimatorStateTransition fromLeft = sideDashLeftState.AddTransition(locomotion);
            fromLeft.hasExitTime = true;
            fromLeft.exitTime = 1f;
            fromLeft.duration = 0.08f;
        }

        if (!sideDashRightState.transitions.Any(transition => transition.destinationState == locomotion))
        {
            AnimatorStateTransition fromRight = sideDashRightState.AddTransition(locomotion);
            fromRight.hasExitTime = true;
            fromRight.exitTime = 1f;
            fromRight.duration = 0.08f;
        }

        if (!dashState.transitions.Any(transition => transition.destinationState == locomotion))
        {
            AnimatorStateTransition fromDash = dashState.AddTransition(locomotion);
            fromDash.hasExitTime = true;
            fromDash.exitTime = 1f;
            fromDash.duration = 0.08f;
        }

        if (!jumpState.transitions.Any(transition => transition.destinationState == locomotion))
        {
            AnimatorStateTransition toLocomotion = jumpState.AddTransition(locomotion);
            toLocomotion.hasExitTime = true;
            toLocomotion.exitTime = 0.9f;
            toLocomotion.duration = 0.1f;
        }

        if (!stateMachine.anyStateTransitions.Any(transition => transition.destinationState == hitState))
        {
            AnimatorStateTransition toHit = stateMachine.AddAnyStateTransition(hitState);
            toHit.AddCondition(AnimatorConditionMode.If, 0f, "Hit");
            toHit.hasExitTime = false;
            toHit.duration = 0.05f;
            toHit.canTransitionToSelf = false;
        }

        if (!hitState.transitions.Any(transition => transition.destinationState == locomotion))
        {
            AnimatorStateTransition fromHit = hitState.AddTransition(locomotion);
            fromHit.hasExitTime = true;
            fromHit.exitTime = 0.95f;
            fromHit.duration = 0.08f;
        }

        if (!stateMachine.anyStateTransitions.Any(transition => transition.destinationState == deathState))
        {
            AnimatorStateTransition toDeath = stateMachine.AddAnyStateTransition(deathState);
            toDeath.AddCondition(AnimatorConditionMode.If, 0f, "Die");
            toDeath.hasExitTime = false;
            toDeath.duration = 0.05f;
            toDeath.canTransitionToSelf = false;
        }

        EditorUtility.SetDirty(blendTree);
        EditorUtility.SetDirty(stateMachine);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("SD_AnimController configured: locomotion, jump, dashes, hit and death.");
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
        bool hasDashLeft = controller.parameters.Any(parameter =>
            parameter.name == "DashLeft" && parameter.type == AnimatorControllerParameterType.Trigger);
        bool hasDashRight = controller.parameters.Any(parameter =>
            parameter.name == "DashRight" && parameter.type == AnimatorControllerParameterType.Trigger);
        bool hasDash = controller.parameters.Any(parameter =>
            parameter.name == "Dash" && parameter.type == AnimatorControllerParameterType.Trigger);
        bool hasJump = controller.parameters.Any(parameter =>
            parameter.name == "Jump" && parameter.type == AnimatorControllerParameterType.Trigger);
        bool hasHit = controller.parameters.Any(parameter =>
            parameter.name == "Hit" && parameter.type == AnimatorControllerParameterType.Trigger);
        bool hasDie = controller.parameters.Any(parameter =>
            parameter.name == "Die" && parameter.type == AnimatorControllerParameterType.Trigger);
        AnimatorState locomotion = FindState(controller.layers[0].stateMachine, "Locomotion");
        AnimatorState jump = FindState(controller.layers[0].stateMachine, "Jump");
        AnimatorState dash = FindState(controller.layers[0].stateMachine, "Dash");
        AnimatorState dashLeft = FindState(controller.layers[0].stateMachine, "Dash_Left");
        AnimatorState dashRight = FindState(controller.layers[0].stateMachine, "Dash_Right");
        AnimatorState hit = FindState(controller.layers[0].stateMachine, "Hit");
        AnimatorState death = FindState(controller.layers[0].stateMachine, "Death");
        BlendTree tree = locomotion?.motion as BlendTree;

        return hasSpeed && hasWeaponStyle && hasJump && hasDash && hasDashLeft && hasDashRight && hasHit && hasDie &&
               jump != null && dash != null && dashLeft != null && dashRight != null &&
               hit != null && hit.motion != null && death != null && death.motion != null &&
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
