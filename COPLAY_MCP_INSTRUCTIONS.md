# Bar Simulator Game - Development Instructions for Coplay MCP

## Project Overview
Unity-based first-person bartending simulation game. The player serves drinks to NPCs in a bar environment, earning money based on drink quality.

**Core Gameplay Loop:**
1. NPC walks into the bar
2. Player presses **E** to take NPC's order
3. Player makes cocktail and serves it
4. Player presses **F** to give drink to NPC
5. NPC evaluates drink → Shows score/money earned with visual effects
6. Repeat with more NPCs

## Current Status & Known Issues

### ✅ Implemented Features
- Basic first-person controller with WASD movement
- Camera pitch/yaw rotation (recently improved)
- Object interaction system (E to pick up, R to return)
- Item management (bottles, glasses, shaker, jigger)
- Input system using Unity's new Input System

### ❌ Current Issues to Fix

**CRITICAL - Camera Control:**
- Mouse sensitivity needs fine-tuning (currently 2.5f may still feel off)
- Camera rotation can be jittery during fast movements
- Need smoother interpolation for professional feel

**Scene Setup:**
- Bar environment incomplete (missing walls, ceiling, proper lighting)
- Objects not properly placed on surfaces (floating/clipping)
- Need complete bar layout with furniture

**Interaction System:**
- Needs visual feedback when hovering over interactable objects
- Missing audio feedback
- Object placement after drop needs improvement

**Missing Core Mechanics:**
- No pouring system
- No liquid visualization
- No NPC walking/ordering system
- No drink serving interaction (F key)
- No scoring/money system
- No visual effects for successful service

## Reference Implementation
Web version located in `src/` directory:
- `src/modules/PlayerController.js` - First-person controls
- `src/modules/InteractionSystem.js` - Object interaction
- `src/modules/CocktailSystem.js` - Pouring and cocktail system
- `src/modules/NPCManager.js` - NPC behavior (use as reference, but simplify)
- `src/modules/BarEnvironment.js` - Scene layout

## Available Resources

### Unity Scripts (Assets/Scripts/)
```
Assets/Scripts/
├── Core/
│   ├── Constants.cs (MouseSensitivity: 2.5f - recently updated)
│   ├── GameManager.cs
│   ├── GameBootstrapper.cs
│   └── GameSceneInitializer.cs
├── Player/
│   ├── FirstPersonController.cs (Recently fixed - improved camera)
│   └── PlayerInteraction.cs
├── Items/
│   ├── InteractableBase.cs
│   ├── Bottle.cs
│   ├── Glass.cs
│   ├── Shaker.cs
│   └── Jigger.cs
└── Managers/
    ├── BottleManager.cs
    ├── InteractionManager.cs
    └── ShaderManager.cs
```

### Available Prefabs (Assets/TheBar/Prefabs/)

**Essential Furniture:**
- `SM_BarchairBlackS.prefab` - Bar stools (place 4-6)
- `SM_SofaLeatherBlack.prefab` - Lounge seating
- `SM_CoffeeTableWoodBrownS.prefab` - Tables

**Bar Equipment:**
- `SM_CashDeskS.prefab` - Cash register
- `SM_Shelve2.prefab` - Horizontal shelves (for bottles)
- `SM_ShelveVert.prefab` - Vertical shelving
- `SM_Cupboard.prefab` - Storage cabinet

**Bottles & Glassware:**
- `SM_WhiskeyBottle1/2/3.prefab` - Whiskey bottles
- `SM_WineBottle1/2/3.prefab` - Wine bottles
- `SM_CoctailGlass.prefab` - Cocktail glass ⭐ (main serving glass)
- `SM_MartiniGlassS.prefab` - Martini glass
- `SM_WineGlass1/2/3.prefab` - Wine glasses
- `SM_BeerMugS.prefab` - Beer mug

**Structure:**
- `SM_WallBlock.prefab` - Wall segments
- `SM_FloorBlock.prefab` - Floor tiles
- `SM_Door.prefab` / `SM_DoorFrame.prefab` - Entrance

