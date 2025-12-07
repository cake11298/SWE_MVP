# Bar Simulator Game - Development Instructions for Coplay MCP

## Project Overview
This is a Unity-based first-person bartending simulation game, replicating and enhancing the web version located in `src/` directory. The goal is to create an immersive bar experience where players can interact with objects, make cocktails, serve NPCs, and manage a bar.

## Current Status & Known Issues

### ✅ Implemented Features
- Basic first-person controller with WASD movement
- Camera pitch/yaw rotation system
- Object interaction system (pick up/drop with E key, return with R key)
- Basic item management (bottles, glasses, shaker, jigger)
- Player interaction script
- Input system using Unity's new Input System

### ❌ Current Issues to Fix
1. **Camera Control Problems** (CRITICAL):
   - Mouse sensitivity still feels too sensitive despite recent fix (currently 2.5f)
   - Camera rotation has jittery/unstable behavior
   - Vertical look (pitch) sometimes glitches when looking up/down rapidly
   - Need smoother camera interpolation

2. **Scene Setup Problems**:
   - Bar environment is incomplete - missing walls, ceiling, proper lighting
   - Objects are not properly placed on surfaces (floating or clipping)
   - No proper bar counter, shelves, or furniture layout
   - Missing ambient atmosphere (lighting, decorations)

3. **Interaction System Issues**:
   - Pickup/hold offset needs adjustment for better visual feedback
   - Objects sometimes clip through surfaces when dropped
   - No visual feedback when hovering over interactable objects
   - Missing audio feedback for interactions

4. **Missing Game Mechanics**:
   - No pouring system implemented yet
   - No liquid visualization in containers
   - No cocktail recognition/recipe system
   - No NPC interaction or dialogue system
   - No scoring or progression system

## Reference Implementation
The complete web version is located in `src/` directory:
- `src/index.js` - Main game coordinator
- `src/modules/PlayerController.js` - First-person controls reference
- `src/modules/InteractionSystem.js` - Object interaction logic
- `src/modules/CocktailSystem.js` - Complete cocktail making system
- `src/modules/NPCManager.js` - NPC behavior and dialogue
- `src/modules/BarEnvironment.js` - Scene setup and layout

**Study these files carefully to understand the target gameplay experience.**

## Available Resources

### Unity Scripts (Assets/Scripts/)
```
Assets/Scripts/
├── Core/
│   ├── Constants.cs (Game constants - recently updated)
│   ├── GameManager.cs
│   ├── GameBootstrapper.cs
│   └── GameSceneInitializer.cs
├── Player/
│   ├── FirstPersonController.cs (Recently updated - has camera fixes)
│   └── PlayerInteraction.cs
├── Items/
│   ├── InteractableBase.cs
│   ├── Bottle.cs
│   ├── Glass.cs
│   ├── Shaker.cs
│   └── Jigger.cs
├── Managers/
│   ├── BottleManager.cs
│   ├── InteractionManager.cs
│   └── ShaderManager.cs
└── UI/
    ├── SettingsManager.cs
    └── GameUIController.cs
```

### Available Prefabs (Assets/TheBar/Prefabs/)
**Furniture & Seating:**
- `SM_BarchairBlackS.prefab` - Bar stools (use 4 of these)
- `SM_SofaLeatherBlack.prefab` - Lounge sofa
- `SM_ChairLeather.prefab` - Additional seating
- `SM_CoffeeTableWoodBrownS.prefab` - Coffee table for lounge
- `SM_Fotel2LeatherBlack.prefab` - Armchair

**Bar Equipment:**
- `SM_CashDeskS.prefab` - Cash register
- `SM_BeerTapS.prefab` - Beer tap
- `SM_EspressoMachineS.prefab` - Coffee machine
- `SM_Shelve2.prefab` - Horizontal shelves (for liquor display)
- `SM_ShelveVert.prefab` - Vertical shelving unit
- `SM_Cupboard.prefab` - Storage cabinet

