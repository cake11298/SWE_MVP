using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using BarSimulator.Objects;
using BarSimulator.Data;

public class BarSceneBuilder : MonoBehaviour
{
    public static void BuildScene()
    {
        // Create Parent Objects
        GameObject envRoot = CreateParent("Environment_Structure");
        GameObject barRoot = CreateParent("Bar_Area");
        GameObject furnitureRoot = CreateParent("Furniture");
        GameObject bottlesRoot = CreateParent("Bottles");

        // --- Materials ---
        Material floorMat = CreateMaterial("FloorMat_Custom", new Color(0.4f, 0.25f, 0.15f)); // Warm Brown
        Material wallMat = CreateMaterial("WallMat_Custom", HexToColor("5d4e37"));
        Material marbleMat = CreateMaterial("MarbleMat_Custom", HexToColor("e8e8e8"));
        Material rubberMat = CreateMaterial("RubberMat_Custom", new Color(0.1f, 0.1f, 0.1f));
        Material glassMat = CreateMaterial("GlassMat_Custom", new Color(0.8f, 0.9f, 1f, 0.3f));
        Material emissiveMat = CreateMaterial("LED_Mat", Color.white);
        emissiveMat.EnableKeyword("_EMISSION");
        emissiveMat.SetColor("_EmissionColor", Color.white * 2f);

        // --- Stage 1: Infrastructure ---

        // 1. Floor (20x20)
        CreatePrimitive(PrimitiveType.Cube, "Floor", envRoot, new Vector3(0, -0.5f, 0), new Vector3(20, 1, 20), floorMat);

        // 2. Walls (10m high)
        // Front (z=10), Back (z=-10), Left (x=-10), Right (x=10)
        // Thickness 0.5
        CreatePrimitive(PrimitiveType.Cube, "Wall_Front", envRoot, new Vector3(0, 5, 10.25f), new Vector3(20.5f, 10, 0.5f), wallMat);
        CreatePrimitive(PrimitiveType.Cube, "Wall_Back", envRoot, new Vector3(0, 5, -10.25f), new Vector3(20.5f, 10, 0.5f), wallMat);
        CreatePrimitive(PrimitiveType.Cube, "Wall_Left", envRoot, new Vector3(-10.25f, 5, 0), new Vector3(0.5f, 10, 20), wallMat);
        CreatePrimitive(PrimitiveType.Cube, "Wall_Right", envRoot, new Vector3(10.25f, 5, 0), new Vector3(0.5f, 10, 20), wallMat);

        // 3. Ceiling (10m high)
        CreatePrimitive(PrimitiveType.Cube, "Ceiling", envRoot, new Vector3(0, 10.5f, 0), new Vector3(20, 1, 20), wallMat);

        // 4. Bar Counter
        // Body: 12x1x2, z=-3
        CreatePrimitive(PrimitiveType.Cube, "Bar_Body", barRoot, new Vector3(0, 0.5f, -3), new Vector3(12, 1, 2), wallMat);
        // Top: 12.2x0.1x2.2
        CreatePrimitive(PrimitiveType.Cube, "Bar_Top", barRoot, new Vector3(0, 1.05f, -3), new Vector3(12.2f, 0.1f, 2.2f), marbleMat);
        // Mat: 4x0.03x2
        CreatePrimitive(PrimitiveType.Cube, "Bar_Mat", barRoot, new Vector3(0, 1.115f, -3), new Vector3(4, 0.03f, 2), rubberMat);

        // 5. Extended Lounge (20x10 extending back)
        // Assuming extending back means negative Z? Or positive? 
        // "Backwards" usually implies behind the bar or further into the scene. 
        // Let's assume it extends from z=-10 to z=-30.
        CreatePrimitive(PrimitiveType.Cube, "Lounge_Floor", envRoot, new Vector3(0, -0.5f, -20), new Vector3(20, 1, 20), floorMat);
        // Lounge Walls
        CreatePrimitive(PrimitiveType.Cube, "Lounge_Wall_Left", envRoot, new Vector3(-10.25f, 5, -20), new Vector3(0.5f, 10, 20), wallMat);
        CreatePrimitive(PrimitiveType.Cube, "Lounge_Wall_Right", envRoot, new Vector3(10.25f, 5, -20), new Vector3(0.5f, 10, 20), wallMat);
        CreatePrimitive(PrimitiveType.Cube, "Lounge_Wall_Back", envRoot, new Vector3(0, 5, -30.25f), new Vector3(20.5f, 10, 0.5f), wallMat);
        CreatePrimitive(PrimitiveType.Cube, "Lounge_Ceiling", envRoot, new Vector3(0, 10.5f, -20), new Vector3(20, 1, 20), wallMat);


        // --- Stage 2: Shelves & Bottles ---

        // 1. Main Liquor Shelf (z=-8)
        GameObject mainShelf = CreateParent("Main_Shelf", furnitureRoot);
        mainShelf.transform.position = new Vector3(0, 0, -8);
        
        // Structure
        CreatePrimitive(PrimitiveType.Cube, "Backing", mainShelf, new Vector3(0, 2.5f, 0.5f), new Vector3(8, 5, 0.5f), wallMat);
        
        float[] shelfHeights = new float[] { 1.5f, 2.7f, 3.9f };
        string[] mainLiquors = new string[] { "gin", "vodka", "whiskey", "tequila", "rum" };
        
        for (int i = 0; i < shelfHeights.Length; i++)
        {
            float h = shelfHeights[i];
            // Shelf
            CreatePrimitive(PrimitiveType.Cube, $"Shelf_{i}", mainShelf, new Vector3(0, h, 0), new Vector3(7.8f, 0.1f, 0.5f), marbleMat);
            // LED
            CreatePrimitive(PrimitiveType.Cube, $"LED_{i}", mainShelf, new Vector3(0, h - 0.06f, 0.2f), new Vector3(7.8f, 0.02f, 0.05f), emissiveMat);
            
            // Bottles (8 per shelf)
            float startX = -3.5f;
            float spacing = 1.0f;
            for (int j = 0; j < 8; j++)
            {
                string type = mainLiquors[(i * 8 + j) % mainLiquors.Length];
                CreateBottle(type, new Vector3(startX + j * spacing, h + 0.3f, -8), bottlesRoot);
            }
        }

        // 2. Ingredient Cabinet (Left Wall, x=-8.5)
        GameObject leftCabinet = CreateParent("Ingredient_Cabinet", furnitureRoot);
        leftCabinet.transform.position = new Vector3(-8.5f, 0, -3); // Aligned with bar roughly
        leftCabinet.transform.rotation = Quaternion.Euler(0, 90, 0);

        // Structure
        CreatePrimitive(PrimitiveType.Cube, "Glass_Case", leftCabinet, new Vector3(0, 2, 0), new Vector3(3, 4, 0.5f), glassMat);
        
        string[][] ingredients = new string[][]
        {
            new string[] { "vermouth_dry", "vermouth_sweet", "campari", "triple_sec" },
            new string[] { "lemon_juice", "lime_juice", "simple_syrup", "grenadine" },
            new string[] { "pineapple_juice", "coconut_cream", "orange_juice", "cranberry_juice" }
        };

        for (int i = 0; i < 3; i++)
        {
            float h = 1.0f + i * 1.0f;
            // Shelf
            CreatePrimitive(PrimitiveType.Cube, $"Shelf_{i}", leftCabinet, new Vector3(0, h, 0), new Vector3(2.8f, 0.05f, 0.4f), glassMat);
            // LED
            CreatePrimitive(PrimitiveType.Cube, $"LED_{i}", leftCabinet, new Vector3(0, h - 0.03f, 0), new Vector3(2.8f, 0.02f, 0.05f), emissiveMat);

            // Bottles
            float startX = -1.0f; // Local X (which is world Z due to rotation)
            float spacing = 0.6f;
            for (int j = 0; j < 4; j++)
            {
                if (j < ingredients[i].Length)
                {
                    // Calculate world position
                    Vector3 localPos = new Vector3(startX + j * spacing, h + 0.3f, 0);
                    Vector3 worldPos = leftCabinet.transform.TransformPoint(localPos);
                    CreateBottle(ingredients[i][j], worldPos, bottlesRoot);
                }
            }
        }

        // 3. Premium Display (Right, x=9.5)
        GameObject rightCabinet = CreateParent("Premium_Cabinet", furnitureRoot);
        rightCabinet.transform.position = new Vector3(9.5f, 0, -3);
        rightCabinet.transform.rotation = Quaternion.Euler(0, -90, 0);

        CreatePrimitive(PrimitiveType.Cube, "Glass_Case", rightCabinet, new Vector3(0, 2, 0), new Vector3(2, 4, 0.5f), glassMat);
        CreatePrimitive(PrimitiveType.Cube, "Shelf_Premium", rightCabinet, new Vector3(0, 2, 0), new Vector3(1.8f, 0.05f, 0.4f), glassMat);
        CreatePrimitive(PrimitiveType.Cube, "LED_Premium", rightCabinet, new Vector3(0, 1.97f, 0), new Vector3(1.8f, 0.02f, 0.05f), emissiveMat);

        // Premium Bottles
        CreateBottle("whiskey", rightCabinet.transform.TransformPoint(new Vector3(-0.3f, 2.3f, 0)), bottlesRoot, "Macallan 25");
        CreateBottle("brandy", rightCabinet.transform.TransformPoint(new Vector3(0.3f, 2.3f, 0)), bottlesRoot, "Dom Pérignon");
    }

