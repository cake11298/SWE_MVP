using UnityEngine;
using UnityEditor;
using BarSimulator.NPC;

public class AddSimpleNPCServe
{
    [MenuItem("Bar/Add SimpleNPCServe to NPCs")]
    public static void AddToNPCs()
    {
        Debug.Log("Adding SimpleNPCServe to NPCs...");

        // Find NPC01
        var npc01 = GameObject.Find("NPC01");
        if (npc01 != null && npc01.GetComponent<SimpleNPCServe>() == null)
        {
            npc01.AddComponent<SimpleNPCServe>();
            Debug.Log("Added SimpleNPCServe to NPC01");
        }

        // Find Gustave_NPC
        var gustave = GameObject.Find("Gustave_NPC");
        if (gustave != null && gustave.GetComponent<SimpleNPCServe>() == null)
        {
            gustave.AddComponent<SimpleNPCServe>();
            Debug.Log("Added SimpleNPCServe to Gustave_NPC");
        }

        // Find Seaton_NPC
        var seaton = GameObject.Find("Seaton_NPC");
        if (seaton != null && seaton.GetComponent<SimpleNPCServe>() == null)
        {
            seaton.AddComponent<SimpleNPCServe>();
            Debug.Log("Added SimpleNPCServe to Seaton_NPC");
        }

        Debug.Log("Done adding SimpleNPCServe components!");

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }
}