**Bottles & Glassware:**
- `SM_WhiskeyBottle1/2/3.prefab` - Whiskey bottles
- `SM_WineBottle1/2/3.prefab` - Wine bottles
- `SM_CoctailGlass.prefab` - Cocktail glass
- `SM_MartiniGlassS.prefab` - Martini glass
- `SM_WineGlass1/2/3.prefab` - Wine glasses
- `SM_BeerMugS.prefab` - Beer mug
- `SM_CoffeeCup.prefab` - Coffee cup
- `SM_SampagneS.prefab` - Champagne bottle

**Structure & Architecture:**
- `SM_WallBlock.prefab` - Wall segments
- `SM_FloorBlock.prefab` - Floor tiles
- `SM_Door.prefab` / `SM_DoorFrame.prefab` - Door elements
- `SM_ColumnWide.prefab` - Decorative columns
- `SM_BuiltInBack/Seat/Side.prefab` - Built-in booth seating

**Decorations:**
- `SM_LogoPoster.prefab` - Wall posters
- `SM_Decor1/2.prefab` - Decorative objects
- `SM_FloorLight.prefab` - Floor lighting
- `SM_StandingLampS.prefab` - Standing lamps
- `SM_Speakers.prefab` - Audio speakers
- `SM_bush2.prefab` / `SM_Bamboo.prefab` - Plants

**Small Items:**
- `SM_AshtrayBlackS.prefab` - Ashtrays
- `SM_RubberCoaster.prefab` - Drink coasters
- `SM_BottleHolderMetal/Plastic.prefab` - Bottle holders

## Development Tasks - Prioritized

### PHASE 1: Fix Camera System (URGENT)
**Goal:** Make the first-person camera feel smooth, responsive, and professional

**Issues to Address:**
1. Camera sensitivity scaling - needs better input processing
   - Current issue: Mouse delta values are too raw, causing jitter
   - Solution: Implement proper delta time scaling and input smoothing
   - Reference: `src/modules/PlayerController.js` lines 64-76

2. Look smoothing improvements
   - Current `lookSmoothing` value (10f) may be too aggressive
   - Experiment with values between 5-15
   - Add separate horizontal/vertical smoothing options

3. Pitch clamping edge cases
   - Verify pitch doesn't "flip" at -90/+90 degree limits
   - Add dead zone near limits to prevent jitter
   - Ensure smooth transition near boundaries

4. Input buffering
   - Implement frame-independent mouse input accumulation
   - Add option for raw input vs. smoothed input
   - Test with different frame rates (30fps, 60fps, 144fps)

**Files to Modify:**
- `Assets/Scripts/Player/FirstPersonController.cs`
  - Improve `HandleLook()` method (lines 212-244)
  - Add sensitivity curve/acceleration options
  - Implement better input filtering

**Testing Checklist:**
- [ ] Camera feels smooth when moving mouse slowly
- [ ] Camera feels responsive when moving mouse quickly
- [ ] No jitter when looking around continuously
- [ ] Vertical look stops cleanly at -90° and +90°
- [ ] Camera doesn't "stick" or "jump" unexpectedly
- [ ] Settings slider (0.1-3.0 range) provides good control range

### PHASE 2: Build Complete Bar Scene
**Goal:** Create a visually complete and properly scaled bar environment using available prefabs

**Reference Layout from Web Version:**
```
Bar Dimensions (from src/modules/bar/BarStructure.js):
- Main floor: 20m x 20m
- Lounge extension: 20m x 10m (extends backward)
- Ceiling height: 10m
- Bar counter: 12m long x 1m deep x 2m tall, positioned at z=-3
```

**Scene Building Tasks:**

1. **Floor & Walls**
   ```
   Use: SM_FloorBlock.prefab
   - Create 20x20m main floor using grid of floor blocks
   - Create 20x10m lounge floor extension
   - Use: SM_WallBlock.prefab for 4 walls at 10m height
   - Add door using SM_Door + SM_DoorFrame
   ```

2. **Bar Counter Setup**
   ```
   Position: (0, 0, -3)
   Use: SM_TableBase + custom countertop
   - Main counter: 12m x 1m x 2m
   - Add bar mat area (4m x 2m) in center using material
   - Place 4x SM_BarchairBlackS in front at positions:
     * (-4, 0, -1)
     * (-1.5, 0, -1)
     * (1.5, 0, -1)
     * (4, 0, -1)
   ```

