using UnityEngine;
using UnityEditor;
using BarSimulator.Player;
using BarSimulator.Objects;
using BarSimulator.Data;

/// <summary>
/// Comprehensive script to fix all interactable objects in the scene
/// </summary>
public class FixAllInteractables
{
    public static void Execute()
    {
        Debug.Log("=== Starting Fix All Interactables ===");

        // Fix wine glasses
        FixWineGlasses();

        // Fix new bottles
        FixNewBottles();

        // Fix Cointreau
        FixCointreau();

        // Update LiquorDatabase
        UpdateLiquorDatabase();

        Debug.Log("=== Fix All Interactables Complete ===");
        EditorUtility.DisplayDialog("Success", "All interactables have been fixed!", "OK");
    }

    private static void FixWineGlasses()
    {
        Debug.Log("--- Fixing Wine Glasses ---");

        string[] glassNames = { "SM_WineGlass1", "SM_WineGlass2" };

        foreach (string glassName in glassNames)
        {
            GameObject glass = GameObject.Find(glassName);
            if (glass == null)
            {
                Debug.LogWarning($"Glass {glassName} not found!");
                continue;
            }

            // Make sure it's not static
            if (glass.isStatic)
            {
                glass.isStatic = false;
                Debug.Log($"Set {glassName} to non-static");
            }

            // Ensure Rigidbody exists
            Rigidbody rb = glass.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = glass.AddComponent<Rigidbody>();
                Debug.Log($"Added Rigidbody to {glassName}");
            }
            rb.mass = 0.2f;
            rb.useGravity = true;
            rb.isKinematic = false;

            // Ensure BoxCollider exists (for pickup)
            BoxCollider boxCollider = glass.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = glass.AddComponent<BoxCollider>();
                Debug.Log($"Added BoxCollider to {glassName}");
            }

