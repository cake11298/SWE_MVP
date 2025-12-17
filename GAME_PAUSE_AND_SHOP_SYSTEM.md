# Game Pause and Shop System Implementation

## Overview
This document describes the implementation of complete game pause functionality and the shop system for liquor upgrades and decorations.

## Changes Made

### 1. Complete Game Pause System

#### GameManager.cs
- **Paused State**: Now completely stops the game by setting `Time.timeScale = 0f` and disabling player input
- **GameOver State**: Changed to keep `Time.timeScale = 0f` (was 1f) to prevent any game updates during the end screen
- **TriggerGameEnd()**: Now explicitly pauses the game and disables player input when time runs out

#### SimplePauseMenu.cs
- **Pause()**: Now disables player controller input in addition to setting `Time.timeScale = 0f`
- **Resume()**: Re-enables player controller input when resuming
- **LoadMenu()**: Re-enables player input before transitioning to main menu
- **ForceGameEnd()**: Keeps game paused and properly triggers GameEndUI

#### GameEndUI.cs
- **ShowGameEndScreen()**: Explicitly disables player input when showing the end screen
- **OnMainMenuClicked()**: Re-enables player input before loading MainMenu scene
- **OnNextGameClicked()**: Re-enables player input before reloading TheBar scene

### 2. Liquor Upgrade System

#### PersistentGameData.cs
- **BaseLiquorType Enum**: Changed from 5 types (Vodka, Gin, Rum, Whiskey, Wine) to (Vodka, Gin, Rum, Whiskey, Tequila)
- **LiquorUpgradeData**:
  - Max level: 5 (unchanged)
  - Upgrade cost: 1000 coins per level (unchanged)
  - All upgrades cost the same regardless of level

#### GameEndUI.cs
- **Left Panel**: Displays all 5 base liquors with upgrade buttons
- **Upgrade Button**: Shows current level (Lv.X/5) and upgrade cost ($1000)
- **Button State**: 
  - Enabled if player has enough coins and liquor is not max level
  - Shows "MAX" when liquor reaches level 5
  - Grayed out if insufficient coins

### 3. Decoration Shop System

#### PersistentGameData.cs
- **DecorationData**: Updated cost system
  - Speaker: 2000 coins (changed from 3000)
  - Plant: 3000 coins
  - Painting: 3000 coins

#### GameEndUI.cs
- **Right Panel**: Displays all decoration options
- **Purchase Button**: 
  - Shows purchase cost
  - Enabled if player has enough coins and decoration not purchased
  - Shows "Purchased" when already owned
  - Chinese names: 音箱 (Speaker), 盆栽 (Plant), 畫 (Painting)

#### DecorationManager.cs
- **Auto-Find**: Automatically finds all objects with "SM_Speakers" in their name
- **Visibility Control**: 
  - Hides speakers by default
  - Shows speakers immediately when purchased
  - Shows speakers on next game start if already purchased
- **Event System**: Subscribes to `OnDecorationPurchased` event to update visibility in real-time

### 4. Game Flow

#### When Time Runs Out or P is Pressed:
1. Game completely pauses (`Time.timeScale = 0f`)
2. Player input is disabled
3. GameEndPanel is shown with:
   - Center: Game statistics (drinks served, total coins)
   - Left: Liquor upgrade shop (5 base liquors)
   - Right: Decoration shop (Speaker, Plant, Painting)

#### Continue Game Button:
1. Resumes time (`Time.timeScale = 1f`)
2. Re-enables player input
3. Reloads TheBar scene
4. **Inherits**: Total coins, liquor levels, decoration purchases (via PersistentGameData)

#### Main Menu Button:
1. Resumes time (`Time.timeScale = 1f`)
2. Re-enables player input
3. Loads MainMenu scene
4. **Keeps**: All persistent data (coins, upgrades, decorations)

#### Pause Menu (ESC):
1. Completely pauses game (`Time.timeScale = 0f`)
2. Disables player input
3. Shows pause panel
4. Resume button re-enables everything

## Technical Details

### Time.timeScale = 0f
- Stops all time-based operations in Unity
- Affects: Update(), FixedUpdate(), animations, physics, timers
- Does NOT affect: UI interactions, Input system

### Player Input Control
- Disabled during pause, game over, and end screen
- Re-enabled when resuming or transitioning scenes
- Prevents player from moving/interacting while UI is active

### Persistent Data
- Stored in PersistentGameData singleton
- Survives scene transitions via DontDestroyOnLoad
- Includes: total coins, liquor levels (1-5), decoration purchases

### Decoration Visibility
- Controlled by DecorationManager
- Updates on scene start and when decorations are purchased
- Speakers remain hidden until purchased, then visible in all future games

## Testing Checklist

- [ ] Press ESC to pause - game should completely freeze
- [ ] Press ESC again to resume - game should continue normally
- [ ] Wait for time to run out - GameEndPanel should appear with game frozen
- [ ] Purchase liquor upgrade - should deduct 1000 coins and increase level
- [ ] Purchase Speaker decoration - should deduct 2000 coins
- [ ] Click "Continue Game" - should reload scene with coins/upgrades intact
- [ ] Verify speakers are visible in new game after purchase
- [ ] Click "Main Menu" - should load MainMenu scene
- [ ] Start new game from MainMenu - should keep all coins/upgrades

## Notes

- All 5 base liquors can be upgraded from Level 1 to Level 5
- Each upgrade costs 1000 coins regardless of current level
- Speakers cost 2000 coins (cheaper than other decorations)
- Decorations are one-time purchases (no levels)
- Game is completely frozen during pause and end screen
- Player cannot move or interact while any UI panel is active
