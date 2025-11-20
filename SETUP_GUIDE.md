# Bar Simulator - Unity Setup Guide

A 3D cocktail simulation game ported from Three.js to Unity C#.

## Requirements

- Unity 2021.3 LTS or newer
- Input System package (com.unity.inputsystem)
- TextMeshPro package

## Quick Start

### 1. Import Project

Open Unity Hub and add the project folder. Unity will import all scripts automatically.

### 2. Install Required Packages

Open **Window > Package Manager** and install:
- **Input System** (if not installed)
- **TextMeshPro** (if not installed)

When prompted to enable the new Input System, select **Yes** and restart Unity.

---

## Setup Steps

### Step 1: Create ScriptableObject Databases

#### LiquorDatabase

1. Right-click in Project window: **Create > BarSimulator > Liquor Database**
2. Name it `LiquorDatabase`
3. Move to `Assets/Resources/` folder
4. The database includes 24 default liquors (gin, vodka, rum, etc.)

#### RecipeDatabase

1. Right-click: **Create > BarSimulator > Recipe Database**
2. Name it `RecipeDatabase`
3. Move to `Assets/Resources/`
4. Add recipes:

| Recipe | Ingredients |
|--------|-------------|
| Martini | gin (60ml), vermouth_dry (20ml) |
| Negroni | gin (30ml), campari (30ml), vermouth_sweet (30ml) |
| Margarita | tequila (45ml), triple_sec (20ml), lime_juice (15ml) |
| Daiquiri | rum (60ml), lime_juice (20ml), simple_syrup (10ml) |

#### NPCDatabase

1. Right-click: **Create > BarSimulator > NPC Database**
2. Name it `NPCDatabase`
3. Move to `Assets/Resources/`
4. Create NPC data entries with:
   - Name, Role, Gender
   - Position, Rotation
   - Dialogues array
   - Shirt color

---

### Step 2: Setup Input System

#### Create Input Actions

1. Navigate to `Assets/Input/`
2. If `PlayerInputActions.inputactions` exists, double-click to open
3. Otherwise create: **Create > Input Actions**

#### Configure Actions

**Movement Action Map:**

| Action | Type | Binding |
|--------|------|---------|
| Move | Value (Vector2) | WASD, Arrow Keys |
| Look | Value (Vector2) | Mouse Delta |
| Interact | Button | E |
| Drop | Button | R |
| Fire | Button | Left Mouse |
| AltFire | Button | Right Mouse |
| Pause | Button | Escape |
| Tab | Button | Tab |

4. Click **Generate C# Class** and save as `PlayerInputActions.cs`

---

### Step 3: Create Prefabs

#### Player Prefab

1. Create empty GameObject named `Player`
2. Add components:
   - `CharacterController` (Height: 1.8, Radius: 0.3, Center: 0, 0.9, 0)
   - `FirstPersonController` script
3. Create child `PlayerCamera`:
   - Add `Camera` component (FOV: 60)
   - Add `AudioListener`
   - Position: (0, 1.6, 0)
4. Drag to `Assets/Prefabs/`

#### Bottle Prefab

1. Create Cylinder primitive
2. Scale: (0.05, 0.15, 0.05)
3. Add `Bottle` script
4. Add `Rigidbody` (Is Kinematic: true)
5. Save as prefab

#### Glass Prefab

1. Create Cylinder primitive
2. Scale: (0.06, 0.08, 0.06)
3. Add `Glass` script
4. Apply transparent material
5. Create child for liquid visual
6. Save as prefab

#### Shaker Prefab

1. Create Capsule primitive
2. Scale: (0.08, 0.12, 0.08)
3. Add `Shaker` script
4. Apply metallic material
5. Save as prefab

#### NPC Prefab

1. Create empty GameObject
2. Add `NPCController` script
3. Create body (Capsule) and head (Sphere) as children
4. Add `CapsuleCollider`
5. Save as prefab

---

### Step 4: Scene Setup

#### Option A: Automatic Setup

1. Create empty scene
2. Add empty GameObject named `SceneSetup`
3. Add `SceneSetup` script
4. Enable:
   - Auto Create Systems: true
   - Auto Create Scene: true
5. Press Play - everything will be created automatically

#### Option B: Manual Setup

1. Create GameObjects for each manager:

```
Hierarchy:
├── GameManager (+ GameManager script)
├── CocktailSystem (+ CocktailSystem script)
├── InteractionSystem (+ InteractionSystem script)
├── NPCManager (+ NPCManager script)
├── UIManager (+ UIManager script)
├── AudioManager (+ AudioManager script)
├── LightingManager (+ LightingManager script)
├── EnvironmentManager (+ EnvironmentManager script)
├── Player (prefab instance)
├── Environment
│   ├── Floor
│   ├── BarCounter
│   └── Walls
└── UI (Canvas)
    ├── HUD
    ├── DialogueBox
    ├── RecipePanel
    └── PourProgressUI
```

---

### Step 5: UI Setup

#### Create Canvas

1. Create **UI > Canvas**
2. Set Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080

#### HUD

