# Dream Games — Software Engineering Case Study

A match-and-blast game implemented in C# with Unity's Built-in Render Pipeline. Includes ten levels, Rockets and TNT, special-item combos, three obstacle types, and saved level progression.

## Run in the Unity Editor

1. Clone the repository and add its root folder to Unity Hub.
2. Open it with **Unity 6000.3.10f1** and allow package resolution and asset import to finish.
3. Open [Assets/Scenes/MainScene.unity](Assets/Scenes/MainScene.unity).
4. Set the Game view to **9:16 portrait**, for example **1080 × 1920**.
5. Press **Play**, then click **Level 1** or the displayed current-level button.

Both `MainScene` and `LevelScene` are included in the project's scene list. The game runs directly in the Editor; no player build is required.

## How to play

- Click a horizontally or vertically connected group of at least **two same-color cubes**. Each valid action uses one move; isolated cubes and obstacle clicks do not.
- Groups of **2–3** blast normally, **4-5** create a randomly oriented Rocket, and **6 or more** create TNT at the clicked cell. Cube symbols indicate eligible special-item groups.
- Click a special item to activate it. Rockets clear a row or column; TNT affects a **5 × 5** area. Explosions can activate other specials.
- Click a connected group of specials to combine them: Rockets produce a cross, one TNT with Rockets produces a three-row/three-column cross, and two or more TNTs produce a **7 × 7** explosion. The clicked cell is the combo's center.
- Clear the goals within the available moves. Vases require two hits and can fall; Stones require one special-item hit and stay fixed; each fixed **2 × 2** Chalice Box must be opened before collecting its ten chalices.
- While goals and moves remain, a settled board with no playable group or special item automatically reshuffles its existing cubes without spending a move. Cube colors, obstacles, and empty cells are preserved.
- After a win, click **Tap To Continue** to return to the next level. After a loss, use **Try Again** to replay the level or **×** to return to the main screen.

## Progress and Editor tools

The current level is saved locally using `PlayerPrefs` and survives Editor restarts. Completing level 10 changes the main button to **Finished** and disables it. In-progress boards are not saved; retrying reloads the level, including newly generated `rand` cells.

To select a level or reset progress, **stop Play mode**, open **Tools → Set Current Level**, choose **1–10**, and click **Apply**. Open `MainScene` and press Play again. Selecting level **1** resets progression, including the Finished state.

During Play mode in `LevelScene`, **Tools → Force Win** and **Tools → Force Lose** exercise the result screens. Force Win also advances saved progression.

## EditMode tests

Open **Window → General → Test Runner**, select **EditMode**, and click **Run All**. The suite contains 31 cases covering special-item thresholds, combo selection, obstacle damage, transaction accounting, level loading, and dead-board recovery.

Tests live in `Assets/Editor/Tests`. The ten-level loading check and board tests use real project assets. The `PlayModeIntegration` test automatically enters and exits Play mode to check falling, reshuffling, and input locking; run it from the same EditMode tab. Small obstacle helpers in `Assets/Tests/Support` compile only in the Editor. Scene flow and other visual effects still require manual verification.

## Implementation choices

- **Chalice doors:** the supplied prefab uses one hit point. A damage source that opens the door does not also collect chalices from that box.
- **Damage identity:** one cube blast, one special explosion, or one combined explosion is a single damage transaction. Overlapping hits within it damage a Vase or closed Chalice door once. An open box collects one chalice per adjacent blasted cube or distinct box cell hit by a special. Specials activated by a chain reaction start separate transactions.
- **Fixed obstacles:** Stones and Chalice Boxes block falling and refill from above. Empty cells beneath them remain empty until a falling item can reach them.
- **Turn completion:** input is blocked during explosions, falling, and any reshuffle. Finished levels skip recovery, with a cleared board taking priority over zero remaining moves. Dead boards try a bounded number of cube permutations, then deliberately place an existing same-color pair in adjacent cube cells if needed.

## Project layout

| Location | Contents |
| --- | --- |
| [Assets/Scenes](Assets/Scenes) | Main menu and gameplay scenes |
| [Assets/Scripts](Assets/Scripts) | Board logic, grid items, combos, goals, UI, progression, and effects |
| [Assets/Resources/Levels](Assets/Resources/Levels) | Ten runtime level JSON files; cells are stored left-to-right, starting at the bottom row |
| [Assets/Editor](Assets/Editor) | Level selection and forced-result tools |
| [Assets/Plugins/Demigiant/DOTween](Assets/Plugins/Demigiant/DOTween) | Included tween library and modules |

## Dependencies

**DOTween by Demigiant** is included in the repository and drives gameplay, UI, and effect animations. Its bundled [readme](Assets/Plugins/Demigiant/DOTween/readme.txt) contains documentation and license references.

Unity packages, including the Input System and Unity UI/TextMesh Pro, are declared in [Packages/manifest.json](Packages/manifest.json), with resolved versions in [Packages/packages-lock.json](Packages/packages-lock.json). No third-party dependency injection framework is used.