**Lighting:**
- `SM_FloorLight.prefab` - Floor lighting
- `SM_StandingLampS.prefab` - Standing lamps

**Decorations:**
- `SM_LogoPoster.prefab` - Wall art
- `SM_Speakers.prefab` - Audio speakers
- `SM_bush2.prefab` - Plants
- `SM_Decor1/2.prefab` - Misc decorations

## Development Phases

---

## PHASE 1: Fix Camera System (CRITICAL - START HERE) 🎯

**Goal:** Professional, smooth first-person camera controls

### Current Implementation Issues:
The `FirstPersonController.cs` has been recently updated but still needs refinement:
- Line 231: `yaw += lookInput.x * mouseSensitivity * Time.deltaTime;`
- Line 235: `pitch -= lookInput.y * mouseSensitivity * Time.deltaTime;`
- Current sensitivity: 2.5f (may need adjustment)
- Look smoothing: 10f (line 25)

### Problems to Fix:

1. **Input Processing**
   - Raw mouse delta from Input System can be jittery
   - Need better frame-rate independent smoothing
   - Consider adding sensitivity curve (slow = precise, fast = responsive)

2. **Smoothing Algorithm**
   - Current `Vector2.SmoothDamp` (lines 221) may be too aggressive
   - Test values between 5-20 for `lookSmoothing`
   - Add separate X/Y smoothing if needed

3. **Edge Cases**
   - Pitch clamping works but may feel "sticky" at limits
   - Add small dead zone near -90° and +90°

### Implementation Tasks:

**Task 1.1: Improve Input Filtering**
```csharp
// In HandleLook() method (around line 212)
// Add input scaling based on magnitude for better control

Vector2 targetLookInput = lookAction.ReadValue<Vector2>();

// Scale sensitivity based on input speed (optional enhancement)
float inputMagnitude = targetLookInput.magnitude;
float dynamicSensitivity = mouseSensitivity;

// Apply smoothing...
```

**Task 1.2: Fine-tune Sensitivity**
- Test different base values: 1.5f, 2.0f, 2.5f, 3.0f
- Ensure UI slider (GameUIController.cs line 505) maps correctly
- Target: Mouse movement feels 1:1 with expectations

**Task 1.3: Add Sensitivity Presets**
```csharp
// Add to Constants.cs
public const float MouseSensitivitySlow = 1.5f;
public const float MouseSensitivityMedium = 2.5f;
public const float MouseSensitivityFast = 4.0f;
```

### Testing Checklist:
- [ ] Camera feels smooth during slow panning
- [ ] Camera responds quickly to fast movements
- [ ] No jitter when continuously moving mouse
- [ ] Vertical look clamps smoothly at -90° and +90°
- [ ] Works consistently at different frame rates (30/60/144 fps)
- [ ] Settings slider provides meaningful control range

**Reference:** `src/modules/PlayerController.js` lines 64-76

---

## PHASE 2: Build Complete Bar Scene 🏗️

**Goal:** Fully furnished bar environment with proper layout

### Scene Dimensions
Based on web version (`src/modules/bar/BarStructure.js`):
- Main floor: 20m x 20m
- Ceiling height: 10m
- Bar counter: 12m long x 1m deep x 2m tall at z=-3
- Entrance door: Front wall (z=10)

### Building Instructions:

### Step 1: Floor & Walls
```
Floor:
- Use SM_FloorBlock.prefab to create 20x20m floor
- Grid layout: 10x10 blocks (each 2x2m)
- Position: Center at (0, 0, 0)

Walls:
- Use SM_WallBlock.prefab
- 4 walls around perimeter, height 10m
- Front wall (z=10): Add SM_Door + SM_DoorFrame at center

Ceiling:
- Optional: Use flipped SM_FloorBlock or ProBuilder plane
- Height: 10m
```

