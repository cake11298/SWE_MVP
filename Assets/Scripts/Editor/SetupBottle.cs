using UnityEngine;
using UnityEditor;

public class SetupBottle
{
    public static void Execute()
    {
        GameObject bottle = GameObject.Find("WhiskeyBottle");
        if (bottle == null)
        {
            Debug.LogError("WhiskeyBottle not found!");
            return;
        }

        // 1. Set isStatic to false
        bottle.isStatic = false;
        Debug.Log("Set WhiskeyBottle isStatic to false.");

        // 2. Add BottleController
        BottleController controller = bottle.GetComponent<BottleController>();
        if (controller == null)
        {
            controller = bottle.AddComponent<BottleController>();
            Debug.Log("Added BottleController to WhiskeyBottle.");
        }

        // 3. Create PourPoint
        Transform pourPoint = bottle.transform.Find("PourPoint");
        if (pourPoint == null)
        {
            GameObject pp = new GameObject("PourPoint");
            pp.transform.SetParent(bottle.transform);
            pp.transform.localPosition = new Vector3(0, 0.35f, 0); // Adjusted height
            pp.transform.localRotation = Quaternion.identity;
            pourPoint = pp.transform;
            Debug.Log("Created PourPoint.");
        }

        // 4. Assign PourPoint to Controller
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("pourPoint").objectReferenceValue = pourPoint;
        so.ApplyModifiedProperties();
        Debug.Log("Assigned PourPoint to BottleController.");
        
        // 5. Ensure Collider
        if (bottle.GetComponent<Collider>() == null)
        {
             BoxCollider bc = bottle.AddComponent<BoxCollider>();
             bc.size = new Vector3(0.1f, 0.35f, 0.1f);
             bc.center = new Vector3(0, 0.175f, 0);
             Debug.Log("Added BoxCollider to WhiskeyBottle.");
        }
    }
}
