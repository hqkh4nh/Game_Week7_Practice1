using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// Practice 1 (slide 3.1): tu dong lam buoc 2-4 sau khi da tha Y Bot.fbx va Swing Dancing.fbx vao Assets/Mixamo
public static class Practice1Setup
{
    const string MixamoFolder = "Assets/Mixamo";
    const string ControllerPath = "Assets/Animations/YBotAnimatorController.controller";
    const string ScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Week7/Setup Practice 1")]
    public static void Setup()
    {
        var fbxPaths = Directory.GetFiles(MixamoFolder, "*.fbx").Select(p => p.Replace('\\', '/')).ToArray();
        string modelPath = fbxPaths.FirstOrDefault(p => Path.GetFileName(p).ToLower().Contains("bot"));
        string animPath = fbxPaths.FirstOrDefault(p => p != modelPath);
        if (modelPath == null || animPath == null)
        {
            Debug.LogError("Can 2 file FBX trong Assets/Mixamo: Y Bot.fbx va Swing Dancing.fbx");
            return;
        }

        // Buoc 2: import model + animation, dat rig Humanoid, cho clip lap lai
        SetHumanoid(modelPath, false);
        SetHumanoid(animPath, true);

        var clip = AssetDatabase.LoadAllAssetsAtPath(animPath)
            .OfType<AnimationClip>()
            .First(c => !c.name.StartsWith("__preview__"));

        // Buoc 3: Animator Controller voi Idle <-> Swing Dancing
        AssetDatabase.DeleteAsset(ControllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Dance", AnimatorControllerParameterType.Bool);

        var sm = controller.layers[0].stateMachine;
        var idle = sm.AddState("Idle");
        var dance = sm.AddState("Swing Dancing");
        dance.motion = clip;
        sm.defaultState = idle;

        var toDance = idle.AddTransition(dance);
        toDance.hasExitTime = false;
        toDance.duration = 0.25f;
        toDance.AddCondition(AnimatorConditionMode.If, 0, "Dance");

        var toIdle = dance.AddTransition(idle);
        toIdle.hasExitTime = false;
        toIdle.duration = 0.25f;
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Dance");

        // Buoc 4: dua Y Bot vao scene va gan Animator
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var old = GameObject.Find("Y Bot");
        if (old != null) Object.DestroyImmediate(old);

        var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        var ybot = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
        ybot.name = "Y Bot";
        ybot.transform.position = Vector3.zero;
        ybot.transform.rotation = Quaternion.Euler(0, 180, 0);

        var animator = ybot.GetComponent<Animator>();
        if (animator == null) animator = ybot.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.avatar = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().First();
        if (ybot.GetComponent<DanceToggle>() == null) ybot.AddComponent<DanceToggle>();

        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 1.2f, -4f);
            cam.transform.rotation = Quaternion.Euler(5, 0, 0);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Practice 1 setup xong. Bam Play de xem Y Bot nhay Swing Dancing (Space: Idle <-> Dance).");
    }

    static void SetHumanoid(string path, bool loop)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(path);
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        if (loop)
        {
            var clips = importer.defaultClipAnimations;
            foreach (var c in clips) c.loopTime = true;
            importer.clipAnimations = clips;
        }
        importer.SaveAndReimport();
    }
}