### Step 2: Bar Counter & Stools
```
Bar Counter (position: 0, 0, -3):
- Create custom counter or use SM_Tablel.prefab (scaled)
- Dimensions: 12m x 1m x 2m
- Counter top height: 1.0m

Bar Stools (front of counter):
- 4x SM_BarchairBlackS.prefab
- Positions:
  * (-4, 0, -1)
  * (-1.5, 0, -1)
  * (1.5, 0, -1)
  * (4, 0, -1)
- Rotation: Face counter (0, 180, 0)
```

### Step 3: Back Bar Shelving
```
Position: (0, 0, -8) - behind counter

Bottle Display:
- Use 3x SM_Shelve2.prefab stacked vertically
- Heights: 1.5m, 2.5m, 3.5m
- On each shelf:
  * 4x SM_WhiskeyBottle (various types)
  * 4x SM_WineBottle (various types)
- Total: 24 bottles

Shelf Lighting:
- Add Point Light under each shelf
- Color: Warm white (255, 240, 220)
- Intensity: 1.5
- Range: 6m
```

### Step 4: Working Area on Counter
```
Items placed on counter surface (y=1.0, z=-3):
- 3x SM_MartiniGlassS at x = -1, 0, 1 (for player use)
- 1x SM_CashDeskS at x = 5
- Scattered SM_RubberCoaster (4-6 pieces)

Tools (for player to pick up):
- These are already scripted, ensure they're placed in scene
- Reference existing item prefabs in scene
```

### Step 5: Lounge Area (Optional but Recommended)
```
Coffee table: (0, 0, 8)
- SM_CoffeeTableWoodBrownS.prefab

Seating:
- SM_SofaLeatherBlack at (0, 0, 10)
- 2x SM_ChairLeather at (-3, 0, 8) and (3, 0, 8)
```

### Step 6: Decorations
```
Wall Decorations:
- 2x SM_LogoPoster on back wall
- 1x SM_Speakers at (8, 2, -8)

Floor Decorations:
- 4x SM_bush2 in corners
- 2x SM_StandingLampS near seating area

Lighting Accents:
- 6x SM_FloorLight along bar counter base
```

### Step 7: Lighting Setup
```
Ambient Light:
- Window > Rendering > Lighting Settings
- Ambient Mode: Color
- Ambient Color: RGB(40, 30, 25) - warm brown

Main Lights:
- 8-10 Point Lights distributed across ceiling
- Color: Warm white (255, 250, 240)
- Intensity: 2.0
- Range: 12m
- Enable shadows (Soft Shadows)

Accent Lights:
- Under bar shelves (as described in Step 3)
- Near entrance door
- Above lounge area
```

### Important Notes:
- Use **Vertex Snap (V key)** to align objects precisely
- All bottles must have:
  - `Rigidbody` (kinematic when on shelf)
  - `Collider`
  - `InteractableBase` script
- Ensure player can walk freely (check NavMesh/collision)
- Test lighting - bar should feel warm and inviting

### Testing Checklist:
- [ ] Can walk around entire bar without getting stuck
- [ ] All bottles are reachable and pickable
- [ ] Lighting creates pleasant ambiance
- [ ] No objects floating or clipping through surfaces
- [ ] Scene feels cohesive and bar-like

**Reference:** `src/modules/BarEnvironment.js` and `src/modules/bar/BarStructure.js`

---

## PHASE 3: Implement Pouring System 🍾

**Goal:** Pour liquids from bottles into glasses with visual feedback

### System Overview:
- Hold bottle → Aim at glass → Hold Left Mouse Button → Liquid pours
- Visual: Particle stream + liquid level rises in glass
- Audio: Pouring sound effect

### Components to Create:

### 3.1: Create PouringSystem.cs

