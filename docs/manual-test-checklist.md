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