3. **Back Bar Shelving System**
   ```
   Position: (0, 0, -8) - behind bar counter
   Use: SM_Shelve2.prefab (3 tiers)
   - Tier 1: Height 1.5m - Place 8x whiskey/wine bottles
   - Tier 2: Height 2.7m - Place 8x whiskey/wine bottles
   - Tier 3: Height 3.9m - Place 8x whiskey/wine bottles

   Add LED lighting under each shelf:
   - Use emissive material strips
   - Warm white color (255, 240, 220)
   ```

4. **Ingredient Display Case**
   ```
   Position: (-8.5, 0, 0) - left wall
   Rotation: (0, 90, 0)
   Use: SM_Cupboard + SM_Shelve2
   - Create glass display with 3 shelves
   - Place 12x SM_WineBottle (representing ingredients)
   - Add LED lighting for each shelf
   ```

5. **Premium Bottle Display**
   ```
   Position: (9.5, 0, 0) - right wall
   Use: SM_Cupboard + SM_ShelveVert
   - Glass display case
   - Place 6x SM_SampagneS and SM_WhiskeyBottle3
   - LED backlighting
   ```

6. **Lounge Area Furniture**
   ```
   Coffee table: (0, 0, 11.5)
   - Use: SM_CoffeeTableWoodBrownS

   Sofa: (0, 0, 13)
   - Use: SM_SofaLeatherBlack

   Additional seating:
   - 2x SM_Fotel2LeatherBlack at (-3, 0, 11.5) and (3, 0, 11.5)
   ```

7. **Bar Counter Working Area**
   ```
   Place on counter (y = 1.0, z = -3):
   - 3x SM_MartiniGlassS at x = -1, 0, 1 (working glasses)
   - 1x SM_EspressoMachineS at x = 5 (decorative)
   - 1x SM_CashDeskS at x = -5
   - SM_RubberCoaster scattered around
   ```

8. **Decorations & Atmosphere**
   ```
   - SM_LogoPoster on walls (4 positions)
   - SM_Speakers at (8, 2, -8) and (-8, 2, -8)
   - SM_StandingLampS in corners (4 positions)
   - SM_FloorLight along bar counter (6 positions)
   - SM_bush2 or SM_Bamboo for plant decoration (4 positions)
   - SM_Decor1/2 on shelves and tables
   ```

**Important Placement Guidelines:**
- All objects MUST have proper Y positioning (not floating or clipping)
- Use Unity's snap tools (Vertex snap: V key) for precise placement
- Verify all bottles on shelves have:
  - Rigidbody component (kinematic when on shelf)
  - Box/Mesh Collider
  - InteractableBase script component
- Ensure player can walk around freely (no collision blocking)

**Lighting Setup:**
```
Ambient Lighting:
- Skybox: None (interior scene)
- Ambient Mode: Color
- Ambient Color: Warm brown (RGB: 40, 30, 25)

Point Lights:
- 15+ point lights distributed across scene
- Warm white temperature (2700K)
- Range: 8-12m
- Intensity: 1.5-2.5
- Shadows: Soft shadows

Shelf LED Lights:
- Use emissive materials (Emission color: white, Intensity: 2)
- Add thin rectangular quads under each shelf
- Standard shader with Emission enabled
```

### PHASE 3: Implement Pouring System
**Goal:** Create realistic liquid pouring mechanics matching web version

**Reference:** `src/modules/CocktailSystem.js` lines 120-220

**System Components:**

1. **Create PouringSystem.cs**
   ```csharp
   Features needed:
   - Detect when player holds bottle + aims at glass + holds left mouse
   - Raycast from bottle to glass (max distance: 1.5m, angle < 30°)
   - Pour rate: 30ml/second
   - Bottle tilt animation (rotate to 60° pitch)
   - Particle system for liquid stream (200 particles)
   - Play pouring sound effect
   ```

2. **Create LiquidRenderer.cs**
   ```csharp
   Features needed:
   - Dynamic liquid level visualization in glass/shaker
   - Color mixing system (blend RGB values by volume ratio)
   - Liquid height calculation based on volume
   - Shader-based liquid surface (use standard shader + alpha)
   ```

