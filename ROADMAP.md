# Duty Engine — Roadmap

**Mission:** Finish a small, polished top-down action game on this engine, then build out 3D.

**Rule:** Every task below ends with *"run the game and see X"*. If you can't run it and see X, it's not done.

---

## How to use this file (the Daily Loop)

1. Open this file. Pick the **first unchecked box**.
2. Split it until it's a task you can finish in **under an hour** (one method, one file, one visible thing).
3. Code it. Run it. Fix it until it runs.
4. Check the box. `git commit`. Stop.

**Never stuck again:** the answer to "what do I do next?" is always *the first unchecked box*.

## Definition of Done (every task)

- [ ] `dotnet build` — no errors
- [ ] Game runs, no exceptions in console
- [ ] You *saw* the feature happen on screen
- [ ] Box checked, committed

---

## Warm-up missions (do these solo — build confidence, ~20 min each)

- [ ] **Coin pickup:** coin disappears when the player touches it (`world.DestroyEntity(coin)` on `COIN ENTERED`)
- [ ] **Crate friction:** crate glides to a stop with small deceleration instead of stopping dead
- [ ] **Jump buffering:** pressing jump 0.1s before landing makes you jump the moment you land

## Phase 1 — Engine gaps (foundation for the game)

- [ ] **1. Audio system** (`Engine.Audio` project, wraps SDL3 Mixer)
  - [ ] Load + play a sound effect; run and hear it
  - [ ] Loop background music
- [ ] **2. Sprite animation** (`Engine.Rendering/Animation.cs`)
  - [ ] `AnimationClip` (frames, fps, loop) + player component
  - [ ] System that advances frames; animate a walking player
- [ ] **3. Particles** (`Engine.Rendering/ParticleSystem.cs`)
  - [ ] Pooled particles with velocity/gravity/fade
  - [ ] Emit a burst on command; see it on screen
- [ ] **4. Camera feel** (`Camera.cs`)
  - [ ] Dead-zone follow
  - [ ] `AddShake(...)`; shake when you trigger it
- [ ] **5. Input actions** (`Engine.Core/InputActions.cs`)
  - [ ] Bind keys to `Move`/`Attack`/`Dash`; game code stops using raw keycodes

## Phase 2 — The top-down action game (new `Game/` project)

- [ ] **Player**
  - [ ] 8-direction movement with acceleration + friction
  - [ ] Melee attack (arc hitbox in front, cooldown)
  - [ ] Dash/roll with invincibility frames
  - [ ] Health + knockback
- [ ] **Enemies**
  - [ ] Chaser AI (idle → chase → attack → hurt → dead)
  - [ ] Shooter AI (keeps distance, fires projectiles)
  - [ ] Hit flash + death burst particles
- [ ] **Combat feel**
  - [ ] Hit-stop (freeze frames on hit)
  - [ ] Screen shake on hits
  - [ ] Damage numbers
- [ ] **World**
  - [ ] Extend `.map` format: `EVENT spawn_enemy`, `spawn_item`, `player_start`, `level_exit`
  - [ ] `LevelLoader` reads spawn events into entities
  - [ ] Pickups: health + coins (via triggers)
- [ ] **HUD + screens**
  - [ ] Health bar, coin counter (Engine.UI)
  - [ ] Title, pause, game over, level-complete screens
- [ ] **Content**
  - [ ] One polished level (2-3 arenas, 30-60 min)
  - [ ] Audio + music wired in
  - [ ] Polish pass (the game feels *good*)

## Phase 3 — 3D foundation (after the game ships)

- [ ] `Engine.Math`: Vector3/Quaternion/Matrix4x4 helpers
- [ ] `Engine.Rendering3D` (OpenGL 4.1 + Silk.NET on an SDL3 window)
  - [ ] Triangle → textured quad → cube
  - [ ] Shader abstraction + perspective camera + Phong lighting
  - [ ] `.obj` mesh loader + `Transform3D` hierarchy
- [ ] `Demo3D` — a small 3D tech demo scene
- [ ] **Decide later:** port 2D rendering onto the 3D backend (unification — optional)

---

## Stretch (only after Phase 2 ships)

- Gamepad support
- Room transitions
- Boss fight
- 2-player
- Save system
