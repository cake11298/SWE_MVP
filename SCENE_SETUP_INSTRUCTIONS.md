# 🎮 Scene Setup Instructions - Integration with Existing Assets

This guide will help you integrate the new refactored systems with your existing **GameScene.unity** and purchased bar assets.

---

## 📋 Prerequisites

You should have:
- ✅ Existing `GameScene.unity` with purchased bar assets ("The Bar")
- ✅ Existing player controller or FPS controller in the scene
- ✅ Phase 1, 2, & 3 refactoring complete (EventBus, FSM, Data layers)

---

## 🔧 Step 1: Create Required Layers

Unity layers are required for the interaction system to work properly.

1. Go to **Edit → Project Settings → Tags and Layers**
2. Create these layers:

| Layer Number | Layer Name    | Purpose                          |
|--------------|---------------|----------------------------------|
| 8            | Interactable  | For bottles, glasses, shakers    |
| 9            | NPC           | For customer NPCs                |
| 10           | Floor         | For floor objects (optional)     |

**Screenshots:**
```
[User Layer 8]: Interactable
[User Layer 9]: NPC
[User Layer 10]: Floor
```

---

## 🏷️ Step 2: Tag Scene Objects

Open your `GameScene.unity` and add tags to existing objects:

### **A. Create Tags (if missing)**

Go to **Edit → Project Settings → Tags and Layers → Tags**:
- `Bar`
- `LiquorShelf`
- `OrderPoint`
- `Bottle`
- `Glass`
- `Floor`
- `Player` (should already exist)

### **B. Tag Your Scene Objects**

Find these objects in your Hierarchy and assign tags:

| Object Name (Find in Hierarchy)     | Tag to Assign  | Layer          |
|--------------------------------------|----------------|----------------|
| Bar counter/table object             | `Bar`          | Default        |
| Shelf with bottles                   | `LiquorShelf`  | Default        |
| Floor object                         | `Floor`        | Floor          |
| Any bottle objects                   | `Bottle`       | Interactable   |
| Any glass objects                    | `Glass`        | Interactable   |
| Your player/FPS controller           | `Player`       | Default        |

**Tips:**
- If you can't find "Bar counter", look for objects named: `Counter`, `BarTable`, `Desk`, etc.
- The `SceneIntegrator` will try to find these automatically, but tagging helps!

---

## 🎯 Step 3: Create OrderPoint GameObject

This is where customers will spawn/stand when ordering.

1. In Hierarchy, **Right-click → Create Empty**
2. Name it: `OrderPoint`
3. Tag it: `OrderPoint`
4. Position it in front of the bar (where customers would stand)
   - Example: `X: 0, Y: 0, Z: 2` (2 meters in front of bar)

**Visual Check:**
- The `SceneIntegrator` will draw a **yellow wireframe sphere** in the Scene view at this location

---

## 🚀 Step 4: Setup Bootstrapper GameObject

1. In Hierarchy, **Right-click → Create Empty**
2. Name it: `GameBootstrapper`
3. In Inspector, **Add Component → Bootstrapper** (`BarSimulator.Core.Bootstrapper`)

### **Inspector Settings:**

#### **Scene Integration:**
- ✅ **Integrate Existing Scene:** `true`
- **Scene Integrator:** Leave empty (auto-creates)

#### **Player Setup:**
- ✅ **Spawn Player If Missing:** `false` (you have existing player)
- **Player Prefab:** Leave empty

#### **Debug:**
- ✅ **Verbose Logging:** `true` (enable for setup, disable later)

**Screenshot:**
```
[Bootstrapper Component]
✓ Integrate Existing Scene: true
  Scene Integrator: (empty)
✓ Spawn Player If Missing: false
✓ Verbose Logging: true
```

---

## 🎮 Step 5: Setup Player Interaction

Your existing player needs the interaction system.

### **Option A: If You Have Existing FPS Controller**

1. Find your Player GameObject in Hierarchy
2. Find the **Camera** child object (usually named "Main Camera" or "PlayerCamera")
3. Select the Camera
4. In Inspector, **Add Component → Player Interaction** (`BarSimulator.Player.PlayerInteraction`)

### **Inspector Settings:**

- **Interaction Distance:** `3.0`
- **Interactable Layer:** Select `Interactable` layer
- **Hand Position:** Leave empty (auto-creates)
- **Pickup Key:** `E`
- **Drop Key:** `Q`
- ✅ **Use Mouse Click:** `true`
- ✅ **Show Debug Ray:** `true`

### **Option B: If You DON'T Have a Player**

1. In **Project → Assets/Scripts/Player**
2. Find `FirstPersonController.cs`
3. Drag it onto an empty GameObject in Hierarchy
4. Name it: `Player`
5. Tag it: `Player`
6. Add `PlayerInteraction` to the Camera (as above)

---

## 📦 Step 6: Make Objects Interactable

For each **Bottle** and **Glass** in your scene:

### **Manual Method (Recommended for Testing):**

1. Select a bottle object in Hierarchy
2. **Add Component → Interactable Object** (`BarSimulator.Objects.InteractableObject`)
3. **Add Component → Rigidbody** (if missing)
   - Mass: `0.5`
   - Drag: `1`
   - Use Gravity: ✅
4. **Add Component → Box Collider** (if missing)
5. Set **Layer** to `Interactable`
6. Set **Tag** to `Bottle`

**Repeat for glasses** (use `Glass` tag).