3. **Create ContainerController.cs**
   ```csharp
   Features needed:
   - Track liquid contents (List<LiquidIngredient>)
   - Max capacity: Glass = 300ml, Shaker = 500ml
   - Prevent overflow
   - Methods: AddLiquid(), RemoveLiquid(), GetTotalVolume()
   - Calculate alcohol percentage (weighted average)
   ```

4. **Create LiquidDatabase.cs**
   ```csharp
   Data structure needed:
   - Liquid name (string)
   - Color (Color)
   - Alcohol percentage (float)

   20+ Liquids to define:
   Base Spirits: Vodka (40%, clear), Gin (40%, clear), Rum (40%, amber),
                 Whiskey (40%, brown), Tequila (40%, clear), Brandy (40%, brown)
   Mixers: Lemon Juice (0%, yellow), Lime Juice (0%, green),
           Simple Syrup (0%, clear), Grenadine (0%, red)
   Juices: Orange (0%, orange), Pineapple (0%, yellow),
           Cranberry (0%, red), Tomato (0%, red)
   Special: Dry Vermouth (18%, pale), Sweet Vermouth (18%, red),
            Campari (25%, red), Triple Sec (40%, orange)
   ```

5. **Particle System Setup**
   ```
   Create particle system prefab: "PouringParticles"
   - Shape: Cone, angle: 5°
   - Start speed: 2-3 m/s
   - Start size: 0.01-0.03
   - Start color: Matches liquid color
   - Emission rate: 200 particles/sec
   - Gravity modifier: 1.0
   - Collision: World collision enabled
   - Renderer: Material with alpha transparency
   ```

**Testing Checklist:**
- [ ] Can pour from bottle into glass within 1.5m range
- [ ] Liquid level rises visually in glass
- [ ] Liquid color matches bottle contents
- [ ] Pouring stops when releasing mouse or moving away
- [ ] Bottle tilts smoothly during pour
- [ ] Particle stream looks realistic
- [ ] Glass prevents overflow at 300ml
- [ ] Can pour from glass to shaker (transfer)

### PHASE 4: Implement Cocktail Recognition System
**Goal:** Identify cocktails based on ingredient ratios

**Reference:** `src/modules/CocktailSystem.js` lines 400-550

1. **Create CocktailRecipes.cs**
   ```csharp
   Data structure:
   - Recipe name (string)
   - Ingredients with ratios (Dictionary<string, float>)
   - Tolerance (float - default 0.15 = 15% variance allowed)
   - Garnish (string - optional)
   - Method (string - "shaken", "stirred", "built")

   25+ Recipes to implement:
   Classic Cocktails:
   - Martini: Gin 5:1 Dry Vermouth
   - Negroni: Gin 1:1:1 Sweet Vermouth 1:1:1 Campari
   - Margarita: Tequila 3:2:1 Triple Sec 2:3:1 Lime Juice
   - Daiquiri: Rum 2:1:0.5 Lime Juice 1:2:0.5 Simple Syrup
   - Mojito: Rum 2:1:0.5:soda Lime 1:2:0.5:soda Simple Syrup
   - Cosmopolitan: Vodka 2:1:1:0.5 Triple Sec Cranberry Lime
   - Manhattan: Whiskey 2:1 Sweet Vermouth
   - Old Fashioned: Whiskey 2:0.25:dash Simple Syrup Bitters
   - Whiskey Sour: Whiskey 2:1:0.5 Lemon Juice Simple Syrup
   - Mai Tai: Rum 2:1:0.5:0.5 Orange Liqueur Lime Orgeat
   (Add 15 more from web version...)
   ```

2. **Create CocktailMatcher.cs**
   ```csharp
   Algorithm:
   1. Get all liquids in container
   2. Calculate ratios (normalize to total volume)
   3. For each recipe:
      a. Check if all required ingredients present
      b. Compare ratios within tolerance
      c. Calculate match score (0-100%)
   4. Return best match if score > 85%
   5. Mark as "perfect" (✨) if score > 95%
   ```

3. **UI Display Component**
   ```
   Create UI panel: "CocktailInfoPanel"
   - Cocktail name (large text)
   - Ingredient list with amounts (scrollable)
   - Total volume: X/300ml
   - Alcohol: X.X%
   - Perfect recipe indicator (✨ icon)
   - Position: Bottom-right corner
   - Auto-hide after 5 seconds
   ```

