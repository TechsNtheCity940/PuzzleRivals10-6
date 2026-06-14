Neon Rivals Rulebreaker Versus - Next Pass
=========================================

What changed
------------
- Simplified the top HUD to remove the cluttered banner/text pile-up.
- Replaced the blocking rule-shift banner with a compact rule chip that appears above each board and never covers puzzle tiles.
- Added vertical score meters in the center lane beside the boards.
- Turned the bottom panels into player-card areas with username placeholders.
- Added animated header lightning, board-edge neon flicker, tile burst FX, and attack screen flashes.
- Replaced the old four-symbol tile language with icon families (Pulse, Nova, Flux, Vector, Prism).
- Added auto-triggered attacks/defenses based on skillful solves and cooldowns for mobile-first play.
- Added a socket.io multiplayer scaffold.

Important honesty
-----------------
The realtime layer is scaffolded, not yet a full authoritative multiplayer rules server.
Wamp alone will not provide websocket matchmaking. Run the Node socket server alongside Wamp.

How to run the visual build
---------------------------
1. Put your local Phaser build in the project root as phaser.js
2. Serve the folder with Wamp
3. Open in browser

How to run the multiplayer scaffold
-----------------------------------
1. Open a terminal in /server
2. npm install
3. npm start
4. Open the game in two browser windows/devices

Notes
-----
- If the socket server is missing, the game falls back to local duel mode.
- The socket layer is wired in the client and a basic room server is included, but the game still uses local rules resolution. The next real engineering step is moving authoritative match state and board resolution fully into shared/server logic.
