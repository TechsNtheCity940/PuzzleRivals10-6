# Puzzle Rivals: Technical Overview

This document provides a technical deep-dive into the architecture, systems, and implementation details of the Puzzle Rivals Unity project.

## 1. Project Description
**Puzzle Rivals** is a competitive, multi-modal puzzle game designed for WebGL. It blends classic grid-based puzzle mechanics with high-stakes competitive play. The project features two primary game modes: **Ranked Free-For-All (FFA)**, a 4-player survival mode where players pass a "bomb" by solving puzzles, and **Brain VS Brain (BvB)**, a 1v1 speed-race featuring offensive attacks and defensive maneuvers.

Core pillars include:
*   **Skill-Based Competition:** High-speed puzzle solving with escalating difficulty.
*   **Asynchronous Multiplayer:** A robust matchmaking system that falls back to AI bots to ensure instant playability.
*   **Player Progression:** A unified profile system managing ranks (Bronze to Legend), currency (Coins/Gems), and a Season Pass.
*   **Deep Customization:** Extensive cosmetic system (Banners, Emblems, Frames, Avatars, etc.) driven by a central library.

## 2. Gameplay Flow / User Loop
1.  **Boot/Main Menu:** The player starts in `Main.unity`. The `MainMenuController` initializes the `GameState`, starts the dashboard music, and handles navigation to the Profile, Store, and Season Pass screens.
2.  **Mode Selection:** The player enters `ModeSelect.unity`. Choosing a mode sets the `GameState.currentMode` and loads the `Match` scene.
3.  **Matchmaking:** `MatchBootstrap` (in `Match.unity`) triggers `NetManager` to find real players. If a full lobby isn't found within a timeout, AI bots fill the remaining slots.
4.  **Gameplay (Match):**
    *   **FFA:** Players solve shared puzzles to pass the bomb. Failing the fuse timer results in elimination. Last player standing wins.
    *   **BvB:** Players solve independent puzzles of the same type. Solves fill a score meter. Reaching checkpoints triggers attacks on the opponent. First to reach the target score wins.
5.  **Post-Match:** Results are calculated. `ProfileManager` updates Rank Points (RP) and Currency. `SeasonPassManager` adds XP. Players return to the Main Menu.

## 3. Architecture
The project follows a **Manager-centric architecture** with clear separation between game logic, UI, and persistent data.

### Match Orchestration
*   `MatchBootstrap`: The entry point for the match scene. It handles the transition from matchmaking to gameplay by instantiating the correct manager.
*   `RankedMatchManager`: Logic for the 4-player FFA mode. Manages the "bomb" rotation, player elimination, and difficulty scaling.
*   `BrainVSBrainManager`: Logic for the 1v1 race mode. Tracks scores, handles attack/defense logic, and manages dual puzzle instances.

### Core Data & State
*   `GameState`: A persistent singleton (`DontDestroyOnLoad`) storing session-specific data like the current mode, audio settings, and shared asset references (colors, names).
*   `ProfileManager`: Handles persistent player data using `PlayerPrefs` and JSON. It serves as the single source of truth for ranks, currency, and unlocks.

`Location: Assets/Scripts/Managers`

## 4. Game Systems & Domain Concepts

### Puzzle System
Puzzles are implemented as UI-based overlays.
*   `BasePuzzle`: Abstract base class for all puzzles. It defines the lifecycle (`Initialize`, `Start`, `Solve`, `TimeOut`) and provides hooks for BvB attacks (e.g., `ApplyAttack`, `AutoSolveChunk`).
*   **Concrete Puzzles**: Classes like `ComboRushPuzzle`, `CircuitScramblePuzzle`, and `LinkLockPuzzle` implement specific logic by overriding `BuildBoard` and handling user interaction.
*   **Difficulty Scaling**: Puzzles accept a `difficulty` parameter that typically adjusts grid size or complexity.

`Location: Assets/Scripts/Puzzles`

### AI Bot System
Bots are integrated directly into the match managers to ensure lobbies are always full.
*   **Logic**: Bots operate on a coroutine-based "chunk" solver. Every few seconds, the bot calls `AutoSolveChunk()` on its current `BasePuzzle` until solved.
*   **Tuning**: Bot speed is adjustable via `botSolveInterval` in the match managers, allowing for difficulty balancing.

### Combat/Attack System (BvB)
In Brain VS Brain, solving puzzles fuels a combat layer.
*   **Attacks**: `Freeze` (pauses fuse/input), `Scramble` (rebuilds board), `InvertControls`, and `Blur`.
*   **Defenses**: `Block`, `Rebound` (reflects attack), `Absorb` (grants points), and `AutoSolve`.
*   **Logic**: Managed by `BrainVSBrainManager.TriggerAttack`, which checks the target's `DefenseInventory` before applying the effect.

## 5. Scene Overview
*   `Main`: The hub scene. Contains the `MainMenuController` and the global `GameState` prefab.
*   `ModeSelect`: A simple transition scene for choosing between FFA and BvB.
*   `Match`: The primary gameplay scene. It is "empty" by design, with `MatchBootstrap` spawning the necessary HUDs and Match Managers at runtime.
*   `Profile / Store / SeasonPass`: Dedicated screens for meta-game progression and customization.

`Location: Assets/Scenes`

## 6. UI System
The project uses a mix of **UI Toolkit (UITK)** for static menus and **UGUI** for dynamic gameplay overlays.

*   **Static Menus**: `MainMenuController` uses `UIDocument` and `VisualElement` queries (`Q<Button>`) to handle interaction.
*   **Gameplay HUDs**: `ArenaHudController` (FFA) and `BvBHudController` (BvB) use standard UGUI components (`Image`, `TMP_Text`) and event-based binding to managers.
*   **Overlays**: Puzzles are prefabs containing a Canvas and a concrete `BasePuzzle` script. They are instantiated by managers and parented to the match Canvas.

`Location: Assets/Scripts/UI`

## 7. Asset & Data Model
*   **CosmeticLibrary**: A ScriptableObject that defines all available cosmetic assets, their IDs, rarities, and costs. It also defines the RP thresholds for rank tiers.
*   **PlayerProfileData**: A serializable class containing all persistent data, saved as a JSON string in `PlayerPrefs`.
*   **Puzzle Prefabs**: Found in `Assets/Prefabs/UI/`, these contain the visual representation and logic for each puzzle type.

`Location: Assets/Resources` (CosmeticLibrary), `Assets/Scripts/Managers` (ProfileData)

## 8. Notes, Caveats & Gotchas
*   **Matchmaking**: The `NetManager` is currently a client-side stub. It simulates a network search but defaults to filling slots with AI. To implement real multiplayer, the `TryParseRemoteHumans` method must be wired to a real backend.
*   **Puzzle Input**: Input is gated by `BasePuzzle.CanAcceptInput()`, which checks if the puzzle is running, if the player is local, and if they are currently "Frozen" by an attack.
*   **Scene Loading**: Most logic relies on `GameState.Instance` being present. Always start from the `Main` scene in the editor to ensure proper initialization.
*   **Canvas Scaling**: `MatchBootstrap` and `RankedMatchManager` both contain `EnsureCanvas` logic to dynamically create a `MatchCanvas` if one is missing, defaulting to a 1080x1920 reference resolution.