# Board SDK — Combat Demo

A two-player space combat game built with the [Board SDK](https://docs.dev.board.fun/) for the Board tabletop gaming platform.

## Gameplay

- Two players each place a physical Board piece on the table to control their ship
- Each ship has a **shield ring** that absorbs 3 bullet hits before breaking
- Once the shield is down, 1 direct hit destroys the ship — **game over**
- Press the **Fire** button on your dashboard to shoot
- Use the **intensity slider** to control bullet speed
- Sound effects play on firing, bullet impacts, and ship destruction

## Scene Setup

### GameObjects
| GameObject | Components |
|---|---|
| Ship1 / Ship2 | `ShipController`, `ShipHealth`, `CircleCollider2D` (Is Trigger, tag: Ship) — assign `fire.wav` to Fire Sound |
| ShieldRing (child of ship) | `ShieldRingRenderer`, `CircleCollider2D` (Is Trigger, tag: Shield) |
| Bullet (prefab) | `Bullet`, `CircleCollider2D` (Is Trigger), `Rigidbody2D` (Kinematic) — assign `impact.wav` to Impact Sound |
| Impact (prefab) | `ImpactEffect`, `SpriteRenderer` — sprite-sheet hit animation spawned on collision |
| ShipManager | `ShipManager` — assign Ship1 and Ship2 in the Ships array |
| GameManager | `GameManager` — assign GameOverPanel, GameOverText, and `explosion.wav` to Explosion Sound |
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
| `Bullet` | Moves bullet, handles collision with shields and ships, plays impact sound |
| `ImpactEffect` | Plays a one-shot sprite-frame impact animation, then self-destructs |
| `PlayerDashboard` | Reads fire button and bullet speed slider input |
| `GameManager` | Shows game over screen, plays explosion sound, handles restart |

## Audio

Sound effects live in `Assets/Combat/Audio/` as `.wav` files and are wired to the scripts
via Inspector slots (see the GameObjects table above). They were generated with
[sfxr.me](https://sfxr.me/), an online 8-bit sound effect generator.

| File | Played by | Trigger |
|---|---|---|
| `fire.wav` | `ShipController` | Each time a bullet is fired |
| `impact.wav` | `Bullet` | Bullet hits a shield or ship |
| `explosion.wav` | `GameManager` | A ship is destroyed |

Playback uses `AudioSource.PlayClipAtPoint`, which spawns a temporary one-shot source so
the sound survives the object being destroyed on the same frame.

### Regenerating / adding sounds

1. Design a sound at [sfxr.me](https://sfxr.me/), then click **Serialize** to copy its JSON.
2. Convert it to a WAV on the command line with the `sfxr-to-wav` script from the
   [`jsfxr`](https://www.npmjs.com/package/jsfxr) npm package:

   ```sh
   # paste the serialized JSON into a file, then:
   cat my-sound.sfxr.json | npx jsfxr Assets/Combat/Audio/my-sound.wav
   ```

   (You can also pass the b58 string from the sfxr.me share URL directly:
   `npx jsfxr <b58-string> Assets/Combat/Audio/my-sound.wav`.)
3. In Unity, set the clip's **Load Type: Decompress On Load** and **Compression: PCM**
   for zero-latency playback, then drag it into the relevant Inspector slot.

Use **WAV** for short SFX (instant, sample-accurate); reserve OGG for long music/ambience.

## Building

Tested on Android (arm64), Unity 6000.0.73f1, Board SDK 3.3.0.

```
bdb install board_demo_v4.apk
bdb launch com.a51.board_demo
```
