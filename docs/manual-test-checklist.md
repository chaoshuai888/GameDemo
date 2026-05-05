# Manual Test Checklist

Use Unity / 团结引擎 1.8.5 to create the first playable scene and verify the prototype loop.

## Editor Setup

- Create `Assets/_Project/Scenes/Main.unity`.
- Create original placeholder sprites for plants, enemies, projectile, sun, grid, and UI.
- Create prefabs:
  - Shooter plant with `Plant`, `PlantHealth`, `PlantAttackController`.
  - Producer plant with `Plant`, `PlantHealth`, `SunProducer`.
  - Blocker plant with `Plant`, `PlantHealth`.
  - Normal enemy with `Enemy`, `EnemyHealth`, `EnemyMovement`, `EnemyAttackController`.
  - Heavy enemy with `Enemy`, `EnemyHealth`, `EnemyMovement`, `EnemyAttackController`.
  - Projectile with `Projectile`.
  - Sun collectible with `SunCollectible`.
- Create config assets:
  - Three `PlantConfig` assets with original names.
  - Two `EnemyConfig` assets with original names.
  - One `ProjectileConfig`.
  - One `WaveConfig` with at least eight total enemies.
  - One `LevelConfig` using 5 rows, 9 columns, and 50 initial sun.
- Create scene objects:
  - `GameBootstrap`
  - `GameStateController`
  - `PoolManager`
  - `GridSystem`
  - `SunSystem`
  - `PlantPlacementSystem`
  - `LaneTargetService`
  - `WaveSystem`
  - `CameraFitController`
  - Canvas with `GameHudView`, `PlantCardView`, and `ResultView`
- Wire all serialized references in the Inspector.

## Play Mode Checks

- Augment choice appears before combat when the level has `AvailableAugments`.
- Exactly three augment cards are offered when at least three augments are configured.
- Selecting one augment hides the augment panel and starts waves.
- `First Light` increases initial sun.
- `Rapid Germination` shortens plant card cooldowns.
- `Sharp Seeds` increases projectile damage.
- `Golden Drip` shortens natural sun timing.
- `Bounty Moss` grants sun after enemy deaths.
- Initial sun appears in the HUD.
- A plant can be selected and placed on an empty grid cell.
- Sun cannot go negative.
- Card cooldown blocks immediate repeat placement.
- Natural sun appears and can be collected.
- Producer plant creates sun.
- Enemies spawn by wave and move left in lanes.
- Shooter plant fires at same-lane enemies.
- Projectile hits reduce enemy health.
- Enemy dies at zero health.
- Enemy stops and attacks a blocking plant.
- Plant death releases its grid cell.
- Enemy reaching the left boundary triggers defeat.
- Clearing all waves triggers victory.
- Console has no new compile errors, missing references, or runtime exceptions.

## Expanded Content Checks

- Run `LawnDefense/Build Expanded Content` from the editor menu.
- Confirm `ExpandedPrototypeLevel` exists under `Assets/_Project/Data/Levels`.
- Confirm 10 augment configs exist under `Assets/_Project/Data/Augments`.
- Confirm expanded plant configs exist for `Mist Sprout`, `Thorn Pod`, `Bloom Battery`, `Bark Bastion`, and `Spore Mine`.
- Confirm expanded enemy configs exist for `Moss Skitter`, `Shellback Shambler`, `Bloom Carrier`, and `Rot Howler`.
- `Mist Sprout` slows enemies on hit.
- `Thorn Pod` can pierce more than one enemy.
- `Bloom Battery` produces larger but slower sun drops.
- `Bark Bastion` survives noticeably longer than `Stoneleaf`.
- `Spore Mine` explodes once and removes itself.
- `Moss Skitter` moves faster than `Moss Walker`.
- `Shellback Shambler` reduces incoming damage through armor.
- `Bloom Carrier` grants extra sun on death.
- `Rot Howler` applies a temporary same-lane speed boost.