```csharp
using UnityEngine;

namespace BarSimulator.Cocktail
{
    /// <summary>
    /// Handles pouring liquid from bottles/containers into glasses
    /// Reference: src/modules/CocktailSystem.js lines 120-220
    /// </summary>
    public class PouringSystem : MonoBehaviour
    {
        [Header("Pour Settings")]
        [SerializeField] private float pourRate = 30f; // ml per second
        [SerializeField] private float maxPourDistance = 1.5f;
        [SerializeField] private float maxPourAngle = 30f; // degrees
        [SerializeField] private float bottleTiltAngle = 60f;

        [Header("References")]
        [SerializeField] private ParticleSystem pouringParticles;
        [SerializeField] private Transform raycastOrigin; // Camera

        private bool isPouring = false;
        private Bottle currentBottle;
        private ContainerController targetContainer;

        void Update()
        {
            if (Input.GetMouseButton(0)) // Left mouse held
            {
                TryPour();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                StopPouring();
            }
        }

        void TryPour()
        {
            // Check if holding a bottle
            // Raycast to find target glass
            // Verify distance and angle
            // If valid: Start pouring
            // Call AddLiquid on target container
            // Play particle effect
            // Tilt bottle
        }

        void StopPouring()
        {
            // Stop particles
            // Reset bottle rotation
            isPouring = false;
        }
    }
}
```

**Key Features:**
- Raycast from camera to detect target glass (LayerMask for containers)
- Distance check: `< 1.5m`
- Angle check: `Vector3.Angle(forward, toTarget) < 30°`
- Pour rate: 30ml per second (`Time.deltaTime * 30f`)
- Bottle tilt: Smoothly rotate bottle to 60° pitch while pouring

### 3.2: Create ContainerController.cs

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace BarSimulator.Cocktail
{
    /// <summary>
    /// Manages liquid contents in glasses and shakers
    /// </summary>
    public class ContainerController : MonoBehaviour
    {
        [Header("Container Settings")]
        [SerializeField] private float maxCapacity = 300f; // ml (glass)
        [SerializeField] private Transform liquidVisual; // Cylinder for liquid
        [SerializeField] private Renderer liquidRenderer;

        private List<LiquidIngredient> contents = new List<LiquidIngredient>();
        private float totalVolume = 0f;

        public bool IsFull => totalVolume >= maxCapacity;
        public float TotalVolume => totalVolume;
        public float Capacity => maxCapacity;

        /// <summary>
        /// Add liquid to container
        /// Returns actual amount added (may be less if near full)
        /// </summary>
        public float AddLiquid(string liquidName, float amount, Color color)
        {
            float spaceAvailable = maxCapacity - totalVolume;
            float amountToAdd = Mathf.Min(amount, spaceAvailable);

            if (amountToAdd <= 0) return 0;

            contents.Add(new LiquidIngredient {
                name = liquidName,
                volume = amountToAdd,
                color = color
            });

            totalVolume += amountToAdd;
            UpdateVisuals();

            return amountToAdd;
        }

        void UpdateVisuals()
        {
            // Update liquid height based on volume
            float fillPercentage = totalVolume / maxCapacity;
            liquidVisual.localScale = new Vector3(1, fillPercentage, 1);

            // Mix colors based on volume ratios
            Color mixedColor = CalculateMixedColor();
            liquidRenderer.material.color = mixedColor;
        }

        Color CalculateMixedColor()
        {
            // Weighted average of all liquid colors
            // Based on volume ratios
        }
    }

    [System.Serializable]
    public class LiquidIngredient
    {
        public string name;
        public float volume;
        public Color color;
    }
}
```

### 3.3: Create Liquid Visual GameObject

**For each glass:**
```
Glass (SM_MartiniGlassS)
└── LiquidVisual (GameObject)
    └── Cylinder mesh (scaled to fit inside glass)
        - Scale: (0.08, 0.01, 0.08) when empty
        - Position: Bottom of glass interior
        - Material: Transparent shader
          * Base Color: Dynamic (set by script)
          * Rendering Mode: Transparent
          * Alpha: 0.7