    private static GameObject CreateParent(string name, GameObject parent = null)
    {
        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent.transform);
        return go;
    }

    private static GameObject CreatePrimitive(PrimitiveType type, string name, GameObject parent, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = pos; // Use local position if parented, but logic above uses world coords mostly. 
        // Wait, if I set parent, localPosition is relative.
        // My logic above uses world coordinates for positions (e.g. Walls).
        // So I should set position AFTER parenting if I want world, or calculate local.
        // To be safe, let's set position in World Space then parent, OR set parent then set position (which sets local).
        // The coordinates I defined (e.g. Wall at 10.25) are clearly World coordinates relative to origin.
        // So:
        go.transform.SetParent(null);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.transform.SetParent(parent.transform, true); // Keep world position
        
        if (mat != null)
        {
            go.GetComponent<Renderer>().material = mat;
        }
        return go;
    }

    private static void CreateBottle(string liquorId, Vector3 pos, GameObject parent, string customName = null)
    {
        GameObject bottle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bottle.name = customName ?? $"Bottle_{liquorId}";
        bottle.transform.position = pos;
        bottle.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f); // Bottle size
        bottle.transform.SetParent(parent.transform, true);

        // Add Components
        Rigidbody rb = bottle.AddComponent<Rigidbody>();
        rb.mass = 1.0f;
        
        // Bottle Script
        Bottle bottleScript = bottle.AddComponent<Bottle>();
        bottleScript.SetLiquor(liquorId);
        
        // Label (Simple TextMesh or just color for now)
        // Let's color the bottle based on liquor
        // We need to get color from database, but database is runtime.
        // We can try to set a default color or let Bottle script handle it on Start.
        // Bottle script has Initialize(LiquorData), but we are in Editor (or Runtime?).
        // If this runs in Editor, Bottle.Start() won't run.
        // We can manually set color if we want.
        
        // Add a label object
        GameObject label = new GameObject("Label");
        label.transform.SetParent(bottle.transform);
        label.transform.localPosition = new Vector3(0, 0, -0.51f); // Front
        label.transform.localScale = new Vector3(0.8f, 0.3f, 0.1f);
        // We could add TextMesh here if needed, but simple geometry is fine for now.
    }

    private static Material CreateMaterial(string name, Color color)
    {
        // Check if exists
        string path = $"Assets/Materials/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
            mat.SetColor("_BaseColor", color);
        }
        return mat;
    }

    private static Color HexToColor(string hex)
    {
        Color color = Color.white;
        ColorUtility.TryParseHtmlString("#" + hex, out color);
        return color;
    }
}
