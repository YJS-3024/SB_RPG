#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class SDAttackAnimatorConfigurator
{
    private const string ControllerPath = "Assets/_InProject/Resources/Anim/SD_AnimController.controller";
    private const string DaggerAssetPath = "Assets/Haons SD series Pack/Animation/WeaponMaster Twin dagger(WTD)/WTD_AttackA np.FBX";
    private const string GreatswordAssetPath = "Assets/Haons SD series Pack/Animation/WeaponMaster Greatsword(WGS)/WGS_AttackA_set.FBX";

    private static readonly string[] AttackClipNames =
    {
        "WTD_AttackA1 np", "WTD_AttackA2 np", "WTD_AttackA3 np",
        "WGS_attackA1", "WGS_attackA2", "WGS_attackA3", "WGS_attackA4", "WGS_attackA5"
    };

    static SDAttackAnimatorConfigurator()
    {
        EditorApplication.delayCall += ConfigureIfNeeded;
        EditorApplication.projectChanged += ConfigureIfNeeded;
    }

    [MenuItem("Tools/SB RPG/Configure SD Attack Animations")]
    public static void ConfigureIfNeeded()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null || controller.layers.Length == 0)
            return;

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState locomotion = FindState(stateMachine, "Locomotion");
        if (locomotion == null)
            return;

        AnimationClip[] clips = new AnimationClip[AttackClipNames.Length];
        for (int i = 0; i < clips.Length; i++)
        {
            string path = i < 3 ? DaggerAssetPath : GreatswordAssetPath;
            clips[i] = LoadClip(path, AttackClipNames[i]);
            if (clips[i] == null)
            {
                Debug.LogError("SD Animator attack clip was not found: " + AttackClipNames[i]);
                return;
            }
        }

        AnimationClip returnClip = LoadClip(GreatswordAssetPath, "WGS_attackA5toStand");
        if (returnClip == null)
        {
            Debug.LogError("SD Animator greatsword return clip was not found.");
            return;
        }

        bool changed = false;
        if (!controller.parameters.Any(parameter => parameter.name == "Attack" &&
            parameter.type == AnimatorControllerParameterType.Trigger))
        {
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            changed = true;
        }

        if (!controller.parameters.Any(parameter => parameter.name == "AttackVariant" &&
            parameter.type == AnimatorControllerParameterType.Int))
        {
            controller.AddParameter("AttackVariant", AnimatorControllerParameterType.Int);
            changed = true;
        }

        AnimatorState returnState = FindState(stateMachine, "Attack_Greatsword_Return");
        if (returnState == null)
        {
            returnState = stateMachine.AddState("Attack_Greatsword_Return", new Vector3(840f, 1000f));
            changed = true;
        }

        if (returnState.motion != returnClip)
        {
            returnState.motion = returnClip;
            changed = true;
        }

        if (!returnState.transitions.Any(transition => transition.destinationState == locomotion))
        {
            AddExitTransition(returnState, locomotion);
            changed = true;
        }

        if (!stateMachine.anyStateTransitions.Any(transition => transition.destinationState == returnState))
        {
            AnimatorStateTransition toReturn = stateMachine.AddAnyStateTransition(returnState);
            toReturn.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            toReturn.AddCondition(AnimatorConditionMode.Equals, 8f, "AttackVariant");
            toReturn.hasExitTime = false;
            toReturn.duration = 0.08f;
            toReturn.canTransitionToSelf = false;
            changed = true;
        }

        for (int i = 0; i < clips.Length; i++)
        {
            string stateName = i < 3
                ? "Attack_Dagger_" + (i + 1)
                : "Attack_Greatsword_" + (i - 2);
            AnimatorState attackState = FindState(stateMachine, stateName);
            if (attackState == null)
            {
                attackState = stateMachine.AddState(stateName,
                    new Vector3(i < 3 ? 320f : 600f, 600f + (i < 3 ? i : i - 3) * 100f));
                changed = true;
            }

            if (attackState.motion != clips[i])
            {
                attackState.motion = clips[i];
                changed = true;
            }

            AnimatorState exitState = i == clips.Length - 1 ? returnState : locomotion;
            if (!attackState.transitions.Any(transition => transition.destinationState == exitState))
            {
                AddExitTransition(attackState, exitState);
                changed = true;
            }

            if (!stateMachine.anyStateTransitions.Any(transition => transition.destinationState == attackState))
            {
                AnimatorStateTransition toAttack = stateMachine.AddAnyStateTransition(attackState);
                toAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
                toAttack.AddCondition(AnimatorConditionMode.Equals, i, "AttackVariant");
                toAttack.hasExitTime = false;
                toAttack.duration = 0.08f;
                toAttack.canTransitionToSelf = false;
                changed = true;
            }
        }

        if (!changed)
            return;

        EditorUtility.SetDirty(stateMachine);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(ControllerPath, ImportAssetOptions.ForceUpdate);
        Debug.Log("SD_AnimController configured: 3 twin dagger and 5 greatsword attacks.");
    }

    private static AnimatorState FindState(AnimatorStateMachine stateMachine, string name)
    {
        return stateMachine.states
            .Select(childState => childState.state)
            .FirstOrDefault(state => state.name == name);
    }

    private static AnimationClip LoadClip(string path, string name)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<AnimationClip>()
            .FirstOrDefault(clip => clip.name == name);
    }

    private static void AddExitTransition(AnimatorState from, AnimatorState to)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = 0.9f;
        transition.duration = 0.1f;
    }
}
#endif