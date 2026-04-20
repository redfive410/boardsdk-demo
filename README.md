# Board SDK — Combat Demo

A two-player space combat game built with the [Board SDK](https://docs.dev.board.fun/) for the Board tabletop gaming platform.

## Gameplay

- Two players each place a physical Board piece on the table to control their ship
- Each ship has a **shield ring** that absorbs 3 bullet hits before breaking
- Once the shield is down, 1 direct hit destroys the ship — **game over**
- Press the **Fire** button on your dashboard to shoot
- Use the **intensity slider** to control bullet speed

## Scene Setup

### GameObjects
| GameObject | Components |
|---|---|
| Ship1 / Ship2 | `ShipController`, `ShipHealth`, `CircleCollider2D` (Is Trigger, tag: Ship) |
| ShieldRing (child of ship) | `ShieldRingRenderer`, `CircleCollider2D` (Is Trigger, tag: Shield) |
| Bullet (prefab) | `Bullet`, `CircleCollider2D` (Is Trigger), `Rigidbody2D` (Kinematic) |
| ShipManager | `ShipManager` — assign Ship1 and Ship2 in the Ships array |
| GameManager | `GameManager` — assign GameOverPanel and GameOverText |
| Canvas | Dashboard UI for each player (Fire button, intensity slider) |

### Tags Required
- `Ship`
- `Shield`

## Scripts

| Script | Purpose |
|---|---|
| `ShipManager` | Assigns the first two detected Board contacts to each ship |
| `ShipController` | Moves and rotates a ship based on its assigned contact |
| `ShipHealth` | Tracks shield hits (3) and ship hits (1), triggers game over |
| `ShieldRingRenderer` | Draws a circle ring using LineRenderer |
| `Bullet` | Moves bullet, handles collision with shields and ships |
| `PlayerDashboard` | Reads fire button and bullet speed slider input |
| `GameManager` | Shows game over screen, handles restart |

## Building

Tested on Android (arm64), Unity 6000.0.73f1, Board SDK 3.3.0.

```
bdb install board_demo_v1.apk
bdb launch com.a51.board_demo
```