### PHASE 5: Create NPC System
**Goal:** 6 interactive NPCs with dialogue and drink evaluation

**Reference:** `src/modules/NPCManager.js`

1. **Create NPCController.cs**
   ```csharp
   Per-NPC Components:
   - Character model (Capsule + Sphere for now)
   - Floating name tag (World Space Canvas)
   - Idle animation (bobbing + swaying)
   - Dialogue array (10+ lines)
   - Drink preferences (favorite ingredients, alcohol tolerance)
   - Interaction range (2.5m)
   ```

2. **NPC Data (6 Characters):**
   ```
   1. Gustave (Founder)
      - Position: (2, 0, -5)
      - Preferences: Whiskey-based, classic cocktails
      - Dialogue: Professional, mentor-like
      - Easter egg: Special dialogues at 100/200/300 interactions

   2. Seaton (Co-founder)
      - Position: (-2, 0, -5)
      - Preferences: Gin-based, sophisticated
      - Dialogue: Friendly, encouraging

   3. 正安 (PR/VP, Female)
      - Position: (9, 0, 1)
      - Preferences: Sweet, colorful cocktails
      - Dialogue: Energetic, social

   4. 瑜柔 (Academic Director, Female)
      - Position: (9, 0, 3)
      - Preferences: Balanced, traditional recipes
      - Dialogue: Intellectual, detailed feedback

   5. 恩若 (Marketing, Female)
      - Position: (9, 0, -1)
      - Preferences: Trendy, Instagram-worthy drinks
      - Dialogue: Creative, modern

   6. 旻偉 (Equipment Manager, Male)
      - Position: (9, 0, 5)
      - Preferences: Strong, simple drinks
      - Dialogue: Technical, straightforward
   ```

3. **Create DrinkEvaluationSystem.cs**
   ```csharp
   Rating Algorithm (1-10 scale):
   Base score: 5

   Volume check:
   - < 30ml: -3 points (too little)
   - 30-200ml: 0 points (good)
   - > 200ml: -2 points (too much)

   Ingredient variety:
   - 1 ingredient: -1 point (boring)
   - 2 ingredients: 0 points
   - 3+ ingredients: +2 points (complex)

   Alcohol content:
   - 0%: -2 points (no alcohol)
   - 10-30%: +1 point (balanced)
   - > 40%: -1 point (too strong for most NPCs)

   Recipe match:
   - Perfect match (✨): +3 points
   - Good match: +2 points
   - No match: 0 points

   NPC preferences:
   - Contains favorite ingredient: +1 point
   - Matches preferred style: +1 point

   Final rating: Clamp(sum, 1, 10)
   ```

4. **Create DialogueSystem.cs**
   ```csharp
   Features:
   - Display NPC name + role
   - Cycle through dialogue array (E key)
   - 4-second display duration
   - Fade in/out animations
   - Different dialogue based on:
     * First interaction
     * Drink given (varies by rating)
     * Idle chat
     * Easter eggs
   ```

5. **Create RatingDisplayUI.cs**
   ```
   Popup UI Elements:
   - Star rating visualization (★★★★★☆☆☆☆☆)
   - Numeric score (7/10)
   - NPC feedback comment
   - 3-second popup duration
   - Position: Top-center of screen
   ```

**Character Models (Simplified for MVP):**
```
Use Unity primitives for now:
- Body: Capsule (height: 1.6m, radius: 0.3m)
- Head: Sphere (radius: 0.15m, offset: 0.9m up)
- Gender indication: Color coding
  * Male: Blue-tinted material
  * Female: Red-tinted material
- Name tag: TextMeshPro on World Canvas
  * Name (large font)
  * Role (smaller font)
  * Always face camera (LookAt script)
```

### PHASE 6: Shaking Mechanic
**Goal:** Implement shaker functionality for cocktails

**Reference:** `src/modules/CocktailSystem.js` lines 280-320

