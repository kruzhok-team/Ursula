# Ursula Project (Godot Engine / C#)

[Русская версия](README.md)

## Description

Ursula is a system for creating interactive 3D worlds using the Godot Engine with C# scripting. The project focuses on modular interactive objects whose behavior is defined using Hierarchical State Machines (HSM) specified in the GraphML format.

## Key Features

*   **Engine:** Godot Engine (C#).
*   **Interactive Object System:** Central `InteractiveObject` component with pluggable modules.
*   **Behavior Management:** Utilizes HSM (GraphML-compatible) for describing complex object logic.
*   **Modular Architecture:** Functionality is divided into modules (movement, audio, detection, timers, counters, animation).
*   **HSM-Object Bridging:** Implemented via an event/command bus (`LocalBus`) and specialized extension modules (`HSM*Module.cs`).
*   **Dynamic Loading:** Supports runtime loading of 3D models (.obj, .glb) and audio (.wav, .mp3, .ogg).
*   **Object Catalog:** System for managing a library of game assets and their metadata (JSON-based).
*   **Assets:** Includes a collection of 3D models (animals, buildings, plants, props), animations, and materials.
*   **Project Management:** Functionality for creating, loading, saving, and exporting game projects.
*   **Dependency Injection:** Used for managing dependencies between system components.

## Technology Stack

*   Godot Engine 4+
*   C# (.NET)
*   GraphML (for HSM definitions)
*   JSON (for object catalog and settings)

## Project Structure (Core Directories)

*   `Assets/`: 3D models, animations, materials, sounds, prefabs.
*   `Modules/`: Key project subsystems (Interactive Objects, HSM (Cyberiada), Resource Loaders, Models Catalog).
*   `Scripts/`: C# scripts implementing component logic and systems.
*   `Prefabs/`: Ready-to-use object and UI scenes.

## How it Works

1.  **Interactive Objects (`InteractiveObject`)** are the primary entities within a scene.
2.  Their behavior logic is loaded from `.graphml` files and interpreted as a **Hierarchical State Machine (HSM)** using `CyberiadaLogic` and `CyberiadaHSMConverter`.
3.  **HSM Extension Modules (`HSM*Module.cs`)** act as a bridge, translating HSM commands and events into method calls on the object's specific components (e.g., `InteractiveObjectMove`, `InteractiveObjectAudio`) via the **event/command bus (`LocalBus`)**.
4.  Physical **movement** is handled by `MoveScript`, utilizing `NavigationAgent3D` and `CharacterBody3D`.
5.  **Animations** are controlled by `AnimationPlayer` through scripts like `BaseAnimation`.
6.  The **Object Catalog** and **Resource Loaders** allow for dynamic asset management and loading.
7.  The **DI system** manages the creation and wiring of dependencies between modules.

## Getting Started

1.  Clone the repository: `git clone https://github.com/kruzhok-team/Ursula.git`
2.  Open the project in Godot Engine 4+.
3.  (Optional) Perform any necessary setup steps.

## License

MIT
