# Game Update Summary

## 1. GameEnd Scene & Shop System
- **New Scene**: Created `GameEnd` scene for post-game summary and shop.
- **Shop UI**: Implemented a shop interface allowing players to:
  - Upgrade Liquor Levels (Vodka, Gin, Rum, Whiskey, Tequila).
  - Purchase Decorations (Speaker, Plant, Painting).
  - View current Coins balance.
- **Navigation**:
  - "Next Game" button loads `TheBar` scene to start a new round.
  - "Main Menu" button returns to the `MainMenu` scene.
- **Persistence**: `PersistentGameData` tracks coins, upgrades, and purchases across sessions.

## 2. Shaker Improvements
- **QTE Removal**: Removed the mandatory QTE requirement for shaking. The shaker now shakes based on time and animation.
- **Pouring Logic**: Implemented pouring functionality for the Shaker.
  - It can now pour its contents into other containers (like `ServeGlass`).
  - Pouring correctly reduces the volume inside the Shaker.
  - Uses `SphereCast` for better detection of target containers.

## 3. Main Menu Settings
- **Settings Panel**: Added a Settings panel to the Main Menu.
- **Quality Settings**: Included a dropdown to adjust game quality (Low, Medium, High).

## 4. Game Loop Integration
- **GameManager Updates**:
  - Handles scene transitions (`TheBar` <-> `GameEnd` <-> `MainMenu`).
  - Automatically re-initializes references when reloading the game scene.
  - Ensures proper state management (Menu, Playing, GameOver).

The game now has a complete loop: Start -> Play -> Win/Lose -> GameEnd (Shop) -> Next Game.
