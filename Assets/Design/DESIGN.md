# Game Design

This document defines the visual and sensory design for the Mirror Maze puzzle. The goal is to move from a segmented, tile-based appearance to a sleek, "cyber-puzzle" aesthetic with continuous, high-glow light beams and tactile mirror feedback.

## Asset Design

- **Visual Identity:** Cyber-industrial, sleek, high-contrast. The world should feel dark and moody, with the light beam and interactive elements providing the primary light sources.
- **Palette:** 
  - **Dominant:** Deep Charcoal (#121212) / Slate (#1E1E1E) for the environment and floor.
  - **Secondary:** Polished Steel (#C0C0C0) / Glass Blue (#A0D8EF) for mirror frames and surfaces.
  - **Accent:** Neon Cyan (#00F3FF) for the active light beam; Lime Green (#9EFD2E) for the "solved" state beam.

### Component Specifications

- **Mirrors (core):**
  - **Surface:** A semi-transparent, slightly reflective glass material. Use a subtle diagonal shimmer texture to suggest glass without requiring real-time reflections.
  - **Frame:** A metallic, rounded square or hexagonal frame with a recessed neon rim that glows cyan when the beam is passing through it.
  - **Idle:** A slow, pulsing shimmer (3-4 second cycle) on the glass surface.

- **Light Beam (core):**
  - **Style:** A smooth, continuous neon line using a high-bloom material. No visible segments or tile breaks.
  - **Behavior:** The beam should have a subtle "flow" or "pulse" animation traveling along its length.
  - **Solve State:** When the receiver is hit, the entire beam should transition from Neon Cyan to Lime Green over 0.2s with a brief intensity spike (flash).

- **Floor Tiles (core):**
  - **Style:** Replace the square grid with a seamless, dark metallic surface.
  - **Detail:** If a grid is used, it should be an extremely faint, non-distracting hex or circuit-line pattern in a slightly lighter neutral tone (e.g., #1A1A1A on #121212). 
  - **Contrast:** The floor must be significantly darker than the beam to prevent visual "bleeding" and ensure the beam path is the focal point.

## Game Feedback

- **Genre Profile:** Tactical / Strategy with High-Energy polish on success.
- **Interaction Map:**

| Interaction | Tier | Importance | Camera | Time | Transform | Visual | Audio | Rationale |
|------------|------|-----------|--------|------|-----------|--------|-------|-----------|
| Mirror Rotate | core | Light | — | — | Squash & Stretch (1.1x / 0.9x) | Brief shimmer spike | Mechanical clink (±10% pitch) | Confirms tactile rotation |
| Beam Hit Mirror | core | Light | — | — | — | Tiny spark burst | Subtle buzz | Shows connection point |
| Beam Hit Receiver | core | Heavy | Subtle Shake | — | Receiver Bounce | Expanding ring / Wave | Resonant chime | Marks successful routing |
| Puzzle Solve | core | Critical | Pulse Zoom | Hitstop (0.05s) | — | Full beam flash to Green | Success Fanfare | Celebrates completion |

### Sequences

- **Mirror Rotation (core):** Player Click → Mirror rotates 90° with Ease-Out-Back (200ms) → Squash/Stretch overshoot (50ms) → Settle.
- **Solve Event (core):** Beam reaches Receiver → 0.05s Hitstop → Beam color ramp to Green (200ms) → Screen-space pulse effect → Solve UI appears.

## UI Design

- **Layout:** Minimalist HUD. The puzzle itself is the center of attention.
- **Color System:** Use the Accent Cyan for all "interactive" UI elements (e.g., "Rotate" button if using UI, or mirror selection highlights).
- **Rounding:** Generous 12px-16px corner radii for all panel and button containers to maintain the sleek "cyber" feel.
- **Inputs:** Mirrors should show a subtle "glow" highlight on hover to indicate they are interactive.
