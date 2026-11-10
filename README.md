# Puzzle Rivals

A cross-platform puzzle match game with online multiplayer capabilities.

## Project Overview

Puzzle Rivals is a match-based puzzle game featuring:
- Multiple puzzle types: Collapse Grid, Pipe Pulse, Hex Pulse, Mirror Maze, Card Matching
- Online multiplayer via Unity Multiplayer Center SDK and Supabase backend
- WebGL deployment for browser play

## Getting Started

### Prerequisites
- Godot 4.2.x (for Godot-based builds)
- Unity 6.0+ (for WebGL deployment)
- Node.js 18+ (for web development)

### Development Setup

1. Clone the repository
2. Copy `.env.example` to `.env` and fill in your credentials:
   ```bash
   cp .env.example .env
   ```

3. Configure your backend:
   - Supabase URL: Set in `.env` as `VITE_SUPABASE_URL`
   - Supabase API Key: Set in `.env` as `VITE_SUPABASE_PUBLISHABLE_KEY`

## Project Structure

```
PuzzleRivals/
├── Assets/                    # Game assets
│   ├── Animations/            # Animation clip folders
│   │   ├── Puzzles/         # Puzzle animations (generated)
│   │   └── Effects/           # Visual effect animations
│   ├── Prefabs/              # Prefab instances
│   ├── Scenes/               # Unity scenes
│   ├── Scripts/              # C# scripts
│   │   ├── Puzzles/          # Puzzle type implementations
│   │   └── Managers/         # Game managers
│   ├── Textures/             # Texture assets
│   │   ├── Puzzles/          # Animation sheet textures
│   │   └── Effects/Anim/     # Effect animation sheets
│   └── Editor/               # Editor scripts
├── .github/workflows/        # GitHub Actions
│   └── webgl-deploy.yml      # WebGL build and deploy
├── Packages/                  # Unity packages
└── VERIFICATION_SUMMARY.md  # Project verification notes
```

## Build & Deployment

### WebGL Build

The project uses a custom build script for WebGL deployment:

```bash
# Build using Unity CLI
unity-editor -batchmode -executeMethod BuildScript.PerformWebGLBuild
```

Or use the GitHub Actions workflow for CI/CD deployment to GitHub Pages.

### Animation Setup

Animations are generated from texture sheets located in:
- `Assets/Textures/Puzzles/` - Puzzle animations
- `Assets/Textures/Effects/Anim/` - Visual effects

Run the editor setup:
```
PuzzleRivals → Setup Verified Animation Sheets
```

## Multiplayer Integration

The game uses a hybrid multiplayer approach:
- **Unity Multiplayer Center SDK** (`com.unity.multiplayer.center: 1.0.1`) for real-time multiplayer
- **Supabase backend** for matchmaking and player data

### Configuration

Set the following environment variables:
- `VITE_SUPABASE_URL` - Your Supabase project URL
- `VITE_SUPABASE_PUBLISHABLE_KEY` - Your Supabase anon key

## Supported Platforms

- WebGL (primary deployment target)
- Windows (development)

## Contributing

Please read the CONTRIBUTING.md file for development guidelines.

## License

[License information]

## Support

For issues and questions, please open an issue on the repository.