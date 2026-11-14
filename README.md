# Multiplayer Drifting Game

Portfolio source snapshot of a Unity driving and drifting project with multiplayer-oriented systems, race construction, progression, customization, and reusable game infrastructure.

Подробный стек и архитектурные решения: [TECHNOLOGIES.md](TECHNOLOGIES.md).

## Highlights

- Race configuration, waypoint progress, difficulty and control presets, and runtime race construction.
- Vehicle spawning and control services, driver animation targets, steering-wheel integration, and drift-oriented presentation.
- Save/load infrastructure, pooling, pub/sub events, scene transitions, screen management, and application state machines.
- Vehicle customization including paint, rims, neon, smoke colors, and garage UI.
- Player progression, XP, health, pickups, drone mode, and additional gameplay prototypes.

## Source layout

- `Assets/Scripts` — primary gameplay architecture and systems.
- `Assets/Script` and `Assets/_Scripts` — supporting gameplay and menu code.
- `Assets/ScriptableObjects` — race and configuration data.
- `Assets/Scenes`, `Assets/Prefabs`, and `Assets/Resources` — lightweight project structure retained for context.
- `Packages` and `ProjectSettings` — Unity dependency and project configuration.

## Portfolio note

Licensed cars, tracks, art packs, SDK copies, baked lighting, and other large third-party assets are excluded from this public repository. It is intended for code review and architectural demonstration, not as a complete redistributable game build.
