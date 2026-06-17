# Umwami's Ascent

A 2D narrative platformer about the founding of Rwanda. You play as the **Umwami** (the King), ascending through a corrupted land to confront the **Shaman** — a dark figure drawn from African oral tradition — reclaim the land, and found the Rwandan nation.

> **Module:** Mission-Based Interactive Simulation
> **Engine:** Unity 6 (6000.x) · Universal Render Pipeline (2D) · **New Input System only**

---

## GCGO Statement

**Grand Challenge & Global Opportunity: Art & Culture.**

> *Our mission is to use interactive technology to preserve and share African cultural heritage — the stories, myths, and founding histories that are too often lost — so a new generation can experience and value them.*

*Umwami's Ascent* turns the founding myth of Rwanda into a playable experience, keeping an oral-tradition story alive through interactive media.

## Problem Context

- **The problem:** Much of Africa's history and mythology lives in **oral tradition** and is fading as generations pass. Founding stories — like that of the Rwandan kingdom — are rarely presented in engaging, interactive formats that young people connect with today.
- **Why it matters:** Cultural identity and pride are anchored in shared stories. Losing them erodes heritage and the sense of where a people come from.
- **How it connects to the GCGO (Art & Culture):** This simulation reframes a cultural founding narrative as an interactive journey. By *playing* the King's ascent to free the land from the Shaman and found the nation, the player engages with the story actively rather than passively — a step toward preserving and popularising cultural heritage through technology.

## Simulation Overview

- **What it does:** Guides the player through a platforming journey as the King. The "ancestral path" (a glowing guide line) leads toward the throne. The way is blocked by danger and ultimately by the **Shaman boss**, who must be defeated before the throne (the founding of the nation) can be claimed.
- **Target users:** Students and young people interested in African history and culture; anyone exploring cultural-heritage games.
- **Key interactions:**
  - Move, jump, and dash across the land.
  - Avoid **danger zones** that drain your health.
  - **Attack** the Shaman with the spirit-spear.
  - Defeat the Shaman to **unlock the throne**, then reach it to win.

## Controls

| Action | Keyboard | Mobile |
|---|---|---|
| Move | A / D or ← / → | on-screen **◀ ▶** |
| Jump | Space | on-screen **JUMP** |
| Attack | J or Left-Click | on-screen **ATK** |
| Dash | Left Shift | — |
| Toggle ancestral path | Tab | — |

On touch devices the on-screen controls appear automatically; on desktop they are hidden.

## Unity Mechanics Implemented

- **UI:** Main Menu (Play / Settings / Quit), a hideable **Settings** panel with music-volume sliders, an in-game **HUD** showing health as a percentage (`Health: NN%`), **Win** and **Lose** screens, and **on-screen mobile controls** — all built with the UI Canvas system and TextMeshPro.
- **Scripting:** All gameplay is custom C# (`Assets/scripts/`): player movement, health/regeneration, combat, scene flow, persistent music, and the danger/win/boss trigger zones.
- **Collision:** Solid collisions with the ground tilemap; **trigger** colliders for the danger zones, the boss-fight zone, the win zone (the throne), and player↔boss contact damage. *(A collider blocks/stands on; a trigger detects overlap without blocking — see `DangerZone`, `WinZone`, `BossFightZone`.)*
- **Raycasting:** The player's **slash** attack (`PlayerAttack`) fires a `Physics2D.Raycast` in the facing direction to detect and damage the Shaman.
- **Line Renderer:** The **Ancestral Path** (`AncestralPath`) draws a glowing line from the King to the throne as cultural wayfinding (toggle with Tab); the attack also draws a brief slash line.

## Additional Features (beyond the module)

1. **Audio system** — a persistent `MusicManager` plays a lobby track everywhere and crossfades to a bossfight track when the Shaman is engaged, with independent volume controls.
2. **Data persistence** — music-volume settings are saved with `PlayerPrefs` and restored across sessions.
3. **Sprite animation** — Animator-driven idle / run / slash / death states.
4. **On-screen mobile controls** — touch buttons via the Input System's **On-Screen Controls**, auto-shown only on touch devices.
5. **Health regeneration** — health slowly recovers after leaving danger.

## Build Information

- **WebGL deployment:** https://play.unity.com/en/games/aff67c3f-3626-4a73-a472-46c09a0adbcc/umwamis-ascend
- **Android build (APK):** https://drive.google.com/drive/folders/1_X-A3TR4_d0WQdNVsjJld6wr1qmH2cRB?usp=sharing
- **GitHub repository:** https://github.com/Strixx-thierry/Umwami-s-Ascent
- **Demo video:** _[add link here]_

### Running the project locally
1. Open the project in **Unity 6 (6000.x)** with the **2D / URP** setup.
2. Open `Assets/Scenes/MainMenu.unity`.
3. Press **Play**, or build via **File ▸ Build Settings** (scenes are already registered in order: MainMenu, Gameplay, Win, Lose).

### WebGL
- File ▸ Build Settings ▸ switch platform to **WebGL** ▸ Build, then host the output (itch.io, Unity Play, or GitHub Pages).

### Android
- Switch platform to **Android**, set a package name, and **Build** the APK. On-screen touch controls appear automatically on the device.

---

*Built with Unity 6 and the new Input System.*
