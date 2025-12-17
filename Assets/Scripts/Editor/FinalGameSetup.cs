using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using BarSimulator.Managers;
using System.Collections.Generic;
using System.Linq;

public class FinalGameSetup
{
    public static void Execute()
    {
        SetupTheBarScene();
        SetupBuildSettings();
    }

    private static void SetupTheBarScene()
    {
        string scenePath = "Assets/SceneS/TheBar.unity";
        EditorSceneManager.OpenScene(scenePath);

        // 1. Setup DecorationManager
        DecorationManager decManager = Object.FindFirstObjectByType<DecorationManager>();
        if (decManager == null)
        {
            GameObject managerObj = new GameObject("DecorationManager");
            decManager = managerObj.AddComponent<DecorationManager>();
        }

        // Find Props
        GameObject props = GameObject.Find("Props");
        if (props != null)
        {
            // Setup Speakers
            Transform speakersParent = props.transform.Find("Speakers");
            if (speakersParent == null)
            {
                GameObject speakersObj = new GameObject("Speakers");
                speakersParent = speakersObj.transform;
                speakersParent.SetParent(props.transform);
                
                // Find all speakers and move them
                var allTransforms = props.GetComponentsInChildren<Transform>(true);
                foreach (var t in allTransforms)
                {
                    if (t != null && t.parent == props.transform && t.name.Contains("Speaker"))
                    {
                        t.SetParent(speakersParent);
                    }
                }
            }

            // Setup Plants (Bamboo)
            Transform plantsParent = props.transform.Find("Plants");
            if (plantsParent == null)
            {
                GameObject plantsObj = new GameObject("Plants");
                plantsParent = plantsObj.transform;
                plantsParent.SetParent(props.transform);

                // Find all bamboo and move them
                var allTransforms = props.GetComponentsInChildren<Transform>(true);
                foreach (var t in allTransforms)
                {
                    if (t != null && t.parent == props.transform && t.name.Contains("Bamboo"))
                    {
                        t.SetParent(plantsParent);
                    }
                }
            }

            // Setup Paintings (SM_PannelFrame)
            Transform paintingsParent = props.transform.Find("Paintings");
            if (paintingsParent == null)
            {
                GameObject paintingsObj = new GameObject("Paintings");
                paintingsParent = paintingsObj.transform;
                paintingsParent.SetParent(props.transform);

                // Find all paintings and move them
                var allTransforms = props.GetComponentsInChildren<Transform>(true);
                foreach (var t in allTransforms)
                {
                    if (t != null && t.parent == props.transform && t.name.Contains("SM_PannelFrame"))
                    {
                        t.SetParent(paintingsParent);
                    }
                }
            }

            // Assign to Manager
            SerializedObject so = new SerializedObject(decManager);
            so.FindProperty("speakersParent").objectReferenceValue = speakersParent.gameObject;
            so.FindProperty("plantsParent").objectReferenceValue = plantsParent.gameObject;
            so.FindProperty("paintingsParent").objectReferenceValue = paintingsParent.gameObject;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("TheBar Scene Setup Complete.");
    }

    private static void SetupBuildSettings()
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        
        string[] requiredScenes = new string[] 
        { 
            "Assets/SceneS/MainMenu.unity",
            "Assets/SceneS/TheBar.unity",
            "Assets/SceneS/GameEnd.unity"
        };

        bool changed = false;
        foreach (var path in requiredScenes)
        {
            if (!scenes.Any(s => s.path == path))
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
                changed = true;
                Debug.Log($"Added {path} to Build Settings.");
            }
        }

        if (changed)
        {
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
