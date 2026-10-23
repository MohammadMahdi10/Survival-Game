
# PlanetFall — Survival Game Prototype

**Module:** ECS657U Multi-platform Game Development  
**Coursework:** Coursework 1 — Prototype Group Submission  
**Game Name:** PlanetFall  
**Group:** Survival_4  
**Target Platforms:** PC / Desktop and WebGL  
**Unity Version:** 2023.1.22f1  
**Input System:** Unity Input System Package  

**Team Members:**
- Mohammad Sabur Ali Mahdi
- Muhammed Aqib Tunu
- Muaaz Raiyan Ahmed
- Aayan Azfar Aboobakar

**WebGL Build:** https://mohammadmahdi10.github.io/Survival-Game/build/index.html

---

## 1. Game Overview

PlanetFall is a first person survival game where the player crash lands on an unfamiliar alien planet. To escape, the player must explore different biomes, gather resources, craft equipment, manage survival needs and defeat bosses to obtain the components required to repair their spaceship.

The prototype focuses on demonstrating the game's main gameplay loop and technically challenging mechanics, including:

- First person movement and exploration
- Resource gathering across different biomes
- Tile based inventory management
- Crafting and equipment
- Enemy AI and melee combat
- Health, saturation and stamina management
- Day and night cycle
- Quest progression
- Progressive difficulty scaling
- Time limited hardcore mode
- Spaceship repair and victory condition

These features were selected because they form the foundation of the intended game and require multiple gameplay systems to work together.

---

## 2. Gameplay Objective

The player's main objective is to repair their crashed spaceship and escape the planet.

### Main Quest Progression

1. Explore the planet and gather resources.
2. Craft and equip weapons to prepare for combat.
3. Defeat the first mini boss.
4. Collect the special item dropped by the mini boss.
5. Travel to the tower and place the special item.
6. Defeat the final boss.
7. Collect the repair component.
8. Return to the crashed spaceship.
9. Repair the spaceship to complete the game.

The quest tracker displays the current objective and updates as the player progresses.

### Time Limit

The intended full game gives the player approximately 10 in game days to complete the main objective.

For the prototype, the deadline has been shortened to Day 5 to make the hardcore mechanics easier to demonstrate and test.

If the player has not repaired the spaceship before hardcore mode activates, the game becomes more difficult and the player loses the ability to respawn after death.

---

## 3. Controls and Playtest Instructions

| Action | Control | Description |
|---|---|---|
| Move | W, A, S, D | Move around the environment |
| Look Around | Mouse | Control the first person camera |
| Sprint | Left Shift | Move faster while consuming stamina |
| Jump | Space | Jump over obstacles |
| Gather Resources | Left Mouse Button | Aim at a gatherable resource and click |
| Attack | Left Mouse Button | Attack enemies while the gameplay cursor is locked |
| Block | Hold Right Mouse Button | Block incoming attacks while stamina is available |
| Open / Close Inventory | E | Open or close the inventory interface |
| Move Inventory Items | Left-click and drag | Rearrange items or transfer them between inventory grids |
| Rotate Inventory Item | R while dragging | Rotate a held item by 90 degrees |
| Equip Item | F while hovering over an item | Equip a supported weapon or item |
| Unequip Item | F while hovering over the equipped slot | Return the equipped item to the inventory |
| Interact | F | Interact with quest objects such as the tower and spaceship |
| Select Crafting Recipe | Left Mouse Button | Select a recipe from the crafting menu |
| Craft Item | Craft button | Craft the selected item when enough resources are available |
| Respawn | Respawn button | Respawn after death in normal mode |
| Restart | Restart button, where available | Restart the game through the relevant UI |

**Important Notes:**

- Opening the inventory unlocks the cursor and disables player movement and mouse look.
- Ground items can be collected through the ground inventory panel when the player is nearby.
- Items must fit within the available inventory cells to be placed.
- Some items can be rotated to fit into smaller spaces.
- Hardcore mode prevents respawning after death.
- Completing the spaceship repair objective activates the victory screen.

---

## 4. Core Technical Features

### A. Environment and Biomes

The prototype contains two main biomes:

- **Plains:** Contains trees, stone resources and areas for exploration.
- **Desert:** Provides a different environment with its own stone resources.

The environment includes terrain, mountains, trees, rocks, environmental structures and the crashed spaceship.

The different biomes encourage players to explore the map and gather materials needed for crafting and progression.

