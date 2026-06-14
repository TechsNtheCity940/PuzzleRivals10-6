How to run the animation asset generator

1. Open the project in Unity Editor (Editor scripts run only inside Unity).
2. In the top menu: Tools > Animations > Generate Animations from Sheets
   - The script will:
     - Ensure *_sheet.png textures are imported as multiple sprites (4x4 by convention)
     - Create AnimationClips (12 FPS) and AnimatorControllers per sheet
     - Create centered prefabs in Assets/Prefabs/Puzzles and Assets/Prefabs/Effects using a SpriteRenderer + Animator
3. Once animations are generated, run Tools > Animations > Assign Animations To Prefabs to attach controllers to matching prefabs.
4. Run Tools > Animations > Assign Animations To Scenes (Project-wide) to assign controllers to scene objects by name.
5. Verify each prefab and scene object in Unity; adjust scale or sorting order as needed.

Notes:
- Originals are kept in source; the generator overwrites import settings to ensure consistent slicing and center pivot.
- The generator currently processes both puzzle and effect sheet folders. Add new 16-frame effect sheets under Assets/Textures/Effects to include them.
- If a sheet has already been generated and is unchanged, the generator will skip it to save time.
- If you want a different fps or naming convention, edit Assets/Editor/PuzzleAnimationGenerator.cs and re-run from the menu.
