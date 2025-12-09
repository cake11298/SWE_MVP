using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrefabBarBuilder : EditorWindow
{
    [MenuItem("Tools/Build Bar Scene")]
    public static void ShowWindow()
    {
        GetWindow<PrefabBarBuilder>("Bar Builder");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Build Complete Bar"))
        {
            BuildScene();
        }
    }

    public static void BuildScene()
    {
        // Cleanup existing
        GameObject existing = GameObject.Find("Bar_Environment_Complete");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }

        // Create Root
        GameObject root = new GameObject("Bar_Environment_Complete");
        Undo.RegisterCreatedObjectUndo(root, "Build Bar Scene");

        // Load Prefabs
        GameObject floorPrefab = LoadPrefab("SM_FloorBlock");
        GameObject wallPrefab = LoadPrefab("SM_WallBlock");
        GameObject doorPrefab = LoadPrefab("SM_Door");
        GameObject doorFramePrefab = LoadPrefab("SM_DoorFrame");
        
        GameObject barStoolPrefab = LoadPrefab("SM_BarchairBlackS");
        GameObject tablePrefab = LoadPrefab("SM_Tablel"); // For counter
        
        GameObject shelfPrefab = LoadPrefab("SM_Shelve2");
        
        GameObject whiskey1 = LoadPrefab("SM_WhiskeyBottle1");
        GameObject whiskey2 = LoadPrefab("SM_WhiskeyBottle2");
        GameObject whiskey3 = LoadPrefab("SM_WhiskeyBottle3");
        GameObject wine1 = LoadPrefab("SM_WineBottle1");
        GameObject wine2 = LoadPrefab("SM_WineBottle2");
        GameObject wine3 = LoadPrefab("SM_WineBottle3");
        
        GameObject martiniGlass = LoadPrefab("SM_MartiniGlassS");
        GameObject cashDesk = LoadPrefab("SM_CashDeskS");
        GameObject coaster = LoadPrefab("SM_RubberCoaster");
        
        GameObject coffeeTable = LoadPrefab("SM_CoffeeTableWoodBrownS");
        GameObject sofa = LoadPrefab("SM_SofaLeatherBlack");
        GameObject chair = LoadPrefab("SM_ChairLeather");
        
        GameObject poster = LoadPrefab("SM_LogoPoster");
        GameObject speaker = LoadPrefab("SM_Speakers");
        GameObject bush = LoadPrefab("SM_bush2");
        GameObject lamp = LoadPrefab("SM_StandingLampS");
        GameObject floorLight = LoadPrefab("SM_FloorLight");

        // --- Step 1: Floor & Walls ---
        GameObject structureRoot = CreateChild("Structure", root);
        
        // Floor (10x10 grid of 2x2m blocks)
        if (floorPrefab != null)
        {
            for (int x = -5; x < 5; x++)
            {
                for (int z = -5; z < 5; z++)
                {
                    Vector3 pos = new Vector3(x * 2 + 1, 0, z * 2 + 1);
                    Spawn(floorPrefab, pos, Quaternion.identity, structureRoot);
                    
                    // Ceiling (flipped floor)
                    Spawn(floorPrefab, new Vector3(x * 2 + 1, 10, z * 2 + 1), Quaternion.Euler(180, 0, 0), structureRoot);
                }
            }
        }

        // Walls
        // Back Wall (z=-10)
        BuildWall(wallPrefab, new Vector3(0, 0, -10), 20, 10, structureRoot, 0);
        
        // Left Wall (x=-10)
        BuildWall(wallPrefab, new Vector3(-10, 0, 0), 20, 10, structureRoot, 90);
        
        // Right Wall (x=10)
        BuildWall(wallPrefab, new Vector3(10, 0, 0), 20, 10, structureRoot, -90);
        
        // Front Wall (z=10) - with Door
        if (doorFramePrefab != null)
            Spawn(doorFramePrefab, new Vector3(0, 0, 10), Quaternion.Euler(0, 180, 0), structureRoot);
        if (doorPrefab != null)
            Spawn(doorPrefab, new Vector3(0, 0, 10), Quaternion.Euler(0, 180, 0), structureRoot);
        
        // Fill walls around door (Simplified: just build full walls and let door clip or be in front)
        // Ideally we should leave a gap.
        // Wall from x=-10 to -1 (9m)
        // Wall from x=1 to 10 (9m)
        // Wall above door (y=2 to 10)
        
        // Left side of door
        BuildWallSection(wallPrefab, new Vector3(-5.5f, 0, 10), 9, 10, structureRoot, 180);
        // Right side of door
        BuildWallSection(wallPrefab, new Vector3(5.5f, 0, 10), 9, 10, structureRoot, 180);
        // Above door
        BuildWallSection(wallPrefab, new Vector3(0, 2.5f, 10), 2, 7.5f, structureRoot, 180);


        // --- Step 2: Bar Counter ---
        GameObject furnitureRoot = CreateChild("Furniture", root);
        
        // Counter: 12m x 1m x 2m at (0, 0, -3)
        if (tablePrefab != null)
        {
            // Assuming table is approx 2m long. We need 6 of them.
            for (int i = -3; i < 3; i++)
            {
                Spawn(tablePrefab, new Vector3(i * 2 + 1, 0, -3), Quaternion.identity, furnitureRoot);
            }
        }
        
        // Stools
        if (barStoolPrefab != null)
        {
            Spawn(barStoolPrefab, new Vector3(-4, 0, -1), Quaternion.Euler(0, 180, 0), furnitureRoot);
            Spawn(barStoolPrefab, new Vector3(-1.5f, 0, -1), Quaternion.Euler(0, 180, 0), furnitureRoot);
            Spawn(barStoolPrefab, new Vector3(1.5f, 0, -1), Quaternion.Euler(0, 180, 0), furnitureRoot);
            Spawn(barStoolPrefab, new Vector3(4, 0, -1), Quaternion.Euler(0, 180, 0), furnitureRoot);
        }

        // --- Step 3: Back Bar Shelving ---
        GameObject shelfRoot = CreateChild("Shelves", furnitureRoot);
        Vector3 shelfPos = new Vector3(0, 0, -8);
        
        if (shelfPrefab != null)
        {
            Spawn(shelfPrefab, shelfPos + Vector3.up * 1.5f, Quaternion.identity, shelfRoot);
            Spawn(shelfPrefab, shelfPos + Vector3.up * 2.5f, Quaternion.identity, shelfRoot);
            Spawn(shelfPrefab, shelfPos + Vector3.up * 3.5f, Quaternion.identity, shelfRoot);
        }
        
        // Bottles
        GameObject bottleRoot = CreateChild("Bottles", root);
        GameObject[] bottles = new GameObject[] { whiskey1, whiskey2, whiskey3, wine1, wine2, wine3 };
        
        // Fill shelves
        float[] shelfHeights = new float[] { 1.5f, 2.5f, 3.5f };
        for (int h = 0; h < shelfHeights.Length; h++)
        {
            for (int i = 0; i < 8; i++)
            {
                GameObject bPrefab = bottles[(h * 8 + i) % bottles.Length];
                if (bPrefab != null)
                {
                    float x = -3.5f + i * 1.0f;
                    Spawn(bPrefab, new Vector3(x, shelfHeights[h] + 0.2f, -8), Quaternion.identity, bottleRoot);
                }
            }
        }

        // --- Step 4: Working Area ---
        if (martiniGlass != null)
        {
            Spawn(martiniGlass, new Vector3(-1, 1.0f, -3), Quaternion.identity, bottleRoot);
            Spawn(martiniGlass, new Vector3(0, 1.0f, -3), Quaternion.identity, bottleRoot);
            Spawn(martiniGlass, new Vector3(1, 1.0f, -3), Quaternion.identity, bottleRoot);
        }
        
        if (cashDesk != null)
            Spawn(cashDesk, new Vector3(5, 1.0f, -3), Quaternion.Euler(0, 180, 0), furnitureRoot);
        
        if (coaster != null)
        {
            Spawn(coaster, new Vector3(-1, 1.0f, -3), Quaternion.identity, furnitureRoot);
            Spawn(coaster, new Vector3(0, 1.0f, -3), Quaternion.identity, furnitureRoot);
            Spawn(coaster, new Vector3(1, 1.0f, -3), Quaternion.identity, furnitureRoot);
        }
        
        // --- Step 5: Lounge ---
        GameObject loungeRoot = CreateChild("Lounge", furnitureRoot);
        if (coffeeTable != null)
            Spawn(coffeeTable, new Vector3(0, 0, 8), Quaternion.identity, loungeRoot);
        if (sofa != null)
            Spawn(sofa, new Vector3(0, 0, 10), Quaternion.Euler(0, 180, 0), loungeRoot);
        if (chair != null)
        {
            Spawn(chair, new Vector3(-3, 0, 8), Quaternion.Euler(0, 90, 0), loungeRoot);
            Spawn(chair, new Vector3(3, 0, 8), Quaternion.Euler(0, -90, 0), loungeRoot);
        }

        // --- Step 6: Decorations ---
        GameObject decorRoot = CreateChild("Decorations", root);
        if (poster != null)
            Spawn(poster, new Vector3(0, 3, -9.9f), Quaternion.identity, decorRoot);
        if (speaker != null)
            Spawn(speaker, new Vector3(8, 2, -8), Quaternion.Euler(0, -45, 0), decorRoot);
        if (bush != null)
        {
            Spawn(bush, new Vector3(9, 0, 9), Quaternion.identity, decorRoot);
            Spawn(bush, new Vector3(-9, 0, 9), Quaternion.identity, decorRoot);
        }
        if (lamp != null)
            Spawn(lamp, new Vector3(-4, 0, 9), Quaternion.identity, decorRoot);
        if (floorLight != null)
        {
            for (int i = -5; i <= 5; i+=2)
            {
                Spawn(floorLight, new Vector3(i, 0, -2.5f), Quaternion.identity, decorRoot);
            }
        }
        
        // --- Step 7: Lighting ---
        GameObject lightRoot = CreateChild("Lights", root);
        
        // Setup Skybox
        Material skyboxMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Settings/Fresh_URP_Skybox.mat");
        if (skyboxMat != null)
        {
            RenderSettings.skybox = skyboxMat;
        }
        
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        // Much brighter ambient light
        RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f); 
        
        // Main Light (Brighter and larger range)
        CreateLight("MainLight", new Vector3(0, 9, 0), 40, 2.0f, new Color(1f, 0.98f, 0.94f), lightRoot);
        
        // Fill Lights - Increased intensity
        CreateLight("FillLight_Front", new Vector3(0, 7, 5), 25, 1.5f, new Color(1f, 0.95f, 0.9f), lightRoot);
        CreateLight("FillLight_Back", new Vector3(0, 7, -5), 25, 1.5f, new Color(1f, 0.95f, 0.9f), lightRoot);
        
        // Spotlights for atmosphere
        CreateSpotLight("BarSpot", new Vector3(0, 9, -3), new Vector3(90, 0, 0), 60, 10.0f, Color.white, lightRoot);
        
        CreateLight("ShelfLight", new Vector3(0, 5, -7), 12, 3.0f, new Color(1f, 0.94f, 0.86f), lightRoot);
        CreateLight("LoungeLight", new Vector3(0, 5, 8), 15, 3.0f, new Color(1f, 0.9f, 0.8f), lightRoot);
        
        // Reflection Probe
        GameObject probeGo = new GameObject("ReflectionProbe");
        probeGo.transform.position = new Vector3(0, 2, 0);
        probeGo.transform.SetParent(root.transform);
        ReflectionProbe probe = probeGo.AddComponent<ReflectionProbe>();
        probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
        probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
        probe.boxProjection = true;
        probe.size = new Vector3(20, 10, 20);
    }

    private static GameObject LoadPrefab(string name)
    {
        string[] guids = AssetDatabase.FindAssets(name + " t:Prefab");
        if (guids.Length == 0) 
        {
            Debug.LogWarning($"Prefab not found: {name}");
            return null;
        }
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot, GameObject parent)
    {
        if (prefab == null) return null;
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = pos;
        go.transform.rotation = rot;
        go.transform.SetParent(parent.transform);
        return go;
    }

    private static GameObject CreateChild(string name, GameObject parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        return go;
    }
    
    private static void BuildWall(GameObject prefab, Vector3 center, float width, float height, GameObject parent, float yRotation)
    {
        if (prefab == null) return;
        
        // SM_WallBlock is approx 0.5m wide x 2m high
        float blockWidth = 0.5f;
        float blockHeight = 2.0f;
        
        int cols = Mathf.CeilToInt(width / blockWidth);
        int rows = Mathf.CeilToInt(height / blockHeight);
        
        GameObject wallRoot = new GameObject("WallSection");
        wallRoot.transform.position = center;
        wallRoot.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        wallRoot.transform.SetParent(parent.transform);
        
        float startX = -(width / 2f) + (blockWidth / 2f);
        
        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.SetParent(wallRoot.transform);
                go.transform.localPosition = new Vector3(startX + c * blockWidth, r * blockHeight, 0);
                go.transform.localRotation = Quaternion.identity;
            }
        }
    }
    
    private static void BuildWallSection(GameObject prefab, Vector3 center, float width, float height, GameObject parent, float yRotation)
    {
        BuildWall(prefab, center, width, height, parent, yRotation);
    }

    private static void CreateLight(string name, Vector3 pos, float range, float intensity, Color color, GameObject parent)
    {
        GameObject go = new GameObject(name);
        go.transform.position = pos;
        go.transform.SetParent(parent.transform);
        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = range;
        light.intensity = intensity;
        light.color = color;
        light.shadows = LightShadows.Soft;
    }

    private static void CreateSpotLight(string name, Vector3 pos, Vector3 rot, float angle, float intensity, Color color, GameObject parent)
    {
        GameObject go = new GameObject(name);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(rot);
        go.transform.SetParent(parent.transform);
        Light light = go.AddComponent<Light>();
        light.type = LightType.Spot;
        light.spotAngle = angle;
        light.intensity = intensity;
        light.color = color;
        light.shadows = LightShadows.Soft;
        light.range = 20f;
    }
}
