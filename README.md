# Psi-Kid

A 2D telekinesis puzzle-platformer made in Unity. You play as a child with psychic powers, locked in a lab, solving room-by-room puzzles by moving objects with her mind.

**Play it:** https://nikolasraposo.itch.io/psi-kid
**Trailer:** https://youtu.be/s2tMrXUq2v0

`Unity 2022.3.48f1 LTS` · `C#` · `License: MIT`

## About

Psi-Kid is a solo learning project I built at FIAP (Game Development). Every production stage was reviewed by a faculty reviewer, and the project earned top marks across game programming, game design, UX and visual interface, 3D modeling and animation, drawing, and writing. The scope and visuals are intentionally small. The focus was the core telekinesis mechanic and the puzzle systems.

## Gameplay

- Telekinesis: select a loose object and move it freely to solve the room.
- Pressure-plate puzzles with different rules: some need a minimum weight, others only trigger while another plate is held down.
- Multi-floor challenge rooms, each with an entry and an exit door.

### Controls

| Action | Input |
| --- | --- |
| Move | WASD |
| Jump | Space |
| Telekinesis (select and move an object) | Hold left mouse button and drag |
| Use elevator / interact | Shift |
| Pause | Esc |

## Tech

- Engine: Unity 2022.3.48f1 (LTS)
- Language: C#
- Input: Unity Input System
- Structure: gameplay split into focused scripts (object manipulation, pressure plates, elevators), the player split into controller and input handler, plus level timer, scene loader, pause and camera managers.

## Project structure

```
Assets/
  Scripts/
    Gameplay/      ObjectController, PressurePlate, Elevator, Player (Controller/Input/State)
    Input System/  PlayerInputSystem
    Systems/       GameManager, GameController, PauseManager, CameraController
    Utilities/     MainMenuController, EndGame, ElevatorController, ChangeCam
  Scenes/          Main Menu, Fase 01 (level), prototypes
  Sprites/  Animations/  Audio/  Tiles/  Prefabs/  UI/  Materials/
```

## Running locally

1. Clone the repo.
2. Open it with Unity 2022.3.48f1 (or a close 2022.3 LTS version).
3. Open `Assets/Scenes/MainMenu/Main Menu.unity` and press Play.

Or just play the build on itch.io (link above).

## Credits and notes

Solo project by Nikolas Raposo (FIAP, 2024).

- Original art (the main character, the movable box and the pressure plates): Nikolas Raposo.
- Environment tiles: Free Pixel Art Tiles by TotusLotus, used under the author's license (https://totuslotus.itch.io/free-pixel-art-tiles).
- Part of the code was developed with AI assistance.

## License

MIT. See [LICENSE](LICENSE).