### **Automatic Method (Advanced):**

The `SceneIntegrator` will attempt to:
- Find objects with "Bottle" or "Glass" in their name
- Automatically add colliders and Rigidbodies
- Set layers and tags

**To use automatic method:**
- Name your objects: `Vodka_Bottle`, `Wine_Glass`, etc.
- Or tag them first before hitting Play

---

## ▶️ Step 7: Test the Scene

1. **Save your scene:** `Ctrl+S` / `Cmd+S`
2. **Press Play** ▶️

### **Expected Console Output:**

```
[Bootstrapper] Initializing for existing GameScene...
[SceneIntegrator] Scanning existing scene...
[SceneIntegrator] ✓ Found Bar Counter: BarTable
[SceneIntegrator] ✓ Found Liquor Shelf: Shelf_01
[SceneIntegrator] ✓ Found 5 existing bottles
[SceneIntegrator] ✓ Found 3 existing glasses
[SceneIntegrator] ✅ Scene integration complete!
[Bootstrapper] ✓ EventBus ready
[Bootstrapper] ✓ RecipeDatabase ready (10 recipes)
[Bootstrapper] ✓ Found existing player: FPSController
[Bootstrapper] ✓ Added PlayerInteraction to existing player camera
[Bootstrapper] ✅ All systems initialized successfully!
[Bootstrapper] Scene is ready to play!
```

### **Test Interaction:**

1. **Walk around** using WASD (your existing controls)
2. **Look at a bottle** - you should see crosshair and text: "Look at: Vodka_Bottle"
3. **Left Click / Press E** - bottle should attach to your hand
4. **Look at the bar counter** - text: "Left Click / E: Place"
5. **Left Click / Press E** - bottle should place on the counter
6. **Press Q** - bottle should drop

### **Troubleshooting:**

| Issue                               | Solution                                          |
|-------------------------------------|---------------------------------------------------|
| "Interactable layer not found"      | Go back to Step 1, create the layer               |
| Can't pick up objects               | Check object has `Interactable` layer             |
| Objects fall through floor          | Tag floor as `Floor`, ensure it has collider      |
| No crosshair visible                | Ensure `PlayerInteraction` has `Show Debug Ray`   |
| Player not found                    | Tag your player GameObject as `Player`            |

---

## 🎨 Optional: Visual Feedback

### **Highlight Objects on Hover**

In `InteractableObject` component:
- ✅ **Highlight On Hover:** `true`
- **Highlight Color:** Yellow

### **Customize Crosshair**

Edit `PlayerInteraction.cs` → `OnGUI()` method for custom crosshair graphics.

---

## 🔄 Step 8: Enable FSM (Optional)

The FSM (customer system) is currently **disabled** for testing interaction.

To enable:
1. Open `Assets/Scripts/Core/Bootstrapper.cs`
2. Find line ~69: `// InitializeStateMachine();`
3. Uncomment: `InitializeStateMachine();`
4. Save and Play

**This will:**
- Spawn a customer automatically
- Start the game loop (Idle → CustomerEntry → Crafting → etc.)

---

## 📁 File Checklist

Ensure these files exist:

- ✅ `Assets/Scripts/Core/SceneIntegrator.cs`
- ✅ `Assets/Scripts/Player/PlayerInteraction.cs`
- ✅ `Assets/Scripts/Objects/InteractableObject.cs`
- ✅ `Assets/Scripts/Core/Bootstrapper.cs` (updated)

---

## 🐛 Common Issues

### **"MissingReferenceException: The object of type 'GameObject' has been destroyed"**

**Cause:** Old systems trying to access destroyed objects.

**Fix:**
1. Delete old `NPCManager`, `CocktailSystem` GameObjects in Hierarchy
2. Only keep `GameBootstrapper`

### **"Layer 'Interactable' does not exist"**

**Fix:** Create the layer in Project Settings (Step 1).

### **Objects don't have physics**

**Fix:** Ensure each interactable has:
- ✅ Rigidbody component
- ✅ Collider component (Box or Mesh)

### **Player can't move**

**Fix:** Ensure your existing FPS controller is still active. The new system only adds interaction, not movement.

---

## ✅ Success Criteria

You know the setup is correct when:

1. ✅ Console shows no errors on Play
2. ✅ You can walk around the bar
3. ✅ Crosshair shows when looking at objects
4. ✅ You can pick up bottles/glasses with E or Left Click
5. ✅ You can place objects on the counter
6. ✅ Objects have physics (fall, collide)

---

## 🎯 Next Steps

Once interaction works:

1. **Enable FSM** (uncomment `InitializeStateMachine()`)
2. **Test customer spawning** (State_CustomerEntry)
3. **Test QTE minigame** (shake a cocktail)
4. **Connect UI** (patience bars, scores)

---

## 🆘 Need Help?

**Check Console Logs:**
- Bootstrapper outputs detailed setup info
- Look for lines starting with `[Bootstrapper]` or `[SceneIntegrator]`

**Enable Debug Mode:**
- `PlayerInteraction`: Enable `Show Debug Ray`
- `Bootstrapper`: Enable `Verbose Logging`
- `SceneIntegrator`: Enable `Verbose Logging`

**Scene View Gizmos:**
- Green cube = Bar Counter position
- Yellow sphere = Order Point position
- Red ray = Looking at non-interactable
- Green ray = Looking at interactable

---

**Good luck! Your bar simulation is ready to serve drinks! 🍸**
