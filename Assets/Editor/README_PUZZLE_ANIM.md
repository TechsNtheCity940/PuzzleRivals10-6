How to run PuzzleAnimationGenerator

1. Open the project in Unity Editor (Editor scripts run only inside Unity).
2. In the top menu: Tools > Puzzles > Generate Animations from Sheets
   - The script will:
     - Ensure *_sheet.png are imported as multiple sprites (4x4 by convention)
     - Create AnimationClips (12 FPS) and AnimatorControllers per sheet
     - Create centered Prefabs in Assets/Prefabs/Puzzles using a SpriteRenderer + Animator
3. Verify each prefab in the Prefab browser and place into scenes. Adjust pixelsPerUnit or scale per-game if needed.

Notes:
- Originals are kept in source; the generator overwrites import settings to ensure consistent slicing and center pivot.
- If you want different fps or naming, edit Assets/Editor/PuzzleAnimationGenerator.cs and re-run from the menu.