            // Make sure MeshCollider is not convex (or remove it)
            MeshCollider meshCollider = glass.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.convex = false;
            }

            // Ensure InteractableItem exists
            InteractableItem interactable = glass.GetComponent<InteractableItem>();
            if (interactable == null)
            {
                interactable = glass.AddComponent<InteractableItem>();
                Debug.Log($"Added InteractableItem to {glassName}");
            }
            interactable.itemType = ItemType.Glass;
            interactable.itemName = glassName;
            interactable.liquidType = LiquidType.None;
            interactable.maxGlassCapacity = 300f;

            // Ensure GlassContainer exists
            GlassContainer glassContainer = glass.GetComponent<GlassContainer>();
            if (glassContainer == null)
            {
                glassContainer = glass.AddComponent<GlassContainer>();
                Debug.Log($"Added GlassContainer to {glassName}");
            }
            glassContainer.maxGlassVolume = 300f;

            Debug.Log($"Fixed {glassName}");
        }
    }

    private static void FixNewBottles()
    {
        Debug.Log("--- Fixing New Bottles ---");

        // Define bottle configurations: name, liquid type, display name
        var bottleConfigs = new[]
        {
            new { Name = "Rum_Elegance", LiquidType = "rum", DisplayName = "Rum" },
            new { Name = "Monin_Amaretto_Syrup", LiquidType = "amaretto_syrup", DisplayName = "Amaretto Syrup" },
            new { Name = "Martini_vermouth", LiquidType = "vermouth_sweet", DisplayName = "Sweet Vermouth" },
            new { Name = "Martin_VSOP", LiquidType = "cognac", DisplayName = "Cognac" },
            new { Name = "Lemon_Juice_Bottle", LiquidType = "lemon_juice", DisplayName = "Lemon Juice" },
            new { Name = "Absolut_Vodka_Bottle", LiquidType = "vodka", DisplayName = "Vodka" }
        };

        foreach (var config in bottleConfigs)
        {
            GameObject bottle = GameObject.Find(config.Name);
            if (bottle == null)
            {
                Debug.LogWarning($"Bottle {config.Name} not found!");
                continue;
            }

            // Ensure Rigidbody exists
            Rigidbody rb = bottle.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = bottle.AddComponent<Rigidbody>();
                Debug.Log($"Added Rigidbody to {config.Name}");
            }
            rb.mass = 0.5f;
            rb.useGravity = true;
            rb.isKinematic = false;

            // Ensure BoxCollider exists
            BoxCollider boxCollider = bottle.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = bottle.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(0.685f, 2.006f, 0.673f);
                Debug.Log($"Added BoxCollider to {config.Name}");
            }

            // Ensure InteractableItem exists
            InteractableItem interactable = bottle.GetComponent<InteractableItem>();
            if (interactable == null)
            {
                interactable = bottle.AddComponent<InteractableItem>();
                Debug.Log($"Added InteractableItem to {config.Name}");
            }
            interactable.itemType = ItemType.Bottle;
            interactable.itemName = config.DisplayName;
            
            // Convert liquid type string to enum
            LiquidType liquidType = ConvertStringToLiquidType(config.LiquidType);
            interactable.liquidType = liquidType;
            interactable.liquidAmount = 750f;
            interactable.maxLiquidAmount = 750f;

            // Ensure LiquidContainer exists
            LiquidContainer liquidContainer = bottle.GetComponent<LiquidContainer>();
            if (liquidContainer == null)
            {
                liquidContainer = bottle.AddComponent<LiquidContainer>();
                Debug.Log($"Added LiquidContainer to {config.Name}");
            }
            liquidContainer.liquidName = config.LiquidType;
            liquidContainer.isInfinite = true;
            liquidContainer.currentVolume = 750f;
            liquidContainer.maxVolume = 750f;
            liquidContainer.pourRate = 3.5f;

            Debug.Log($"Fixed {config.Name} with liquid type: {config.LiquidType}");
        }
    }

    private static void FixCointreau()
    {
        Debug.Log("--- Fixing Cointreau ---");

        GameObject cointreau = GameObject.Find("Cointreau");
        if (cointreau == null)
        {
            Debug.LogWarning("Cointreau not found!");
            return;
        }

        // Update InteractableItem
        InteractableItem interactable = cointreau.GetComponent<InteractableItem>();
        if (interactable != null)
        {
            interactable.liquidType = LiquidType.Juice; // Using Juice as closest match for liqueur
            Debug.Log("Updated Cointreau InteractableItem liquid type");
        }

        // Update LiquidContainer
        LiquidContainer liquidContainer = cointreau.GetComponent<LiquidContainer>();
        if (liquidContainer != null)
        {
            liquidContainer.liquidName = "cointreau";
            Debug.Log("Updated Cointreau LiquidContainer liquid name to 'cointreau'");
        }

        Debug.Log("Fixed Cointreau");
    }

    private static void UpdateLiquorDatabase()
    {
        Debug.Log("--- Updating LiquorDatabase ---");

        LiquorDatabase database = Resources.Load<LiquorDatabase>("LiquorDataBase");
        if (database == null)
        {
            Debug.LogError("LiquorDatabase not found at Resources/LiquorDataBase!");
            return;
        }

        // Check if we need to add new liquors
        bool needsUpdate = false;
        var liquorsToAdd = new System.Collections.Generic.List<LiquorData>();

        // Check for missing liquors
        string[] requiredLiquors = { "rum", "amaretto_syrup", "vermouth_sweet", "cognac", "lemon_juice", "vodka", "cointreau" };
        
        foreach (string liquorId in requiredLiquors)
        {
            if (database.GetLiquor(liquorId) == null)
            {
                needsUpdate = true;
                Debug.Log($"Missing liquor: {liquorId}");

                // Create the missing liquor data
                LiquorData newLiquor = CreateLiquorData(liquorId);
                if (newLiquor != null)
                {
                    liquorsToAdd.Add(newLiquor);
                }
            }
        }

        if (needsUpdate && liquorsToAdd.Count > 0)
        {
            // Add new liquors to the database
            var currentLiquors = new System.Collections.Generic.List<LiquorData>();
            if (database.liquors != null && database.liquors.Length > 0)
            {
                currentLiquors.AddRange(database.liquors);
            }
            currentLiquors.AddRange(liquorsToAdd);
            database.liquors = currentLiquors.ToArray();

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Debug.Log($"Added {liquorsToAdd.Count} new liquors to database");
        }
        else
        {
            Debug.Log("LiquorDatabase already contains all required liquors");
        }
    }

    private static LiquorData CreateLiquorData(string liquorId)
    {
        switch (liquorId)
        {
            case "rum":
                return LiquorData.Create("rum", "蘭姆酒", "Rum", 0xd4a574, 40f, LiquorCategory.BaseSpirit);
            
            case "amaretto_syrup":
                return LiquorData.Create("amaretto_syrup", "杏仁糖漿", "Amaretto Syrup", 0x8b4513, 0f, LiquorCategory.Mixer);
            
            case "vermouth_sweet":
                return LiquorData.Create("vermouth_sweet", "甜香艾酒", "Sweet Vermouth", 0x8b4513, 18f, LiquorCategory.FortifiedWine);
            
            case "cognac":
                return LiquorData.Create("cognac", "干邑白蘭地", "Cognac", 0x8b4513, 40f, LiquorCategory.BaseSpirit);
            
            case "lemon_juice":
                return LiquorData.Create("lemon_juice", "檸檬汁", "Lemon Juice", 0xfff44f, 0f, LiquorCategory.Mixer);
            
            case "vodka":
                return LiquorData.Create("vodka", "伏特加", "Vodka", 0xf0f0f0, 40f, LiquorCategory.BaseSpirit);
            
            case "cointreau":
                return LiquorData.Create("cointreau", "君度橙酒", "Cointreau", 0xffa500, 40f, LiquorCategory.Liqueur);
            
            default:
                Debug.LogWarning($"Unknown liquor ID: {liquorId}");
                return null;
        }
    }

    private static LiquidType ConvertStringToLiquidType(string liquidTypeString)
    {
        switch (liquidTypeString.ToLower())
        {
            case "vodka":
                return LiquidType.Vodka;
            case "gin":
                return LiquidType.Gin;
            case "rum":
                return LiquidType.Rum;
            case "whiskey":
                return LiquidType.Whiskey;
            case "tequila":
                return LiquidType.Tequila;
            case "brandy":
            case "cognac":
                return LiquidType.Wine; // Using Wine as closest match
            case "lemon_juice":
                return LiquidType.Juice;
            case "lime_juice":
                return LiquidType.Juice;
            case "simple_syrup":
            case "amaretto_syrup":
                return LiquidType.Soda; // Using Soda as closest match for syrup
            case "vermouth_dry":
            case "vermouth_sweet":
                return LiquidType.Wine; // Using Wine as closest match
            case "cointreau":
            case "triple_sec":
                return LiquidType.Juice; // Using Juice as closest match for liqueur
            default:
                Debug.LogWarning($"Unknown liquid type: {liquidTypeString}, defaulting to None");
                return LiquidType.None;
        }
    }
}