1. Create Panel as HUD container
2. Add `HUDController` script
3. Create children:
   - Crosshair (Image, centered)
   - InteractionHint (TextMeshProUGUI, bottom center)
   - MessageText (TextMeshProUGUI, top center)

#### DialogueBox

1. Create Panel
2. Add `DialogueBox` script
3. Add CanvasGroup component
4. Create children:
   - SpeakerName (TextMeshProUGUI)
   - DialogueText (TextMeshProUGUI)
   - ContinueHint (TextMeshProUGUI)

#### RecipePanel

1. Create Panel
2. Add `RecipePanel` script
3. Add CanvasGroup
4. Create ScrollView for recipe cards

#### PourProgressUI

1. Create Panel
2. Add `PourProgressUI` script
3. Add CanvasGroup
4. Create progress bars (Image with Fill)

---

### Step 6: Configure References

#### InteractionSystem

- Assign Player Camera
- Set Interaction Distance: 3
- Set Hold Distance: 0.5

#### CocktailSystem

- Assign LiquorDatabase
- Assign RecipeDatabase
- Set Pour Rate: 30

#### NPCManager

- Assign NPCDatabase
- Assign NPC Prefab
- Set Interaction Distance: 3

#### EnvironmentManager

- Assign LiquorDatabase
- Assign Bottle/Glass/Shaker Prefabs
- Set spawn transforms

#### UIManager

- Assign all UI components

---

## Controls

| Key | Action |
|-----|--------|
| WASD | Move |
| Mouse | Look around |
| E | Interact / Talk to NPC / Give drink |
| R | Drop held object |
| Left Click (Hold) | Pour / Shake |
| Right Click | Drink from glass |
| Tab | Toggle recipe panel |
| Escape | Pause menu |

---

## Gameplay Flow

1. **Pick up a glass** - Press E near a glass
2. **Pick up a bottle** - Press E near a bottle
3. **Pour into glass** - Hold left click while bottle is near glass
4. **Mix cocktails** - Combine ingredients in correct ratios
5. **Use shaker** - Pour ingredients, then hold left click to shake
6. **Serve to NPC** - Hold glass and press E near NPC
7. **Get feedback** - NPC evaluates your drink (0-100 score)

---

## Cocktail Recipes

| Cocktail | Ingredients | Ratio |
|----------|-------------|-------|
| Martini | Gin + Dry Vermouth | 2-3:1 |
| Vodka Martini | Vodka + Dry Vermouth | 2-3:1 |
| Negroni | Gin + Campari + Sweet Vermouth | 1:1:1 |
| Margarita | Tequila + Triple Sec + Lime Juice | 2:1:1 |
| Daiquiri | Rum + Lime Juice + Simple Syrup | 3:1:0.5 |
| Cosmopolitan | Vodka + Triple Sec + Cranberry + Lime | 2:1:1:0.5 |
| Whiskey Sour | Whiskey + Lemon Juice + Simple Syrup | 2:1:0.5 |

---

## Scoring System

Base score: 50 points

| Criteria | Points |
|----------|--------|
| Valid cocktail name | +30 |
| Optimal volume (100-250ml) | +10 |
| Optimal alcohol (15-35%) | +10 |
| Balanced ingredients (2-4 types) | +5 |
| Too little volume | -5 |
| Too much volume | -3 |
| Almost no alcohol | -10 |
| Too strong (>50%) | -5 |
| Single ingredient | -5 |

---

## Troubleshooting

### Input not working

1. Check **Edit > Project Settings > Player**
2. Set Active Input Handling to **Both** or **Input System Package**
3. Restart Unity

### Scripts not compiling

1. Check for missing namespaces
2. Ensure all required packages are installed
3. Check Console for specific errors

### UI not showing

1. Verify Canvas exists in scene
2. Check EventSystem exists
3. Verify CanvasGroup alpha is 1

### Objects not interacting

1. Check Collider components exist
2. Verify Layer settings
3. Check InteractionSystem references

---

## Project Structure

```
Assets/
├── Input/
│   └── PlayerInputActions.inputactions
├── Materials/
│   ├── LiquidShader.shader
│   └── GlassShader.shader
├── Prefabs/
│   ├── Player.prefab
│   ├── Bottle.prefab
│   ├── Glass.prefab
│   ├── Shaker.prefab
│   └── NPC.prefab
├── Resources/
│   ├── LiquorDatabase.asset
│   ├── RecipeDatabase.asset
│   └── NPCDatabase.asset
├── Scenes/
│   └── MainScene.unity
└── Scripts/
    ├── Core/
    ├── Data/
    ├── Interaction/
    ├── Managers/
    ├── NPC/
    ├── Objects/
    ├── Player/
    ├── Systems/
    └── UI/
```

---

## Building

1. **File > Build Settings**
2. Add your scene to build
3. Select target platform
4. Click **Build**

### Recommended Settings

- Resolution: 1920x1080
- Fullscreen Mode: Windowed
- Graphics API: Auto

---

## Credits

Ported from Three.js cocktail simulation game to Unity C#.