```

### 3.4: Create Pouring Particle System

**Prefab: "PouringParticles"**
```
Particle System Settings:
- Duration: Looping
- Start Lifetime: 0.3-0.5
- Start Speed: 2-3
- Start Size: 0.01-0.03
- Start Color: Dynamic (matches liquid)
- Gravity Modifier: 1.0
- Shape: Cone, Angle: 5°, Radius: 0.01
- Emission: Rate over Time = 200
- Collision: World collision, Type: Planes, Bounce: 0
- Renderer: Default material with transparency
```

**Attach to bottle prefabs:**
- Position: Bottle spout/opening
- Rotation: Pointing downward when bottle tilts
- Play when pouring starts, Stop when ends

### Testing Checklist:
- [ ] Can pick up bottle and pour into glass
- [ ] Liquid level rises visually in glass
- [ ] Pouring only works within 1.5m range
- [ ] Pouring stops when aiming away from glass
- [ ] Glass prevents overflow at max capacity (300ml)
- [ ] Particle stream looks realistic
- [ ] Bottle tilts smoothly during pour

**Reference:** `src/modules/CocktailSystem.js` lines 120-220

---

## PHASE 4: Create Liquid Database & Color System 🎨

**Goal:** Define 10-15 common liquids with colors for visual mixing

### 4.1: Create LiquidDatabase.cs

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace BarSimulator.Cocktail
{
    /// <summary>
    /// Database of all available liquids with properties
    /// Reference: src/modules/CocktailSystem.js
    /// </summary>
    public static class LiquidDatabase
    {
        private static Dictionary<string, LiquidData> liquids;

        static LiquidDatabase()
        {
            liquids = new Dictionary<string, LiquidData>
            {
                // Base Spirits
                { "Vodka", new LiquidData("Vodka", new Color(0.95f, 0.95f, 0.95f), 40f) },
                { "Gin", new LiquidData("Gin", new Color(0.9f, 0.95f, 0.95f), 40f) },
                { "Rum", new LiquidData("Rum", new Color(0.8f, 0.6f, 0.4f), 40f) },
                { "Whiskey", new LiquidData("Whiskey", new Color(0.7f, 0.5f, 0.3f), 40f) },
                { "Tequila", new LiquidData("Tequila", new Color(0.95f, 0.9f, 0.8f), 40f) },

                // Liqueurs
                { "Triple Sec", new LiquidData("Triple Sec", new Color(1f, 0.8f, 0.4f), 40f) },
                { "Dry Vermouth", new LiquidData("Dry Vermouth", new Color(0.9f, 0.9f, 0.7f), 18f) },
                { "Sweet Vermouth", new LiquidData("Sweet Vermouth", new Color(0.6f, 0.2f, 0.2f), 18f) },
                { "Campari", new LiquidData("Campari", new Color(0.8f, 0.1f, 0.1f), 25f) },

                // Juices & Mixers
                { "Lemon Juice", new LiquidData("Lemon Juice", new Color(1f, 1f, 0.6f), 0f) },
                { "Lime Juice", new LiquidData("Lime Juice", new Color(0.7f, 1f, 0.6f), 0f) },
                { "Orange Juice", new LiquidData("Orange Juice", new Color(1f, 0.6f, 0.2f), 0f) },
                { "Cranberry Juice", new LiquidData("Cranberry Juice", new Color(0.8f, 0.1f, 0.2f), 0f) },
                { "Simple Syrup", new LiquidData("Simple Syrup", new Color(0.95f, 0.95f, 0.95f), 0f) },
                { "Grenadine", new LiquidData("Grenadine", new Color(0.9f, 0.1f, 0.1f), 0f) },
            };
        }

        public static LiquidData GetLiquid(string name)
        {
            return liquids.ContainsKey(name) ? liquids[name] : null;
        }
    }

    [System.Serializable]
    public class LiquidData
    {
        public string name;
        public Color color;
        public float alcoholPercentage;

        public LiquidData(string name, Color color, float alcoholPercentage)
        {
            this.name = name;
            this.color = color;
            this.alcoholPercentage = alcoholPercentage;
        }
    }
}
```

### 4.2: Assign Liquids to Bottles

**Update Bottle.cs:**
```csharp
public class Bottle : InteractableBase
{
    [Header("Liquid Contents")]
    [SerializeField] private string liquidName = "Vodka"; // Set in inspector

    public string LiquidName => liquidName;
    public LiquidData LiquidData => LiquidDatabase.GetLiquid(liquidName);
}
```

