using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class CharacterAnimationSetup
{
    private const string Base = "Assets/Characters/Gas Mask/";
    private const string PrefabPath = "Assets/Prefabs/Player.prefab";
    private const string ControllerPath = Base + "GasMaskAnimator.controller";

    [MenuItem("Tools/Character/Set Up Gas Mask Animations")]
    public static void SetUp()
    {
        string[] names = { "Breathing Idle", "Running", "Jumping Up", "Falling", "Falling Flat Impact", "Getting Up" };
        var clips = names.ToDictionary(name => name, name => GetClip(name));
        foreach (string name in names)
        {
            string path = Base + "Animations/Ch35_nonPBR@" + name + ".fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            var settings = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            if (settings.Length == 0) throw new Exception("No animation take in " + path);
            bool loop = name == "Breathing Idle" || name == "Running" || name == "Falling";
            if (settings[0].loopTime != loop)
            {
                settings[0].loopTime = loop;
                importer.clipAnimations = settings;
                importer.SaveAndReimport();
            }
            clips[name] = GetClip(name);
        }

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) throw new Exception("Missing controller " + ControllerPath);
        foreach (var layer in controller.layers)
        {
            foreach (var state in layer.stateMachine.states)
                layer.stateMachine.RemoveState(state.state);
        }
        foreach (var parameter in controller.parameters)
            controller.RemoveParameter(parameter);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        var machine = controller.layers[0].stateMachine;
        var tree = AssetDatabase.LoadAllAssetsAtPath(ControllerPath).OfType<BlendTree>()
            .FirstOrDefault(candidate => candidate.name == "Idle / Running");
        if (tree == null)
        {
            tree = new BlendTree { name = "Idle / Running" };
            AssetDatabase.AddObjectToAsset(tree, controller);
        }
        tree.children = Array.Empty<ChildMotion>();
        tree.blendParameter = "Speed";
        tree.useAutomaticThresholds = false;
        tree.AddChild(clips["Breathing Idle"], 0f);
        tree.AddChild(clips["Running"], 1f);
        var locomotion = machine.AddState("Locomotion");
        locomotion.motion = tree;
        machine.defaultState = locomotion;
        foreach (string name in names.Skip(2))
            machine.AddState(name).motion = clips[name];
        EditorUtility.SetDirty(tree);
        EditorUtility.SetDirty(controller);

        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            Transform visual = root.transform.Find("Gas Mask Visual");
            if (visual == null)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Base + "Ch35_nonPBR.fbx");
                if (model == null) throw new Exception("Gas mask FBX missing");
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
                instance.name = "Gas Mask Visual";
                visual = instance.transform;
            }
            visual.localPosition = new Vector3(0f, -1.1f, 0f);
            visual.localRotation = Quaternion.identity;
            visual.localScale = Vector3.one;
            Animator animator = visual.GetComponent<Animator>();
            if (animator == null) animator = visual.gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            var player = root.GetComponent<PlayerController>();
            player.animator = animator;
            player.jumpingClip = clips["Jumping Up"];
            player.impactClip = clips["Falling Flat Impact"];
            player.gettingUpClip = clips["Getting Up"];
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Debug.Log("Character animation setup complete.");
    }

    private static AnimationClip GetClip(string name)
    {
        string path = Base + "Animations/Ch35_nonPBR@" + name + ".fbx";
        AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
        if (clip == null) throw new Exception("Missing animation clip in " + path);
        return clip;
    }
}
