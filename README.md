**Module:** ECS657U Multi-platform Game Development  
**Target Platform:** PC / Desktop  
**Unity Version:** 2023.1.22f1 (Input System Package installed)  
**Team Members:** Muaaz Raiyan Ahmed, Muhammed Aqib Tunu, Aayan Azfar Aboobakar, Mohammad Sabur Ali Mahdi

## 1. Summary & Core Feature Focus

This prototype explores the idea of core survival, resource gathering, elemental crafting and biome-related combat mechanics. The build demonstrates how elemental weapons affect a mini-boss from a biome whilst also validating inventory interaction.

### Core Tested Features
1. **Multi-Biome Environment & Lighting Engine:** 2 distinct terrain biomes (Forest, Desert) featuring an asset-free, 24-hour procedural day/night sky cycle, ambient color blending, and directional sun rotations.
2. **First-Person Interaction & Resource Gathering:** Input System implementation handling movement, camera control, direct Raycast tree/rock harvesting, dynamic ground drop generation and UI HUD tracking.
3. **Inventory, Crafting & Grid Interactions:** Dynamic inventory UI (toggled using 'E') allowing drag-and-drop gathering of dropped world items into crafting recipes e.g. 'FireSword', 'WaterBucket'.
4. **Elemental Mini-Boss Combat & Game Over State:** AI chases the player and has a melee attack system integrated with elemental weakness calculations ('FireSword' deals 2x damage against 'Ice' Mini-Bosses) and a complete `PlayerStats` death flow with scene restart capabilities.

## 2. Controls & Playtest Instructions

| Action | Control / Keybind | Description |
| :--- | :--- | :--- |
| **Move** | 'W', 'A', 'S', 'D' | Smooth directional character movement |
| **Look Around** | 'Mouse' | First-person camera control |
| **Sprint** | 'Left Shift' | Increases movement speed (drains Stamina bar) |
| **Gather Resource** | 'Left Click' (on Tree / Rock) | Harvests the resources; spawns the labeled 3D item pickup on ground |
| **Attack** | 'Left Click' (when equipped) | Deals melee base/weapon damage to targeted entity/boss |
| **Open / Close Inventory**| `E` | Toggles Inventory overlay and unlocks cursor |
| **Pickup / Store Item** | 'Drag & Drop' (Left Mouse) | Drag ground drop items into the Inventory grid slots |
| **Restart Game** | 'Left Click' on Restart button from UI overlay | Reloads active scene upon player death |

## 3. Key Standout Technical Features

### A. Procedural Day/Night Sky & Lighting Loop ('DayNightManager.cs')
* Built without asset dependencies to have an adherence to strict prototyping constraints.
* Rotates the primary `Directional Light` on a 24-hour cycle.
* Dynamically interpolates ambient sky/equator colors ('RenderSettings.ambientSkyColor') and camera backdrop colors between day and night phases.
* Feeds real-time mathematical time calculations directly into the top-centered `TextMeshPro` UI components in the format of (`Day X`, `HH:MM`).

### B. Dynamic World Loot & Drag-and-Drop Inventory System ('WorldPickup.cs' / 'GridInventory.cs')
* Harvested trees/rocks and defeated mini-bosses execute `WorldPickup.DropNear()`, dynamically creating primitive 3D pickup instances with world-space `TextMeshPro` labels.
* Ground pickups utilize raycasting ground-checks to prevent items from floating or intersecting enemy hitboxes.
* Inventory UI captures canvas drag events, enabling seamless and easy item transfer into inventory slots and crafting recipes.

### C. Elemental Mini-Boss AI & State Management ('MiniBossAI.cs' / 'EnemyHealth.cs')
* Uses Unity's `NavMeshAgent` pathfinding with custom slope '50°' and step-height '0.8m' parameters to be able to traverse a multi-biome terrain.
* Features detection radii (detectionRange = 15m) and timed melee attack coroutines that trigger 'PlayerStats.TakeDamage()'.
* Evaluates weapon signatures via `TakeDamage(damage, weaponType)`; applying a `2.0x` damage multiplier when attacking an Ice Mini-Boss with a `FireSword`.

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