**In Unity Inspector:**
- Assign liquid names to each bottle prefab in scene
- Example: WhiskeyBottle1 → "Whiskey", WineBottle1 → "Rum", etc.

### Testing Checklist:
- [ ] Each bottle has assigned liquid type
- [ ] Liquid colors display correctly in glasses
- [ ] Multiple liquids mix colors properly (weighted average)
- [ ] Database contains 10-15 different liquids

---

## PHASE 5: NPC System (Simplified) 👥

**Goal:** NPCs walk in, order drinks, receive drinks, show score/money

### Simplified NPC Features:
- ✅ NPC walks into bar through door
- ✅ Player presses **E** near NPC → NPC orders (shows dialogue bubble)
- ✅ Player makes drink and approaches NPC
- ✅ Player presses **F** with glass → Gives drink to NPC
- ✅ NPC evaluates drink → Shows money earned + visual effects
- ✅ NPC leaves bar
- ✅ Next NPC spawns

**NO complex dialogue, NO idle animations, NO preferences - keep it SIMPLE**

### 5.1: Create NPCController.cs

```csharp
using UnityEngine;
using TMPro;

namespace BarSimulator.NPC
{
    /// <summary>
    /// Simple NPC that walks in, orders, receives drink, pays, and leaves
    /// </summary>
    public class NPCController : MonoBehaviour
    {
        [Header("NPC Info")]
        [SerializeField] private string npcName = "Customer";
        [SerializeField] private float moveSpeed = 2f;

        [Header("Order Settings")]
        [SerializeField] private float interactionRange = 2.5f;

        [Header("UI")]
        [SerializeField] private Canvas nameTagCanvas;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject orderBubble; // "💭" icon

        private enum NPCState { Walking, Waiting, Ordered, Drinking, Leaving }
        private NPCState currentState = NPCState.Walking;

        private Transform targetPosition; // Bar counter position
        private bool hasOrdered = false;
        private bool hasDrink = false;

        void Start()
        {
            nameText.text = npcName;
            orderBubble.SetActive(false);

            // Find target position (in front of bar counter)
            targetPosition = GameObject.Find("NPCWaitPoint").transform;
            WalkToBar();
        }

        void Update()
        {
            switch (currentState)
            {
                case NPCState.Walking:
                    MoveToTarget();
                    break;

                case NPCState.Waiting:
                    // Idle, waiting for player to press E
                    orderBubble.SetActive(true); // Show order indicator
                    CheckForPlayerInteraction();
                    break;

                case NPCState.Ordered:
                    // Waiting for drink (F key)
                    CheckForDrinkGiven();
                    break;

                case NPCState.Drinking:
                    // Evaluate drink, show money
                    break;

                case NPCState.Leaving:
                    MoveToExit();
                    break;
            }
        }

        void WalkToBar()
        {
            currentState = NPCState.Walking;
        }

        void MoveToTarget()
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition.position) < 0.5f)
            {
                currentState = NPCState.Waiting;
            }
        }

        void CheckForPlayerInteraction()
        {
            // Check if player is nearby and presses E
            if (Input.GetKeyDown(KeyCode.E))
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                float distance = Vector3.Distance(transform.position, player.transform.position);

                if (distance <= interactionRange)
                {
                    TakeOrder();
                }
            }
        }

        void TakeOrder()
        {
            hasOrdered = true;
            currentState = NPCState.Ordered;
            orderBubble.SetActive(false);

            // Show simple order text
            Debug.Log($"{npcName} orders a drink!");
            // TODO: Show UI notification "Order taken!"
        }

        void CheckForDrinkGiven()
        {
            // Player presses F while holding glass near NPC
            if (Input.GetKeyDown(KeyCode.F))
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                float distance = Vector3.Distance(transform.position, player.transform.position);

                if (distance <= interactionRange)
                {
                    // TODO: Check if player is holding a glass with liquid
                    ReceiveDrink(null); // Pass glass container
                }
            }
        }

        public void ReceiveDrink(ContainerController glass)
        {
            currentState = NPCState.Drinking;
            hasDrink = true;

            // Evaluate drink
            float score = EvaluateDrink(glass);
            int moneyEarned = Mathf.RoundToInt(score * 10); // $10 per score point

            // Show visual effects
            ShowMoneyEffect(moneyEarned);
            ShowScoreEffect(score);

            // Add money to player
            GameManager.Instance.AddMoney(moneyEarned);

            // Leave after 2 seconds
            Invoke(nameof(StartLeaving), 2f);
        }

        float EvaluateDrink(ContainerController glass)
        {
            if (glass == null) return 1f; // Minimum score

            float score = 5f; // Base score

            // Simple evaluation
            float volume = glass.TotalVolume;

            // Volume check
            if (volume < 30f) score -= 2f; // Too little
            else if (volume >= 50f && volume <= 200f) score += 1f; // Good amount
            else if (volume > 250f) score -= 1f; // Too much

            // Ingredient variety (simple)
            int ingredientCount = glass.Contents.Count;
            if (ingredientCount >= 2) score += 1f;
            if (ingredientCount >= 3) score += 1f;

            // Random factor (±1)
            score += Random.Range(-1f, 1f);

            return Mathf.Clamp(score, 1f, 10f);
        }

        void ShowMoneyEffect(int amount)
        {
            // Create floating text: "+$50"
            // Green color, floats upward, fades out
            // TODO: Implement particle effect or UI animation
            Debug.Log($"Earned: ${amount}");
        }

        void ShowScoreEffect(float score)
        {
            // Show stars: ⭐⭐⭐⭐⭐ (based on score/2)
            // TODO: Implement UI popup
            Debug.Log($"Score: {score}/10");
        }

        void StartLeaving()
        {
            currentState = NPCState.Leaving;
            targetPosition = GameObject.Find("DoorExit").transform;
        }

        void MoveToExit()
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition.position) < 0.5f)
            {
                Destroy(gameObject); // Remove NPC
                NPCSpawner.Instance.SpawnNextNPC(); // Trigger next customer
            }
        }
    }
}
```