The prototype uses a manually constructed environment to provide predictable conditions for testing gameplay mechanics.

### B. Player Movement and Survival

**Key Scripts:** `PlayerMovement.cs`, `PlayerStats.cs`, `CameraMovement.cs`

The player can move, jump, sprint and explore the environment using first person controls.

The survival system manages several player attributes:

- Health
- Stamina
- Saturation

Stamina is consumed during activities such as sprinting and blocking.

Saturation is continuously being reduced. To saturate, eating or drinking water increases it (for the prototype this has not been implemented). In hardcore mode, this will reduce faster.

Health and other survival attributes are displayed through the player HUD, allowing the player to monitor their current condition.

These mechanics demonstrate how survival needs influence the player's decisions while exploring and fighting enemies.

### C. Resource Gathering and World Items

**Key Scripts:** `PlayerGathering.cs`, `GatherResource.cs`, `GroundItems.cs`, `WorldPickup.cs`

Players can interact with gatherable resources such as trees and rocks.

When a resource is collected, its corresponding item can appear in the world as a pickup.

The item pickup system:

- Spawns resource items into the environment.
- Displays item names above world pickups.
- Allows nearby items to appear in the ground inventory.
- Supports transferring items into the player's backpack.
- Returns remaining ground inventory items to the world when the interface closes.

This connects resource gathering with the inventory and crafting systems.

### D. Tile Based Inventory System

**Key Scripts:** `GridInventory.cs`, `ItemShapes.cs`, `InventoryDragDrop.cs`, `InventoryGridUI.cs`

Unlike a traditional inventory, items occupy different shapes and numbers of grid cells.

The system supports:

- Different shapes for different item types
- Item rotation
- Drag and drop movement
- Placement validation
- Collision and overlap prevention between inventory items
- Transferring items between ground and player inventories
- Visual previews while dragging
- Item names and colours
- Equipping supported items

The inventory system uses reusable placement and shape-handling methods so the same logic can manage different inventory grids.

This mechanic adds a strategic element to resource management because players must organise their limited storage space carefully.

### E. Crafting and Equipment

**Key Scripts:** `CraftingManager.cs`, `CraftingUI.cs`, `RecipePageUI.cs`, `EquipmentManager.cs`

The crafting system allows players to create equipment using collected resources.

Prototype crafting recipes include:

- Basic Sword
- Fire Sword
- Water Bucket

Each recipe requires a specific combination of materials.

The crafting system checks whether the player has enough ingredients before allowing the item to be crafted.

Once an item is crafted:

- Required materials are removed from the backpack.
- The crafted item is added to the inventory if space is available.
- If the backpack is full, the crafted item is dropped near the player to prevent item loss.

Players can equip supported items and return previously equipped equipment to their inventory.

### F. Enemy AI and Combat

**Key Scripts:** `MiniBossAI.cs`, `EnemyHealth.cs`, `MeleeCombat.cs`, `BlockCombat.cs`, `WeaponStats.cs`

The prototype includes enemy encounters and boss combat.

Enemy behaviour includes:

- Detecting the player within a specified range
- Following the player using Unity's NavMesh navigation system
- Facing the player during combat
- Attacking when within melee range
- Using attack cooldowns to prevent overlapping attacks

Combat supports different weapon types and elemental damage.

For example, the Fire Sword deals additional damage against ice type enemies, encouraging the player to craft and select suitable equipment.

Enemies have health values and can drop items after being defeated.

The first mini boss and final boss are also connected to the quest progression system.

**Platform Note:** Enemy navigation works in the Unity Editor, but a NavMeshAgent movement issue has been identified in the WebGL build. The enemy can detect and face the player, but its chasing behaviour does not currently function correctly in the browser version.

### G. Day and Night Cycle

**Key Script:** `DayNightManager.cs`

The day and night system simulates the passage of in game time.

It includes:

- A continuously progressing in-game clock
- Changes to the environment's lighting
- Day and night visual transitions
- Tracking the current in-game day
- A time display on the player HUD

The day counter also supports the prototype's hardcore mode deadline.

In the intended full game, additional mechanics such as increased enemy spawning at night are planned to make night time survival more challenging.

### H. Quest and Progression System

**Key Scripts:** `QuestManager.cs`, `QuestUI.cs`, `TowerInteraction.cs`, `SpaceshipInteraction.cs`

The quest system guides the player through the game's main objectives.