1. **Extend Shaker.cs**
   ```csharp
   Features:
   - Detect left mouse hold when holding shaker
   - Shake animation: Oscillating rotation
     * X-axis: ±30° at 4 Hz
     * Y-axis: ±15° at 5 Hz
   - Shake timer: Min 2 seconds for effect
   - "Mixed" state flag
   - Play shaking sound effect (rattle)
   - Particle effects (ice cubes bouncing)
   ```

2. **Mixing Enhancement System**
   ```csharp
   Benefits of shaking:
   - Improves flavor blend (visual: color becomes more uniform)
   - Required for certain cocktails (Margarita, Daiquiri, etc.)
   - Bonus points in evaluation if recipe requires shaking
   ```

### PHASE 7: UI & HUD System
**Goal:** Complete in-game user interface

**Reference:** `src/index.html` and CSS files

1. **Main Menu UI** (Canvas: "MainMenuCanvas")
   ```
   Panels needed:
   - Title screen with logo
   - Buttons: Play, Tutorial, Settings, Credits, Quit
   - Tutorial panel (scrollable text, controls diagram)
   - Settings panel (sliders for volume + sensitivity)
   - Credits panel (team names, roles)
   ```

2. **Game HUD** (Canvas: "GameHUDCanvas")
   ```
   Top panel:
   - Score counter (large text)
   - Satisfied drinks: X/5 (progress bar)
   - Time played (optional)

   Center:
   - Crosshair (always visible, + shape)
   - Interaction hint (dynamic text below crosshair)
     * "Press E to pick up [Object Name]"
     * "Press R to return to shelf"
     * "Hold LMB to pour"
     * etc.

   Bottom-right:
   - Pour progress panel
     * Container volume bar (0-300ml)
     * Current pour bar
     * ML indicators
     * Auto-hide after 5 seconds

   Bottom-left:
   - Container info panel
     * Cocktail name
     * Ingredient list
     * Total volume
     * Alcohol %
   ```

3. **Game Flow UI**
   ```
   Victory Screen:
   - Triggered when 5 drinks rated ≥ 7
   - Display: Total score, time, accuracy
   - Buttons: Play Again, Main Menu

   Pause Menu (ESC key):
   - Semi-transparent overlay
   - Buttons: Resume, Settings, Restart, Main Menu
   - Unlock cursor
   ```

### PHASE 8: Audio System
**Goal:** Add sound effects and background music

1. **Create AudioManager.cs**
   ```csharp
   Sound effects needed:
   - Pickup object (soft click)
   - Drop object (thud, varies by surface)
   - Pour liquid (running water)
   - Shake shaker (ice rattling)
   - Success/Fail evaluation (ding/buzz)
   - UI button clicks

   Music system:
   - Background ambient music (loopable)
   - Optional: 8-bit style procedural music (like web version)

   Features:
   - Volume control (master, SFX, music separate)
   - 3D spatial audio for pouring/shaking
   - Pitch variation for repeated sounds
   ```

2. **Audio Sources Setup**
   ```
   - Global AudioManager object (singleton)
   - Attach AudioSource to each interactable (for pickup/drop)
   - Attach AudioSource to player (for pouring while holding bottle)
   - UI AudioSource for menu sounds
   ```

### PHASE 9: Polish & Optimization

1. **Visual Polish**
   - Post-processing stack (ambient occlusion, bloom, color grading)
   - Improved materials with normal maps
   - Better lighting (baked lightmaps for static objects)
   - Shadow quality tuning

2. **Physics Optimization**
   - Set bottles to kinematic when on shelves
   - Use layer-based collision (ignore player-player, etc.)
   - Reduce physics update rate for distant objects

3. **Input Improvements**
   - Add key rebinding system
   - Support for gamepad (optional)
   - Accessibility options (toggle hold vs. press)

4. **Bug Fixes & Edge Cases**
   - Handle edge cases (pouring into full glass)
   - Prevent objects from falling through floor
   - Fix any remaining camera jitter
   - Ensure all NPCs are reachable without collision issues

## Technical Requirements

### Unity Version
- Unity 2021.3 LTS or newer
- URP (Universal Render Pipeline) recommended

### Required Packages
- Unity Input System (already installed)
- TextMeshPro (for UI text)
- Post Processing (for visual effects)
- ProBuilder (optional, for level design)