### 5.2: Create Simple NPC Character Model

**Use Unity Primitives (Quick & Simple):**
```
NPC (GameObject)
├── Body (Capsule)
│   - Height: 1.6m
│   - Radius: 0.3m
│   - Color: Random color per NPC
├── Head (Sphere)
│   - Radius: 0.15m
│   - Position: (0, 0.9, 0) relative to body
│   - Color: Skin tone
├── NameTag (World Space Canvas)
│   - Position: (0, 2.0, 0) - above head
│   - TextMeshPro: NPC name
│   - Font size: 1.5
│   - Always face camera (Billboard script)
└── OrderBubble (UI Image - World Space)
    - Icon: 💭 or "!" symbol
    - Position: (0, 1.8, 0)
    - Shows when NPC is ready to order
```

### 5.3: Create NPCSpawner.cs

```csharp
using UnityEngine;

namespace BarSimulator.NPC
{
    /// <summary>
    /// Spawns NPCs one at a time
    /// </summary>
    public class NPCSpawner : MonoBehaviour
    {
        public static NPCSpawner Instance;

        [SerializeField] private GameObject npcPrefab;
        [SerializeField] private Transform spawnPoint; // Door entrance
        [SerializeField] private float spawnDelay = 5f;

        private int npcsServed = 0;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            SpawnNextNPC();
        }

        public void SpawnNextNPC()
        {
            npcsServed++;

            // Spawn after delay
            Invoke(nameof(SpawnNPC), spawnDelay);
        }

        void SpawnNPC()
        {
            GameObject npc = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);
            npc.name = $"Customer_{npcsServed}";
        }
    }
}
```

### 5.4: Create MoneyDisplay UI

```
Canvas (Screen Space)
└── MoneyPanel (Top-right corner)
    └── MoneyText (TextMeshProUGUI)
        - Text: "$0"
        - Font size: 48
        - Color: Gold (#FFD700)
```

