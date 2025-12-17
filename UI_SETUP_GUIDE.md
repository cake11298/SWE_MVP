# Simple UI System - Implementation Guide

This guide provides step-by-step instructions for setting up a clean, functional UI system with Main Menu and Pause Menu.

---

## Prerequisites

- **Scripts Created:**
  - `Assets/Scripts/UI/SimpleMainMenu.cs`
  - `Assets/Scripts/UI/SimplePauseMenu.cs`

- **Scenes Required:**
  - `MainMenu` (to be created)
  - `TheBar` (already exists)

---

## Part 1: Create Main Menu Scene

### Step 1: Create New Scene
1. In Unity, go to **File > New Scene**
2. Choose **Basic (Built-in)** or **Basic (URP)** template
3. Save the scene as `MainMenu` in `Assets/Scenes/` folder

### Step 2: Add Main Menu Scene to Build Settings
1. Go to **File > Build Settings**
2. Click **Add Open Scenes** to add MainMenu
3. Ensure **MainMenu** is at index 0 (drag to reorder if needed)
4. Ensure **TheBar** is at index 1

### Step 3: Create Main Menu UI

#### A. Create Canvas
1. Right-click in Hierarchy > **UI > Canvas**
2. Rename to `MainMenu_Canvas`
3. Set Canvas properties:
   - **Render Mode:** Screen Space - Overlay
   - **Canvas Scaler > UI Scale Mode:** Scale With Screen Size
   - **Reference Resolution:** 1920 x 1080

#### B. Create Background Panel
1. Right-click `MainMenu_Canvas` > **UI > Panel**
2. Rename to `Background`
3. Set properties:
   - **Anchor:** Stretch both (Alt+Shift+Click on anchor preset)
   - **Left/Right/Top/Bottom:** 0
   - **Image > Color:** Dark Gray `RGB(40, 40, 40)` or `#282828`

#### C. Create Title Text
1. Right-click `MainMenu_Canvas` > **UI > Text - TextMeshPro**
   - If prompted, import TMP Essentials
2. Rename to `TitleText`
3. Set properties:
   - **Rect Transform:**
     - Anchor: Top Center
     - Pos X: 0, Pos Y: -150
     - Width: 800, Height: 150
   - **TextMeshPro:**
     - Text: "THE BAR"
     - Font Size: 72
     - Alignment: Center & Middle
     - Color: White `RGB(255, 255, 255)`

#### D. Create Button Container
1. Right-click `MainMenu_Canvas` > **UI > Empty** (Create Empty GameObject)
2. Rename to `ButtonContainer`
3. Add Component: **Vertical Layout Group**
   - **Spacing:** 20
   - **Child Alignment:** Middle Center
   - **Child Force Expand:** Width ✓, Height ✗
4. Add Component: **Content Size Fitter**
   - **Vertical Fit:** Preferred Size
5. Set Rect Transform:
   - Anchor: Middle Center
   - Pos X: 0, Pos Y: -50
   - Width: 300, Height: 200

#### E. Create Start Game Button
1. Right-click `ButtonContainer` > **UI > Button - TextMeshPro**
2. Rename to `StartButton`
3. Set Button properties:
   - **Image > Color:** Light Gray `RGB(200, 200, 200)` or `#C8C8C8`
   - **Transition:** Color Tint
   - **Highlighted Color:** White `RGB(255, 255, 255)`
   - **Pressed Color:** Gray `RGB(150, 150, 150)`
4. Set Rect Transform:
   - **Height:** 60
5. Select child `Text (TMP)`:
   - **Text:** "Start Game"
   - **Font Size:** 24
   - **Color:** Black `RGB(0, 0, 0)`
   - **Alignment:** Center & Middle

#### F. Create Quit Button
1. Duplicate `StartButton` (Ctrl+D)
2. Rename to `QuitButton`
3. Select child `Text (TMP)`:
   - **Text:** "Quit"

#### G. Add Script and Wire Up Buttons
1. Select `MainMenu_Canvas`
2. Add Component: **SimpleMainMenu** script
3. Select `StartButton`:
   - In **Button > OnClick()**, click **+**
   - Drag `MainMenu_Canvas` to the object field
   - Select function: **SimpleMainMenu > StartGame()**
4. Select `QuitButton`:
   - In **Button > OnClick()**, click **+**
   - Drag `MainMenu_Canvas` to the object field
   - Select function: **SimpleMainMenu > QuitGame()**

---

## Part 2: Setup Pause Menu in TheBar Scene

### Step 1: Open TheBar Scene
1. In Project window, navigate to `Assets/Scenes/TheBar.unity`
2. Double-click to open

### Step 2: Create Pause Menu UI

#### A. Find or Create Canvas
- If `UI_Canvas` already exists, use it
- Otherwise, create: Right-click Hierarchy > **UI > Canvas**

#### B. Create Pause Panel
1. Right-click Canvas > **UI > Panel**
2. Rename to `PausePanel`
3. Set properties:
   - **Anchor:** Stretch both (Alt+Shift+Click)
   - **Left/Right/Top/Bottom:** 0
   - **Image > Color:** Black with transparency `RGBA(0, 0, 0, 200)` or `#000000` with Alpha ~0.78