### Performance Targets
- 60 FPS on mid-range PC (GTX 1060 equivalent)
- < 2 second load time for main scene
- < 100 draw calls in main view
- < 1000 active GameObjects at once

### Code Quality Standards
- All public methods must have XML documentation comments
- Use constants from `Constants.cs` instead of magic numbers
- Follow Unity naming conventions (PascalCase for public, camelCase for private)
- Implement proper object pooling for particles
- Use events/delegates for system communication (avoid tight coupling)

## Testing Procedures

### Functionality Testing
After each phase, verify:
1. No console errors or warnings
2. All interactive objects work as expected
3. UI displays correct information
4. Performance is acceptable (check FPS)
5. Scene looks visually coherent

### Playtest Checklist
- [ ] Can walk around entire bar without getting stuck
- [ ] Can pick up and return all bottles/glasses
- [ ] Can make at least 5 different cocktails
- [ ] NPCs respond correctly to drinks
- [ ] Can win game by satisfying 5 NPCs
- [ ] Camera feels smooth and responsive
- [ ] UI is readable and informative
- [ ] Audio feedback is appropriate and not annoying

## Expected Deliverables

### Phase-by-Phase Deliverables
1. **Phase 1**: Smooth camera control system
2. **Phase 2**: Complete bar scene with all furniture/decorations
3. **Phase 3**: Working pouring system with visual feedback
4. **Phase 4**: Cocktail recognition for 25+ recipes
5. **Phase 5**: 6 NPCs with dialogue and evaluation
6. **Phase 6**: Functional shaker with animations
7. **Phase 7**: Complete UI/HUD system
8. **Phase 8**: Audio feedback for all interactions
9. **Phase 9**: Polished, optimized final product

### Final Product
A fully playable bartending simulation game matching or exceeding the web version's features, with:
- Professional first-person controls
- Realistic cocktail making mechanics
- Engaging NPC interactions
- Complete game loop (start → serve drinks → win)
- Polished visuals and audio

## Common Pitfalls to Avoid

1. **Don't** create new prefabs for every bottle - use prefab variants
2. **Don't** use `FindObjectOfType` in Update loops - cache references
3. **Don't** forget to set Layer masks for raycasts (ignore player layer)
4. **Don't** use hard-coded positions - use Transform references
5. **Don't** implement features without referencing web version first
6. **Don't** skip testing after each change - iterate frequently

## Questions to Consider

Before implementing each phase, ask:
- Does this match the web version's behavior?
- Is this performant (no FPS drops)?
- Is this user-friendly (clear feedback)?
- Does this integrate well with existing systems?
- Have I tested edge cases?

## Success Criteria

The project is successful when:
- ✅ Camera controls feel professional and smooth
- ✅ All 70+ prefabs are properly placed in scene
- ✅ Can make and serve cocktails to NPCs
- ✅ Game loop is complete and fun
- ✅ Visuals match or exceed web version quality
- ✅ Code is clean, documented, and maintainable
- ✅ Zero critical bugs or performance issues

---

## Quick Start Commands for Coplay MCP

Use these prompts to begin development:

### To Fix Camera (Start Here):
```
Review the FirstPersonController.cs camera system and fix all jitter/sensitivity issues.
Reference the web version's PlayerController.js for smooth input handling.
Ensure camera feels professional at all mouse speeds and frame rates.
Test thoroughly and iterate until perfect.
```

### To Build Scene:
```
Build the complete bar scene using prefabs from Assets/TheBar/Prefabs/.
Follow the layout in COPLAY_MCP_INSTRUCTIONS.md Phase 2.
Position all objects correctly (no floating or clipping).
Add proper lighting with warm ambiance.
Reference src/modules/BarEnvironment.js for exact dimensions and placement.
```

### To Implement Pouring:
```
Implement the complete pouring system as described in Phase 3.
Create PouringSystem.cs, LiquidRenderer.cs, and ContainerController.cs.
Reference src/modules/CocktailSystem.js lines 120-220.
Include particle effects and liquid visualization.
Test thoroughly with multiple bottles and glasses.
```

---

**Good luck! Follow these instructions carefully and reference the web version frequently.**