**Update GameManager.cs:**
```csharp
public class GameManager : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private TextMeshProUGUI moneyText;
    private int totalMoney = 0;

    public void AddMoney(int amount)
    {
        totalMoney += amount;
        moneyText.text = $"${totalMoney}";

        // Play cash register sound
        // TODO: AudioManager.PlaySound("CashRegister");
    }
}
```

### 5.5: Setup Scene for NPCs

**Add to scene:**
1. **NPCSpawnPoint** (Empty GameObject)
   - Position: (0, 0, 12) - outside door

2. **NPCWaitPoint** (Empty GameObject)
   - Position: (0, 0, 2) - in front of bar counter
   - NPC walks here and waits

3. **DoorExit** (Empty GameObject)
   - Position: (0, 0, 12) - same as spawn, NPC returns here to leave

### 5.6: Visual Effects for Money

**Create MoneyPopup Prefab:**
```
MoneyPopup (World Space Canvas)
└── TextMeshPro
    - Text: "+$50"
    - Color: Green (#00FF00)
    - Font size: 3
    - Animation:
      * Float upward (Y += 2 over 1 second)
      * Fade out (alpha 1 → 0)
      * Destroy after 1.5 seconds
```

**Script: MoneyPopupEffect.cs**
```csharp
using UnityEngine;
using TMPro;

public class MoneyPopupEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float lifetime = 1.5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        Color color = text.color;
        color.a -= Time.deltaTime / lifetime;
        text.color = color;
    }

    public void SetAmount(int amount)
    {
        text.text = $"+${amount}";
    }
}
```

**Instantiate in NPCController:**
```csharp
void ShowMoneyEffect(int amount)
{
    GameObject popup = Instantiate(moneyPopupPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
    popup.GetComponent<MoneyPopupEffect>().SetAmount(amount);
}
```

### Testing Checklist:
- [ ] NPC spawns at door entrance
- [ ] NPC walks to bar counter
- [ ] Press E near NPC → Shows order confirmation
- [ ] Make drink and press F near NPC → NPC receives drink
- [ ] Money popup appears with correct amount
- [ ] Score/rating shows (even if simple)
- [ ] NPC walks back to door and despawns
- [ ] Next NPC spawns after delay
- [ ] Money counter updates in UI

**Reference:** `src/modules/NPCManager.js` (but simplified - no complex dialogue/preferences)

---

## Quick Start Commands

### To fix camera (highest priority):
```
Fix the FirstPersonController.cs camera jitter issues.
Test different sensitivity values (1.5f - 3.0f) and smoothing parameters.
Ensure camera feels smooth at all mouse speeds.
Reference: COPLAY_MCP_INSTRUCTIONS.md Phase 1
```

### To build the scene:
```
Build the complete bar scene using TheBar prefabs.
Follow Phase 2 layout: floor, walls, counter, shelves, furniture, lighting.
Place 24 bottles on back bar shelves (Whiskey/Wine bottles).
Add proper lighting for warm ambiance.
Reference: COPLAY_MCP_INSTRUCTIONS.md Phase 2
```

### To implement pouring:
```
Implement the pouring system as described in Phase 3.
Create PouringSystem.cs, ContainerController.cs, and liquid visuals.
Add particle effects for pouring stream.
Test pouring from bottles into glasses.
Reference: COPLAY_MCP_INSTRUCTIONS.md Phase 3
```

### To add NPCs:
```
Create simple NPC system: walk in, order (E key), receive drink (F key), pay, leave.
Use primitive shapes for NPC model (capsule body + sphere head).
Show money earned with floating text effect.
Spawn NPCs one at a time.
Reference: COPLAY_MCP_INSTRUCTIONS.md Phase 5
```

---

## Success Criteria

Project is complete when:
- ✅ Camera controls are smooth and professional
- ✅ Bar scene is fully built with furniture and bottles
- ✅ Can pour liquids from bottles into glasses (with visuals)
- ✅ NPCs walk in, order, receive drinks, pay money, and leave
- ✅ Money system works with UI display
- ✅ Visual effects for money earned
- ✅ Game loop is functional (serve multiple NPCs)

**Keep it simple and functional - polish can come later!**
