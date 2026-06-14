Automating Puzzle Sprite Redesign

This folder contains scripts to compose, audit, and auto-redesign 16-frame sprite sheets for the PuzzleRivals project.

Scripts:
- make_spritesheets.py: Compose a single set of configured sources into 4x4 sprite sheets.
- make_spritesheets_all.py: Walks Assets/Textures/Puzzles and Assets/Textures/Effects and composes *_sheet.png for each PNG, skipping sheets that are already up to date.
- visual_audit.py: Scans *_sheet.png files and flags single-frame / low-variation assets.
- auto_redesign.py: Generates 16-frame redesign sheets for flagged assets as *_redesign_sheet.png, skipping unchanged redesigns.
- automate_redesign.ps1: Wrapper to run the full pipeline, backup originals, apply redesigns, and optionally commit.

Usage (local):
1. Open PowerShell at the repo root.
2. Run: pwsh tools\automate_redesign.ps1 -Commit $true

CI:
A GitHub Actions workflow is included at .github/workflows/auto-redesign.yml to run the automation on Windows runners.