Quest progression follows a fixed sequence:

1. Defeat the mini boss.
2. Collect the special item.
3. Place the item at the tower.
4. Defeat the final boss.
5. Collect the repair component.
6. Return to the spaceship.
7. Repair the spaceship and win.

Each quest stage checks the player's current objective before allowing progression.

This prevents objectives from being completed out of order.

The quest UI updates whenever the player completes a stage, providing clear guidance throughout the prototype.

### I. Progressive World Difficulty

**Key Scripts:** `WorldDifficultyManager.cs`, `PlayerStats.cs`, `MeleeCombat.cs`

The prototype includes a progressive difficulty system.

The world begins with a difficulty multiplier of **×1.0**.

Defeating a mini boss increases the world difficulty by **0.5**, making the multiplier **×1.5** after the first mini boss.

Increasing difficulty affects several gameplay mechanics, including:

- Reducing player maximum health
- Reducing player maximum stamina
- Reducing weapon damage

Difficulty adjustments are calculated through multipliers rather than relying entirely on fixed values.

This allows the system to be expanded in future development as new equipment, player attributes and gameplay mechanics are introduced.

The current world difficulty is displayed on the player's HUD.

### J. Hardcore Mode

**Key Scripts:** `DayNightManager.cs`, `PlayerStats.cs`, `InventoryToggle.cs`

The prototype includes a time based hardcore mode.

For testing purposes, hardcore mode activates on Day 5 if the player has not completed the main objective.

When hardcore mode activates:

- Maximum health and stamina are reduced.
- Hunger and thirst become harder to manage.
- Player weapon damage is reduced.
- The player can no longer respawn after death.
- A hardcore indicator appears on the HUD.

The purpose of hardcore mode is to create additional tension and demonstrate the consequences of missing the game's intended deadline.

### K. Player Death, Respawning and Victory

**Key Scripts:** `PlayerStats.cs`, `QuestManager.cs`, `SpaceshipInteraction.cs`

The prototype contains both a death condition and a victory condition.

**Normal Mode:**
- When the player dies, a death screen appears.
- The player can respawn and continue playing.

**Hardcore Mode:**
- Death remains permanent for the current attempt.
- The respawn option is no longer available.

**Victory:**
- The player defeats the final boss.
- The repair component is collected.
- The player returns to the crashed spaceship.
- Repairing the spaceship completes the main quest and displays the victory screen (for the final game, in hardcore more, the ship is irrepairable so the player can never escape).

These systems provide a complete beginning, progression and ending for the prototype.

---

## 5. Project Architecture and Folder Organisation

The Unity project is organised by asset type and script responsibility to make development and maintenance easier.

### Main Assets Structure

```text
Assets/
├── Art/
│   └── Materials/
├── Prefabs/
│   ├── Characters/
│   ├── Environment/
│   ├── Items/
│   ├── Structures/
│   └── UI/
├── Scenes/
│   ├── Main/
│   │   └── SampleScene.unity
│   └── Prototypes/
│       ├── BossPrototype.unity
│       └── Prototype.unity
├── Scripts/
│   ├── Player/
│   ├── Combat/
│   ├── Mobs/
│   ├── Inventory/
│   │   ├── Core/
│   │   ├── UI/
│   │   └── WorldItems/
│   ├── Crafting/
│   ├── UI/
│   ├── World/
│   └── Managers/
├── Settings/
├── Terrain/
└── ThirdParty/
```

### Script Organisation

**Player**
- Player movement
- Camera movement
- Player statistics
- Resource gathering

**Combat**
- Melee combat
- Blocking
- Weapon statistics

**Mobs**
- Enemy health
- Mini-boss AI
- Non-hostile animal movement

**Inventory**
- Inventory data and placement
- Item shapes and item types
- Inventory UI and drag-and-drop
- Ground items and world pickups

**Crafting**
- Crafting recipes
- Crafting menu
- Recipe page UI

**UI**
- Equipment HUD
- Quest tracker
- Compass and crosshair
- Victory screen

**World**
- Day and night cycle
- Resource objects
- Tower and spaceship interactions

**Managers**
- Input manager
- Resource manager
- Equipment manager
- Quest manager
- World difficulty manager

This structure keeps related functionality grouped together and helps different team members locate and modify scripts without unnecessarily affecting unrelated systems.

---

## 6. Scenes and Testing

### Main Prototype Scene