#### C. Create Pause Title
1. Right-click `PausePanel` > **UI > Text - TextMeshPro**
2. Rename to `PauseTitle`
3. Set properties:
   - **Rect Transform:**
     - Anchor: Top Center
     - Pos X: 0, Pos Y: -100
     - Width: 400, Height: 80
   - **TextMeshPro:**
     - Text: "PAUSED"
     - Font Size: 48
     - Alignment: Center & Middle
     - Color: White

#### D. Create Button Container
1. Right-click `PausePanel` > **UI > Empty**
2. Rename to `ButtonContainer`
3. Add Component: **Vertical Layout Group**
   - **Spacing:** 20
   - **Child Alignment:** Middle Center
   - **Child Force Expand:** Width ✓, Height ✗
4. Add Component: **Content Size Fitter**
   - **Vertical Fit:** Preferred Size
5. Set Rect Transform:
   - Anchor: Middle Center
   - Pos X: 0, Pos Y: 0
   - Width: 300, Height: 250

#### E. Create Resume Button
1. Right-click `ButtonContainer` > **UI > Button - TextMeshPro**
2. Rename to `ResumeButton`
3. Set Button properties:
   - **Image > Color:** Light Gray `RGB(200, 200, 200)`
   - **Transition:** Color Tint
4. Set Rect Transform:
   - **Height:** 60
5. Select child `Text (TMP)`:
   - **Text:** "Resume"
   - **Font Size:** 24
   - **Color:** Black
   - **Alignment:** Center & Middle

#### F. Create Back to Menu Button
1. Duplicate `ResumeButton`
2. Rename to `MenuButton`
3. Select child `Text (TMP)`:
   - **Text:** "Back to Menu"

#### G. Create Quit Button
1. Duplicate `ResumeButton`
2. Rename to `QuitButton`
3. Select child `Text (TMP)`:
   - **Text:** "Quit"

#### H. Add Script and Wire Up Buttons
1. Select Canvas (e.g., `UI_Canvas`)
2. Add Component: **SimplePauseMenu** script
3. In Inspector:
   - **Pause Panel:** Drag `PausePanel` to this field
4. Select `ResumeButton`:
   - In **Button > OnClick()**, click **+**
   - Drag Canvas to the object field
   - Select function: **SimplePauseMenu > Resume()**
5. Select `MenuButton`:
   - In **Button > OnClick()**, click **+**
   - Drag Canvas to the object field
   - Select function: **SimplePauseMenu > LoadMenu()**
6. Select `QuitButton`:
   - In **Button > OnClick()**, click **+**
   - Drag Canvas to the object field
   - Select function: **SimplePauseMenu > QuitGame()**

#### I. Hide Pause Panel by Default
1. Select `PausePanel` in Hierarchy
2. **Uncheck** the checkbox at the top of the Inspector to disable it

### Step 3: Save Scene
- Press **Ctrl+S** to save TheBar scene

---

## Part 3: Testing

### Test Main Menu
1. Open `MainMenu` scene
2. Press **Play**
3. Click **Start Game** → Should load TheBar scene
4. Return to MainMenu scene, press Play
5. Click **Quit** → Should exit play mode (or quit in build)

### Test Pause Menu
1. Open `TheBar` scene
2. Press **Play**
3. Press **Escape** → Pause panel should appear, game should freeze
4. Click **Resume** → Panel hides, game resumes
5. Press **Escape** again to pause
6. Click **Back to Menu** → Should load MainMenu scene
7. Start game again, pause, click **Quit** → Should exit

---

## Visual Style Summary

**Main Menu:**
- Background: Dark Gray `#282828`
- Title: White, 72pt, centered
- Buttons: Light Gray `#C8C8C8`, Black text, 24pt
- Layout: Clean vertical stack, centered

**Pause Menu:**
- Overlay: Semi-transparent Black `RGBA(0,0,0,200)`
- Title: White "PAUSED", 48pt
- Buttons: Light Gray, Black text, 24pt
- Layout: Centered vertical stack

**Design Philosophy:**
- Clean and legible
- Standard Unity UI components
- No pixel art or stylized graphics
- Simple flat colors
- Professional and functional

---

## Troubleshooting

**Issue: Scene not loading**
- Verify both scenes are added to Build Settings (File > Build Settings)
- Check scene names match exactly: "MainMenu" and "TheBar"

**Issue: Pause menu doesn't appear**
- Ensure PausePanel is assigned in SimplePauseMenu script
- Check that PausePanel is a child of the Canvas
- Verify Canvas has EventSystem in scene

**Issue: Buttons don't work**
- Check OnClick() events are properly assigned
- Ensure correct GameObject is dragged to object field
- Verify correct function is selected from dropdown

**Issue: Time doesn't resume after pause**
- SimplePauseMenu automatically handles this
- If issues persist, check that Time.timeScale is being reset in LoadMenu()

---

## Complete! 🎉

Your minimalist UI system is now ready. The game flow is:
1. **MainMenu** → Start Game → **TheBar**
2. In **TheBar** → Press Escape → Pause Menu
3. From Pause → Resume / Back to Menu / Quit
