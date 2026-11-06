# Enchanted Forest Runner — Setup Guide

All the game logic lives in `Assets/Scripts/` (9 files, namespace `EnchantedForest`).
One of them, **`Bootstrap.cs`**, is the only thing you need to add to the scene —
it builds the runner, world, camera, HUD, audio, and FX from code. So setting up
the game is just a few clicks.

The project is already configured the way the code expects:
- URP (Universal Render Pipeline) — built into the blank URP template
- New Input System (`activeInputHandler: 1`) — keyboard input uses `Keyboard.current`

You don't need to add any art, audio, or model files. Everything is generated at runtime.

---

## 1. Open the project

1. Open **Unity Hub** → select the **enchnted** project (version 6000.3.22f1).
   If it's not listed, click **+ / Add**, browse to this folder, and pick the
   `ProjectSettings` folder.
2. Let Unity import. When it opens, make sure you're looking at
   **Assets/Scenes/SampleScene**.unity** in the Hierarchy (it's already the
   default open scene).

## 2. Add the Bootstrap

1. In the **Hierarchy** panel, right-click → **Create Empty**.
2. Rename it to `Forest Runner Boot` (any name is fine).
3. With it selected, in the **Inspector** click **Add Component** and search
   `Bootstrap`.
   - It's in the `EnchantedForest` namespace, so type **Bootstrap** and pick the
     one whose full name is `EnchantedForest.Bootstrap`.
4. Leave all the Inspector fields blank — defaults are tuned.

That's the whole scene. `Bootstrap` creates:
- the runner (capsule body + glowing mask + ears) with a rune trail
- the camera rig
- ground, trees, crystal clusters, fallen-root obstacles, and fireflies ahead of you
- the HUD canvas, the post-processing (bloom + vignette), and the procedural audio

## 3. Confirm a Main Camera exists

The default URP scene already contains a **Main Camera** (tagged `MainCamera`).
`Bootstrap` reuses it and adds the `CameraRig` to it. You can verify by selecting
the camera in the Hierarchy and checking its **Tag** is `MainCamera`.

> If you deleted it, create a new **Camera** (right-click Hierarchy →
> Create Empty → Add Component → **Camera**) and set its Tag to `MainCamera`.

## 4. Run it

1. Press the big **Play** button (top-center).
2. You'll see the **ENCHANTED FOREST RUNNER** title card.
3. Press **Space** (or click) to start.

### Controls
| Input | Action |
|---|---|
| `←` / `→` or `A` / `D` | Change lane |
| `Space` / `↑` | Jump |
| `R` | Restart (on the game-over screen) |

## 5. (Optional) Restart during development

To start over without reloading the scene, press **R** on the game-over card, or
click **RUN AGAIN**. Both do the same thing.

---

## If it doesn't compile (red errors in the Console)

The code is written for Unity 6000.3.22f1 + URP. If you see errors, the usual
causes are:

- **`UnityEditor` / `Render` settings**: confirm the project is actually URP
  (Project Settings → Quality → each Quality level's *Render Pipeline* is a URP
  asset). If it's HDRP or Built-in, `Shader.Find("Universal Render Pipeline/Lit")`
  and the `Volume` post-FX won't work.
- **Input System**: Project Settings → **Player** → *Active Input Handling* should
  be **Input System Package (New)**. It already is in this project (`activeInputHandler: 1`).
- **Namespace mismatch**: every script is `EnchantedForest`. If you renamed a
  script or removed the `namespace` line, the references break.

## File map

| Script | Responsibility |
|---|---|
| `Bootstrap.cs` | Builds the whole world + drives the run loop (the one component you add) |
| `GameManager.cs` | Score, lives, distance, run state, combo, invulnerability (event-based singleton) |
| `RunnerController.cs` | Lane / jump / forward movement + collisions + the hit-blink |
| `CameraRig.cs` | Smooth follow + look-ahead + impact shake |
| `Entities.cs` | `Obstacle` marker + `FireflyPickup` trigger/spin/bob |
| `GameHUD.cs` | Canvas HUD: distance, hearts, title card, game-over card + retry |
| `ProceduralAudio.cs` | Runtime-generated hum, collect, and hit sounds (no audio files) |
| `WorldFX.cs` | Particle bursts (collect / impact) + drifting ambient spores |
| `WorldFactory.cs` | Creates all materials, primitive shapes, and the URP post-FX volume |