`Assets/Scenes/Main/SampleScene.unity`

This is the main playable scene and should be used to assess the complete PlanetFall prototype.

It includes the environment, player, survival systems, inventory, crafting, combat, quest progression and spaceship repair objective.

### Additional Prototype Scenes

`Assets/Scenes/Prototypes/BossPrototype.unity`

Used during development for testing boss-related gameplay.

`Assets/Scenes/Prototypes/Prototype.unity`

An additional development scene used for prototyping and testing gameplay mechanics.

These scenes are provided as development and demonstration resources. The main scene should be used to experience the intended gameplay loop.

---

## 7. Running the Project in Unity

1. Clone or download the GitHub repository.
2. Open Unity Hub.
3. Select **Add Project** and choose the `Survival Game` folder containing `Assets`, `Packages` and `ProjectSettings`.
4. Open the project using Unity **2023.1.22f1**.
5. Allow Unity to import the project's assets and packages.
6. Open `Assets/Scenes/Main/SampleScene.unity`.
7. Enter Play Mode to start testing the prototype.

The project uses Unity's Input System package and Unity navigation components for enemy movement.

Ensure the packages required by the project are available and have been imported correctly.

---

## 8. WebGL Build

The browser version of the prototype is available at:

https://mohammadmahdi10.github.io/Survival-Game/build/index.html

The WebGL build allows the game to be tested without opening the Unity Editor.

For the most complete demonstration of the current enemy AI behaviour, use the Unity Editor version while the browser-specific navigation issue is being investigated.

---

## 9. Prototyping Decisions and Technical Challenges

The group prioritised systems that are important to the core survival experience and present meaningful technical challenges. We also prioritised technically risky systems to ensure that the most challenging aspects of the game were tested early in development.

These included:

- Creating a spatial inventory where items occupy different shapes.
- Integrating resource gathering, inventory management and crafting.
- Implementing enemy navigation and elemental combat.
- Connecting boss encounters to quest progression.
- Managing survival attributes alongside changing difficulty modifiers.
- Implementing hardcore mode and permanent death after a deadline.

These features helped the team explore how the game's main mechanics interact and identify challenges associated with integrating systems developed by different team members.

The prototype also demonstrates a complete objective based gameplay loop, allowing the team to evaluate how exploration, crafting, combat and progression work together.

---

## 10. Planned Features for the Full Game

The prototype focuses on the main gameplay systems rather than every planned feature.

Features intended for future development include:

- **Armour System:** Craftable and equippable armour that reduces incoming damage.
- **Building and Mining:** Placing and mining blocks to construct structures.
- **Procedural Generation:** Generating more varied environments and resource locations.
- **Additional Biomes:** More environments with unique resources and challenges.
- **Expanded Crafting:** Additional weapons, tools, armour and recipes.
- **More Enemies and Bosses:** Increased combat variety and more challenging encounters.
- **Expanded Survival Mechanics:** Additional food types and survival resources.
- **Night-Time Threats:** Increased enemy activity and danger after dark.
- **Environmental Detail:** More custom models, textures, structures and points of interest.

These features were left out of the current prototype so the team could prioritise developing and testing the essential gameplay mechanics.

---

## 11. Version Control and Collaboration

The project was developed collaboratively using Git and GitHub.

The team used:

- Individual development branches
- Commits throughout the prototyping process
- Feature integration through merge branches
- Pull requests
- Shared testing and debugging
- Regular communication to coordinate dependent features

Different team members worked on different gameplay systems before combining their work into the main project.

### Repository

https://github.com/MohammadMahdi10/Survival-Game

### Main Branch

`main`

The prototype should be assessed using the submitted version of the main branch.

---

## 12. Known Issues

- Enemy NavMesh movement works in the Unity Editor, but chasing behaviour is currently not functioning correctly in the WebGL build. The boss can still detect and face the player.
- Some platform-specific rendering warnings may appear in the WebGL browser console.

The prototype is intended to demonstrate the game's key systems and is not a final production release.

---

## 13. Team Members

| Team Member | GitHub Username |
|---|---|
| Mohammad Sabur Ali Mahdi | MohammadMahdi10 |
| Muhammed Aqib Tunu | AqibTunu |
| Muaaz Raiyan Ahmed | Muaaz2006 |
| Aayan Azfar Aboobakar | AayanAboobakar |

---

**PlanetFall — ECS657U Multi-platform Game Development, Coursework 1**
