# UI Prompt Manager System

## Overview
The UI Prompt Manager displays temporary text messages below the crosshair. Messages automatically disappear after 3 seconds, and new messages immediately replace existing ones.

## Features
- ✅ Displays black text below the crosshair
- ✅ Auto-hides after 3 seconds
- ✅ New messages override existing ones
- ✅ Simple static API for easy access from anywhere
- ✅ Supports both legacy UI.Text and TextMeshProUGUI

## Setup
The system is already set up in the scene:
- **UI_Canvas** has the `UIPromptManager` component
- **PromptText** is positioned below the crosshair
- Text is black and centered

## Usage

### Basic Usage
```csharp
using BarSimulator.UI;

// Show a prompt
UIPromptManager.Show("Picked up Gin");

// Manually hide (optional, auto-hides after 3 seconds)
UIPromptManager.Hide();
```

### Integration Examples

#### 1. Picking Up Items
```csharp
void PickUpItem(InteractableItem item)
{
    // Your pickup logic...
    
    // Show prompt
    UIPromptManager.Show($"Picked up {item.itemName}");
}
```

#### 2. Earning Coins
```csharp
void EarnCoins(int amount)
{
    // Your coin logic...
    
    // Show prompt
    UIPromptManager.Show($"Get {amount} coin");
}
```

#### 3. Pouring Drinks
```csharp
void PourLiquid(LiquidType liquid, float amount)
{
    // Your pour logic...
    
    // Show prompt
    UIPromptManager.Show($"Poured {amount}ml of {liquid}");
}
```

#### 4. Serving Drinks
```csharp
void ServeDrink(string drinkName, int reward)
{
    // Your serve logic...
    
    // Show prompt
    UIPromptManager.Show($"Served {drinkName} - Get {reward} coin");
}
```

#### 5. Any Game Event
```csharp
// The system is flexible - use it for any game event
UIPromptManager.Show("Level Up!");
UIPromptManager.Show("Achievement Unlocked!");
UIPromptManager.Show("New Recipe Available!");
```

## How It Works

### Message Display
1. Call `UIPromptManager.Show("message")`
2. Text appears below crosshair
3. After 3 seconds, text automatically disappears

### Message Override
```csharp
UIPromptManager.Show("First message");
// User sees "First message"

UIPromptManager.Show("Second message");
// "First message" is immediately replaced with "Second message"
// Timer resets to 3 seconds
```

## Customization

### Change Display Duration
Edit the `displayDuration` field in the Inspector:
- Select **UI_Canvas** in the hierarchy
- Find **UIPromptManager** component
- Adjust **Display Duration** (default: 3 seconds)

### Change Text Color
Edit the `textColor` field in the Inspector:
- Select **UI_Canvas** in the hierarchy
- Find **UIPromptManager** component
- Adjust **Text Color** (default: black)

### Change Text Position
- Select **PromptText** in the hierarchy
- Adjust **Anchored Position Y** in RectTransform
- Default: Y = -50 (50 pixels below center)

### Change Text Size
- Select **PromptText** in the hierarchy
- Adjust **Font Size** in Text component
- Default: 24

### Change Text Width
- Select **PromptText** in the hierarchy
- Adjust **Size Delta X** in RectTransform
- Default: 600 pixels wide

## Testing

### Test in Editor
1. Add the `UIPromptExample` component to any GameObject
2. Enter Play Mode
3. Press number keys to test:
   - **1**: "Picked up Gin"
   - **2**: "Picked up Shaker"
   - **3**: "Picked up ServeGlass"
   - **4**: "Picked up Jigger"
   - **5**: "Get 200 coin"
   - **6**: Custom message
   - **H**: Hide prompt manually

### Test in Your Code
```csharp
// Add this to any script
void Update()
{
    if (Input.GetKeyDown(KeyCode.T))
    {
        UIPromptManager.Show("Test message!");
    }
}
```

## Integration Checklist

To integrate the prompt system into your game:

- [ ] Add `using BarSimulator.UI;` to your scripts
- [ ] Call `UIPromptManager.Show("message")` when picking up items
- [ ] Call `UIPromptManager.Show("message")` when earning coins
- [ ] Call `UIPromptManager.Show("message")` for other game events
- [ ] Test that messages appear and disappear correctly
- [ ] Adjust text size/position if needed

## Common Integration Points

### ItemInteractionSystem.cs
```csharp
// In HandleInteractionInput() method
if (Input.GetKeyDown(KeyCode.E))
{
    if (heldObject == null && currentHighlightedObject != null)
    {
        InteractableItem item = currentHighlightedObject.GetComponent<InteractableItem>();
        if (item != null)
        {
            // Existing pickup code...
            
            // Add this line:
            UIPromptManager.Show($"Picked up {item.itemName}");
        }
    }
}
```

### Coin/Reward System
```csharp
public void AddCoins(int amount)
{
    coins += amount;
    
    // Add this line:
    UIPromptManager.Show($"Get {amount} coin");
}
```

### NPC Serving System
```csharp
public void ServeDrink(NPCData npc, DrinkRecipe recipe)
{
    // Existing serve logic...
    
    // Add this line:
    UIPromptManager.Show($"Served {recipe.recipeName}");
}
```

## Troubleshooting

### Prompt Not Showing
1. Check that **UI_Canvas** has the **UIPromptManager** component
2. Check that **promptText** field is assigned in the Inspector
3. Check that **PromptText** GameObject exists in the hierarchy
4. Check the console for warning messages

### Text Not Visible
1. Check that **PromptText** is not behind other UI elements
2. Check that text color is not the same as background
3. Check that Canvas is set to **Screen Space - Overlay**
4. Check that **PromptText** has a **Text** or **TextMeshProUGUI** component

### Messages Not Auto-Hiding
1. Check that **Display Duration** is set (default: 3)
2. Check that the game is not paused
3. Check the console for errors

## API Reference

### Static Methods
```csharp
// Show a prompt message
UIPromptManager.Show(string message)

// Hide the prompt immediately
UIPromptManager.Hide()
```

### Instance Methods
```csharp
// Show a prompt (use static method instead)
Instance.ShowPrompt(string message)

// Hide the prompt (use static method instead)
Instance.HidePrompt()
```

### Properties
```csharp
// Get the singleton instance
UIPromptManager.Instance
```

## Notes
- Messages are displayed in the order they are called
- Each new message resets the 3-second timer
- The system uses a singleton pattern for easy access
- Compatible with both legacy UI and TextMeshProUGUI
- No performance impact when not displaying messages
