**Module:** ECS657U Multi-platform Game Development  
**Target Platform:** PC / Desktop  
**Unity Version:** 2023.1.22f1 (having Input System installed)
**Team Members:** Muaaz Raiyan Ahmed, Muhammed Aqib Tunu, Aayan Azfar Aboobakar, Mohammad Sabur Ali Mahdi

## 1. Summary & Core Feature Focus

We built a working prototype focused on survival basics: gathering resources, crafting elemental items, and fighting biome-specific enemies. Everything you see is functional, from how you collect materials to how elemental weapons deal extra damage to matching bosses and we’ve tested every main feature to make sure it runs smoothly.

### Core Tested Features
1. **Two distinct biomes + dynamic lighting:** We’ve put together a forest area and a desert area, plus a full 24‑hour day/night cycle that changes the sky, ambient light, and sun position automatically where there are no pre‑made assets that were used for this system.
2. **First‑person controls and resource gathering:** All movement and camera handling uses Unity’s new Input System. You can aim at trees or rocks and left click to be able to harvest them and then the resources drop as physical objects which you can then pick up, and your inventory immediately updates to show everything that you have collected.
3.  **Inventory and crafting system:** Press E to open or close your inventory. You can drag items you’ve picked up into slots, and combine materials to make things like a Fire Sword or Water Bucket. Recipes are set up so you can only craft when you have the right items.
4. **Elemental boss combat + game over flow:** We’ve added an ice mini‑boss that chases you and attacks up close. If you hit the mini-boss with the Fire Sword, it takes twice the damage. If your health hits zero, the game shows a death screen and you are prompted with a restart overlay in the UI which you can do to restart the scene straight away.


## 2. Controls & Playtest Instructions
| Action | Control / Keybind | Description |
| :--- | :--- | :--- |
| **Move** | W, A, S, D | Moves your character around |
| **Look Around** | Mouse | Turns your view in first person |
| **Sprint** | Left Shift | Moves you faster, but uses up stamina more quickly |
| **Gather Resources** | Left click (aimed at tree or rock) | Cuts down tree / breaks rock; spawns collectible items on the ground |
| **Attack** | Left click (with weapon equipped) | Hits whatever you’re aiming at, enemies or bosses |
| **Open / Close Inventory**| E | Brings up the inventory screen and lets you use your mouse to move items |
| **Pickup / Store Item** | Drag and drop with left mouse button | Drag items from the world or ground into your inventory slots, or into crafting grids |
| **Restart Game** | Left click Restart button | Reloads the scene so you can play again |

## 3. Key Standout Technical Features

### A. Day/Night Cycle – DayNightManager.cs
* It rotates the main directional light to simulate the sun moving across the sky over a full 24‑hour period.
* It smoothly blends the ambient lighting, sky colour and background colour between day time and night so the change feels natural.
* Calculating the in‑game time mathematically and displaying it on screen which will be shown as Day 1, then the hours and minutes counting up.

### B. Loot System & Drag‑and‑Drop Inventory – WorldPickup.cs / GridInventory.cs
* When you harvest a resource or defeat a boss, the game runs DropNear() to spawn the item nearby as a 3D object with its name floating above it.
* We’ve added ground checks so items always sit flat on the terrain and won’t float in mid‑air or clip through the floor or enemies.
* The inventory reads drag events of the items that are on the ground or you already possess from the UI, so you can pick up items straight from the ground, drag them into your inventory slots, or straight into the crafting options to make something new. Everything updates instantly when you move it.

### . Boss AI & Damage System – MiniBossAI.cs / EnemyHealth.cs
* The boss uses Unity’s NavMesh to find its way around the map and we set the navigation limits to allow slopes up to 50° and steps up to 0.8m high, so it can move properly across our different terrain types.
* It will only start chasing you once you’re within 15 metres. When it gets close, it uses a timed attack routine to damage you.
* Damage calculation checks what weapon you’re using. If you hit the Ice Boss with the Fire Sword, it applies a ×2 multiplier and this is set up so we can easily add more enemy types and elemental match‑ups later.
* Your health is tracked separately so that when it drops to zero, the game stops normal play time because the player has died and then gives you a button which you can click to reload the exact same scene from the beginning.

## 4. Project Architecture & Folder Organization

The project directory strictly follows a clean modular hierarchy:

```text
Assets/
├── Materials/         # Biome ground, UI, and entity materials
├── Prefabs/           # MiniBoss prefabs (Ice, Volcano, Forest, Desert), HUD panels
├── Scenes/
│   └── SampleScene    # Primary combined testing scene (clean layout, no unused assets)
├── Scripts/           # Single-responsibility modular C# C# code structure
│   ├── DayNightManager.cs
│   ├── EnemyHealth.cs
│   ├── GridInventory.cs
│   ├── ItemType.cs
│   ├── MiniBossAI.cs
│   ├── PlayerStats.cs
│   └── WorldPickup.cs
└── TextMeshPro/       # UI rendering assets
