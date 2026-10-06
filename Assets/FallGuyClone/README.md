# Bean Dash – Fall Guys style obstacle course

Dodge the obstacles and reach the FINISH line before the timer (2:30) runs out.

## How to play in the Editor
1. Open the project in Unity 6000.3. On first compile the editor script creates
   `Assets/Scenes/FallGuyCourse.unity` and opens it (menu: **Fall Guy Clone > Create Game Scene**).
2. Press **Play**, click **PLAY** (or press Enter).

## Controls
| Input | Action |
|---|---|
| W A S D / Arrows | Move (relative to the camera) |
| Mouse | Orbit the third person camera |
| Mouse wheel | Zoom |
| Space | Jump |
| Left Mouse / Left Ctrl / E | Dive |
| R | Respawn at last checkpoint |
| Esc / P | Pause |
| Q / E (menu) | Change character |

## Course
1. Sweeper arena: spinning bars, jump over them. Spring pads launch you onto the bridge.
2. Wrecking-ball bridge: swinging pendulums knock you off.
3. Sliding platforms over a gap.
4. Barrel ramp: barrels roll down at you.
5. Falling tiles: tiles drop shortly after you step on them.
6. Punching walls, then the finish line.

Falling into the pink goo sends you back to the last checkpoint.

## Code (Assets/FallGuyClone/Scripts)
- `FallGuyGame` – entry point; sets up lighting, course, player, camera, UI.
- `CourseBuilder` – builds the whole level in code (edit it to change the layout).
- `PlayerController` – Rigidbody movement, jump, dive, knock-back, moving platform support.
- `PlayerAvatar` – Kenney character + animations via the Playables API (bean fallback).
- `ThirdPersonCamera` – mouse orbit camera with collision.
- `GameManager` / `GameUI` – timer, checkpoints, qualify/eliminate, menus and HUD.
- `Obstacles/` – Rotator, Pendulum, Oscillator, FallingTile, BarrelSpawner, Hazard, triggers.

## Free assets
3D models and animations: **Platformer Kit by Kenney** (www.kenney.nl), license CC0.
Stored in `Assets/Resources/Kenney` (see `Kenney-License.txt`).
If a model is missing the game falls back to Unity primitives.
