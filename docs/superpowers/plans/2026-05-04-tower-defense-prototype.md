# Tower Defense Prototype Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first playable 2D lane-based tower-defense prototype source foundation for the approved plant-style design.

**Architecture:** Use MonoBehaviour for scene lifecycle and editor wiring, ScriptableObject for gameplay data, plain C# structs/enums for small rules data, a lightweight event hub, and prefab-based object pooling. The current repository is not yet a Unity project, so this plan first creates the `Assets/_Project` source tree and scripts; scene, prefab, and `.asset` authoring must be completed in Unity editor 1.8.5 after scripts import successfully.

**Tech Stack:** Unity-compatible C#, Unity UI/uGUI, ScriptableObject configs, no DOTS, no ECS package, no third-party combat/UI/camera framework.

---

## File Map

Create these source directories:

- `Assets/_Project/Scripts/Core/`: game state, bootstrap, event hub, pool.
- `Assets/_Project/Scripts/Data/`: ScriptableObject configs and serializable wave entries.
- `Assets/_Project/Scripts/Grid/`: grid coordinates, cell state, world/grid conversion.
- `Assets/_Project/Scripts/Sun/`: sun wallet, falling/collectible sun, natural sun spawner.
- `Assets/_Project/Scripts/Placement/`: plant card runtime state and placement system.
- `Assets/_Project/Scripts/Plants/`: plant root, health, attack, sun production.
- `Assets/_Project/Scripts/Enemies/`: enemy root, health, movement, attack.
- `Assets/_Project/Scripts/Combat/`: damage data, projectile, lane target queries.
- `Assets/_Project/Scripts/Waves/`: wave scheduler and lane spawning.
- `Assets/_Project/Scripts/UI/`: HUD and plant card views.
- `Assets/_Project/Scripts/Camera/`: fixed orthographic camera fitting.

Create these Unity asset directories for editor-authored content:

- `Assets/_Project/Scenes/`
- `Assets/_Project/Prefabs/Plants/`
- `Assets/_Project/Prefabs/Enemies/`
- `Assets/_Project/Prefabs/Projectiles/`
- `Assets/_Project/Prefabs/Grid/`
- `Assets/_Project/Prefabs/UI/`
- `Assets/_Project/Art/Placeholders/`
- `Assets/_Project/Audio/`
- `Assets/_Project/Data/Plants/`
- `Assets/_Project/Data/Enemies/`
- `Assets/_Project/Data/Projectiles/`
- `Assets/_Project/Data/Levels/`
- `Assets/_Project/Data/Waves/`
- `Assets/_Project/UI/`
- `Assets/ThirdParty/`

## Task 1: Unity Project Skeleton

**Files:**
- Create directories listed in File Map.
- Create: `Assets/_Project/Scripts/Core/GameState.cs`
- Create: `Assets/_Project/Scripts/Core/GameEvents.cs`

- [ ] **Step 1: Create directories**

Run:

```powershell
New-Item -ItemType Directory -Force -Path `
  'Assets/_Project/Scripts/Core', `
  'Assets/_Project/Scripts/Data', `
  'Assets/_Project/Scripts/Grid', `
  'Assets/_Project/Scripts/Sun', `
  'Assets/_Project/Scripts/Placement', `
  'Assets/_Project/Scripts/Plants', `
  'Assets/_Project/Scripts/Enemies', `
  'Assets/_Project/Scripts/Combat', `
  'Assets/_Project/Scripts/Waves', `
  'Assets/_Project/Scripts/UI', `
  'Assets/_Project/Scripts/Camera', `
  'Assets/_Project/Scenes', `
  'Assets/_Project/Prefabs/Plants', `
  'Assets/_Project/Prefabs/Enemies', `
  'Assets/_Project/Prefabs/Projectiles', `
  'Assets/_Project/Prefabs/Grid', `
  'Assets/_Project/Prefabs/UI', `
  'Assets/_Project/Art/Placeholders', `
  'Assets/_Project/Audio', `
  'Assets/_Project/Data/Plants', `
  'Assets/_Project/Data/Enemies', `
  'Assets/_Project/Data/Projectiles', `
  'Assets/_Project/Data/Levels', `
  'Assets/_Project/Data/Waves', `
  'Assets/_Project/UI', `
  'Assets/ThirdParty'
```

Expected: all directories exist.

- [ ] **Step 2: Add game state enum**

Create `Assets/_Project/Scripts/Core/GameState.cs`:

```csharp
namespace LawnDefense.Core
{
    public enum GameState
    {
        Preparing,
        Playing,
        Paused,
        Victory,
        Defeat
    }
}
```

- [ ] **Step 3: Add event hub**

Create `Assets/_Project/Scripts/Core/GameEvents.cs`:

```csharp
using System;

namespace LawnDefense.Core
{
    public static class GameEvents
    {
        public static event Action<int> SunChanged;
        public static event Action<GameState> GameStateChanged;
        public static event Action<float> WaveProgressChanged;
        public static event Action<string, float> PlantCardCooldownChanged;

        public static void RaiseSunChanged(int value) => SunChanged?.Invoke(value);
        public static void RaiseGameStateChanged(GameState state) => GameStateChanged?.Invoke(state);
        public static void RaiseWaveProgressChanged(float progress) => WaveProgressChanged?.Invoke(progress);
        public static void RaisePlantCardCooldownChanged(string plantId, float normalizedRemaining) =>
            PlantCardCooldownChanged?.Invoke(plantId, normalizedRemaining);
    }
}
```

- [ ] **Step 4: Verify files**

Run:

```powershell
Get-ChildItem -Recurse Assets/_Project/Scripts | Select-Object FullName
```

Expected: `GameState.cs` and `GameEvents.cs` are listed.

- [ ] **Step 5: Commit**

```bash
git add Assets docs/superpowers/plans/2026-05-04-tower-defense-prototype.md
git commit -m "feat: add Unity prototype source skeleton"
```

## Task 2: ScriptableObject Data Model

**Files:**
- Create: `Assets/_Project/Scripts/Data/PlantRole.cs`
- Create: `Assets/_Project/Scripts/Data/DamageType.cs`
- Create: `Assets/_Project/Scripts/Data/LaneMode.cs`
- Create: `Assets/_Project/Scripts/Data/PlantConfig.cs`
- Create: `Assets/_Project/Scripts/Data/EnemyConfig.cs`
- Create: `Assets/_Project/Scripts/Data/ProjectileConfig.cs`
- Create: `Assets/_Project/Scripts/Data/WaveConfig.cs`
- Create: `Assets/_Project/Scripts/Data/LevelConfig.cs`

- [ ] **Step 1: Add enums**

Create `PlantRole.cs`:

```csharp
namespace LawnDefense.Data
{
    public enum PlantRole
    {
        Shooter,
        Producer,
        Blocker
    }
}
```

Create `DamageType.cs`:

```csharp
namespace LawnDefense.Data
{
    public enum DamageType
    {
        Normal
    }
}
```

Create `LaneMode.cs`:

```csharp
namespace LawnDefense.Data
{
    public enum LaneMode
    {
        Specific,
        RandomAny,
        RandomFromAllowed
    }
}
```

- [ ] **Step 2: Add projectile config**

Create `ProjectileConfig.cs`:

```csharp
using UnityEngine;

namespace LawnDefense.Data
{
    [CreateAssetMenu(menuName = "LawnDefense/Projectile Config")]
    public sealed class ProjectileConfig : ScriptableObject
    {
        public string Id;
        public GameObject Prefab;
        public float Speed = 6f;
        public int Damage = 20;
        public float HitRadius = 0.2f;
        public bool CanPierce;
        public int MaxPierceCount;
        public GameObject HitEffectPrefab;
    }
}
```

- [ ] **Step 3: Add plant and enemy configs**

Create `PlantConfig.cs`:

```csharp
using UnityEngine;

namespace LawnDefense.Data
{
    [CreateAssetMenu(menuName = "LawnDefense/Plant Config")]
    public sealed class PlantConfig : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Sprite Icon;
        public GameObject Prefab;
        public int SunCost = 50;
        public float Cooldown = 5f;
        public int MaxHealth = 100;
        public PlantRole Role;
        public float AttackInterval = 1.5f;
        public float AttackRange = 9f;
        public ProjectileConfig ProjectileConfig;
        public float SunProduceInterval = 8f;
        public int SunProduceAmount = 25;
    }
}
```

Create `EnemyConfig.cs`:

```csharp
using UnityEngine;

namespace LawnDefense.Data
{
    [CreateAssetMenu(menuName = "LawnDefense/Enemy Config")]
    public sealed class EnemyConfig : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public GameObject Prefab;
        public int MaxHealth = 100;
        public float MoveSpeed = 0.7f;
        public int AttackDamage = 20;
        public float AttackInterval = 1f;
        public int RewardSun;
        public string[] EnemyTags;
    }
}
```

- [ ] **Step 4: Add wave and level configs**

Create `WaveConfig.cs`:

```csharp
using System;
using UnityEngine;

namespace LawnDefense.Data
{
    [CreateAssetMenu(menuName = "LawnDefense/Wave Config")]
    public sealed class WaveConfig : ScriptableObject
    {
        public WaveEntry[] Entries;
    }

    [Serializable]
    public sealed class WaveEntry
    {
        public EnemyConfig EnemyConfig;
        public float SpawnTime;
        public int Count = 1;
        public float Interval = 1f;
        public LaneMode LaneMode = LaneMode.RandomAny;
        public int SpecificLane;
        public int[] AllowedLanes;
        public bool IsMajorWave;
    }
}
```

Create `LevelConfig.cs`:

```csharp
using UnityEngine;

namespace LawnDefense.Data
{
    [CreateAssetMenu(menuName = "LawnDefense/Level Config")]
    public sealed class LevelConfig : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public int Rows = 5;
        public int Columns = 9;
        public int InitialSun = 50;
        public float NaturalSunInterval = 7f;
        public int NaturalSunAmount = 25;
        public PlantConfig[] AvailablePlants;
        public WaveConfig WaveConfig;
    }
}
```

- [ ] **Step 5: Verify no forbidden IP names**

Run:

```powershell
Select-String -Path 'Assets/_Project/Scripts/**/*.cs' -Pattern 'Plants vs Zombies|PvZ|Peashooter|Sunflower|Wall-nut|Zombie' -SimpleMatch
```

Expected: no matches.

- [ ] **Step 6: Commit**

```bash
git add Assets/_Project/Scripts/Data
git commit -m "feat: add gameplay config assets"
```

## Task 3: Grid System

**Files:**
- Create: `Assets/_Project/Scripts/Grid/GridCoordinate.cs`
- Create: `Assets/_Project/Scripts/Grid/GridCell.cs`
- Create: `Assets/_Project/Scripts/Grid/GridSystem.cs`

- [ ] **Step 1: Add grid coordinate and cell**

Create `GridCoordinate.cs`:

```csharp
using System;

namespace LawnDefense.Grid
{
    [Serializable]
    public struct GridCoordinate
    {
        public int Row;
        public int Column;

        public GridCoordinate(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}
```

Create `GridCell.cs`:

```csharp
using LawnDefense.Plants;

namespace LawnDefense.Grid
{
    public sealed class GridCell
    {
        public GridCoordinate Coordinate { get; }
        public Plant Occupant { get; private set; }
        public bool IsOccupied => Occupant != null;

        public GridCell(GridCoordinate coordinate)
        {
            Coordinate = coordinate;
        }

        public bool TryOccupy(Plant plant)
        {
            if (IsOccupied || plant == null)
            {
                return false;
            }

            Occupant = plant;
            return true;
        }

        public void Clear(Plant plant)
        {
            if (Occupant == plant)
            {
                Occupant = null;
            }
        }
    }
}
```

- [ ] **Step 2: Add grid system**

Create `GridSystem.cs` with initialization, coordinate validation, occupancy, and world conversion:

```csharp
using LawnDefense.Data;
using LawnDefense.Plants;
using UnityEngine;

namespace LawnDefense.Grid
{
    public sealed class GridSystem : MonoBehaviour
    {
        [SerializeField] private Vector2 origin = new Vector2(-4f, -2f);
        [SerializeField] private Vector2 cellSize = new Vector2(1f, 1f);

        private GridCell[,] cells;

        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public Vector2 CellSize => cellSize;

        public void Initialize(LevelConfig levelConfig)
        {
            Rows = Mathf.Max(1, levelConfig.Rows);
            Columns = Mathf.Max(1, levelConfig.Columns);
            cells = new GridCell[Rows, Columns];

            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    cells[row, column] = new GridCell(new GridCoordinate(row, column));
                }
            }
        }

        public bool IsValid(GridCoordinate coordinate) =>
            coordinate.Row >= 0 && coordinate.Row < Rows &&
            coordinate.Column >= 0 && coordinate.Column < Columns;

        public bool TryGetCell(GridCoordinate coordinate, out GridCell cell)
        {
            if (!IsValid(coordinate) || cells == null)
            {
                cell = null;
                return false;
            }

            cell = cells[coordinate.Row, coordinate.Column];
            return true;
        }

        public bool TryOccupy(GridCoordinate coordinate, Plant plant)
        {
            return TryGetCell(coordinate, out GridCell cell) && cell.TryOccupy(plant);
        }

        public void ClearOccupant(GridCoordinate coordinate, Plant plant)
        {
            if (TryGetCell(coordinate, out GridCell cell))
            {
                cell.Clear(plant);
            }
        }

        public Vector3 GridToWorld(GridCoordinate coordinate)
        {
            return new Vector3(
                origin.x + coordinate.Column * cellSize.x,
                origin.y + coordinate.Row * cellSize.y,
                0f);
        }

        public bool TryWorldToGrid(Vector3 worldPosition, out GridCoordinate coordinate)
        {
            int column = Mathf.RoundToInt((worldPosition.x - origin.x) / cellSize.x);
            int row = Mathf.RoundToInt((worldPosition.y - origin.y) / cellSize.y);
            coordinate = new GridCoordinate(row, column);
            return IsValid(coordinate);
        }
    }
}
```

- [ ] **Step 3: Unity import check**

Open the project with Unity 1.8.5 and verify the scripts compile. Expected: no Console compile errors for `LawnDefense.Grid`.

- [ ] **Step 4: Commit**

```bash
git add Assets/_Project/Scripts/Grid
git commit -m "feat: add grid coordinate system"
```

## Task 4: Core Runtime Services

**Files:**
- Create: `Assets/_Project/Scripts/Core/IPoolable.cs`
- Create: `Assets/_Project/Scripts/Core/PoolManager.cs`
- Create: `Assets/_Project/Scripts/Core/GameStateController.cs`
- Create: `Assets/_Project/Scripts/Core/GameBootstrap.cs`

- [ ] **Step 1: Add pooling contract**

Create `IPoolable.cs`:

```csharp
namespace LawnDefense.Core
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
```

- [ ] **Step 2: Add pool manager**

Create `PoolManager.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace LawnDefense.Core
{
    public sealed class PoolManager : MonoBehaviour
    {
        private readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> prefabByInstance = new Dictionary<GameObject, GameObject>();

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                return null;
            }

            if (!pools.TryGetValue(prefab, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                pools[prefab] = queue;
            }

            GameObject instance = queue.Count > 0 ? queue.Dequeue() : Instantiate(prefab);
            prefabByInstance[instance] = prefab;
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);

            foreach (IPoolable poolable in instance.GetComponentsInChildren<IPoolable>(true))
            {
                poolable.OnSpawned();
            }

            return instance;
        }

        public void Despawn(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            foreach (IPoolable poolable in instance.GetComponentsInChildren<IPoolable>(true))
            {
                poolable.OnDespawned();
            }

            instance.SetActive(false);

            if (prefabByInstance.TryGetValue(instance, out GameObject prefab))
            {
                pools[prefab].Enqueue(instance);
            }
            else
            {
                Destroy(instance);
            }
        }
    }
}
```

- [ ] **Step 3: Add game state controller and bootstrap**

Create `GameStateController.cs`:

```csharp
using UnityEngine;

namespace LawnDefense.Core
{
    public sealed class GameStateController : MonoBehaviour
    {
        public GameState CurrentState { get; private set; } = GameState.Preparing;

        public void SetState(GameState state)
        {
            if (CurrentState == state)
            {
                return;
            }

            CurrentState = state;
            Time.timeScale = state == GameState.Paused ? 0f : 1f;
            GameEvents.RaiseGameStateChanged(state);
        }
    }
}
```

Create `GameBootstrap.cs`:

```csharp
using LawnDefense.Data;
using LawnDefense.Grid;
using LawnDefense.Sun;
using LawnDefense.Waves;
using UnityEngine;

namespace LawnDefense.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private GridSystem gridSystem;
        [SerializeField] private SunSystem sunSystem;
        [SerializeField] private WaveSystem waveSystem;
        [SerializeField] private GameStateController gameStateController;

        private void Start()
        {
            gridSystem.Initialize(levelConfig);
            sunSystem.Initialize(levelConfig);
            waveSystem.Initialize(levelConfig);
            gameStateController.SetState(GameState.Playing);
        }
    }
}
```

- [ ] **Step 4: Unity import check**

Open Unity and verify no compile errors. If `SunSystem` or `WaveSystem` does not exist yet, implement Task 5 and Task 9 before expecting full compilation.

- [ ] **Step 5: Commit after dependent services exist**

```bash
git add Assets/_Project/Scripts/Core
git commit -m "feat: add core runtime services"
```

## Task 5: Sun Economy

**Files:**
- Create: `Assets/_Project/Scripts/Sun/SunWallet.cs`
- Create: `Assets/_Project/Scripts/Sun/SunCollectible.cs`
- Create: `Assets/_Project/Scripts/Sun/SunSystem.cs`

- [ ] **Step 1: Add sun wallet**

Create `SunWallet.cs`:

```csharp
using LawnDefense.Core;

namespace LawnDefense.Sun
{
    public sealed class SunWallet
    {
        public int Current { get; private set; }

        public void Set(int value)
        {
            Current = value < 0 ? 0 : value;
            GameEvents.RaiseSunChanged(Current);
        }

        public bool CanSpend(int amount) => Current >= amount;

        public bool TrySpend(int amount)
        {
            if (!CanSpend(amount))
            {
                return false;
            }

            Set(Current - amount);
            return true;
        }

        public void Add(int amount)
        {
            Set(Current + amount);
        }
    }
}
```

- [ ] **Step 2: Add collectible and system**

Create `SunCollectible.cs`:

```csharp
using LawnDefense.Core;
using UnityEngine;

namespace LawnDefense.Sun
{
    public sealed class SunCollectible : MonoBehaviour, IPoolable
    {
        [SerializeField] private float fallSpeed = 1.2f;
        private SunSystem sunSystem;
        private int amount;
        private float targetY;
        private bool falling;

        public void Initialize(SunSystem owner, int value, float stopY)
        {
            sunSystem = owner;
            amount = value;
            targetY = stopY;
            falling = true;
        }

        private void Update()
        {
            if (!falling)
            {
                return;
            }

            Vector3 position = transform.position;
            position.y = Mathf.Max(targetY, position.y - fallSpeed * Time.deltaTime);
            transform.position = position;
            falling = position.y > targetY;
        }

        private void OnMouseDown()
        {
            sunSystem.Collect(this, amount);
        }

        public void OnSpawned() { }
        public void OnDespawned() => sunSystem = null;
    }
}
```

Create `SunSystem.cs`:

```csharp
using LawnDefense.Core;
using LawnDefense.Data;
using UnityEngine;

namespace LawnDefense.Sun
{
    public sealed class SunSystem : MonoBehaviour
    {
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private GameObject sunPrefab;
        [SerializeField] private Vector2 spawnXRange = new Vector2(-3.5f, 3.5f);
        [SerializeField] private float spawnY = 3.5f;
        [SerializeField] private float stopYMin = -1.5f;
        [SerializeField] private float stopYMax = 2.5f;

        private readonly SunWallet wallet = new SunWallet();
        private LevelConfig levelConfig;
        private float timer;

        public SunWallet Wallet => wallet;

        public void Initialize(LevelConfig config)
        {
            levelConfig = config;
            timer = 0f;
            wallet.Set(config.InitialSun);
        }

        private void Update()
        {
            if (levelConfig == null)
            {
                return;
            }

            timer += Time.deltaTime;
            if (timer >= levelConfig.NaturalSunInterval)
            {
                timer = 0f;
                SpawnSun(levelConfig.NaturalSunAmount, new Vector3(Random.Range(spawnXRange.x, spawnXRange.y), spawnY, 0f));
            }
        }

        public void SpawnSun(int amount, Vector3 position)
        {
            GameObject instance = poolManager.Spawn(sunPrefab, position, Quaternion.identity);
            if (instance != null && instance.TryGetComponent(out SunCollectible collectible))
            {
                collectible.Initialize(this, amount, Random.Range(stopYMin, stopYMax));
            }
        }

        public void Collect(SunCollectible collectible, int amount)
        {
            wallet.Add(amount);
            poolManager.Despawn(collectible.gameObject);
        }
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/_Project/Scripts/Sun
git commit -m "feat: add sun economy"
```

## Task 6: Plants and Placement

**Files:**
- Create: `Assets/_Project/Scripts/Plants/Plant.cs`
- Create: `Assets/_Project/Scripts/Plants/PlantHealth.cs`
- Create: `Assets/_Project/Scripts/Plants/PlantAttackController.cs`
- Create: `Assets/_Project/Scripts/Plants/SunProducer.cs`
- Create: `Assets/_Project/Scripts/Placement/PlantCardState.cs`
- Create: `Assets/_Project/Scripts/Placement/PlantPlacementSystem.cs`

- [ ] **Step 1: Add plant root and health**

Create `Plant.cs` and `PlantHealth.cs` so each plant stores its `PlantConfig`, `GridCoordinate`, and releases its grid cell on death.

Required public API:

```csharp
public PlantConfig Config { get; }
public GridCoordinate Coordinate { get; }
public void Initialize(PlantConfig config, GridCoordinate coordinate, GridSystem gridSystem);
```

`PlantHealth.TakeDamage(int amount)` must reduce health and call `Plant.Die()` at zero.

- [ ] **Step 2: Add plant card state**

Create `PlantCardState.cs`:

```csharp
using LawnDefense.Core;
using LawnDefense.Data;
using UnityEngine;

namespace LawnDefense.Placement
{
    public sealed class PlantCardState
    {
        public PlantConfig Config { get; }
        public float RemainingCooldown { get; private set; }
        public bool IsReady => RemainingCooldown <= 0f;

        public PlantCardState(PlantConfig config)
        {
            Config = config;
        }

        public void StartCooldown()
        {
            RemainingCooldown = Config.Cooldown;
            GameEvents.RaisePlantCardCooldownChanged(Config.Id, 1f);
        }

        public void Tick(float deltaTime)
        {
            if (RemainingCooldown <= 0f)
            {
                return;
            }

            RemainingCooldown = Mathf.Max(0f, RemainingCooldown - deltaTime);
            float normalized = Config.Cooldown <= 0f ? 0f : RemainingCooldown / Config.Cooldown;
            GameEvents.RaisePlantCardCooldownChanged(Config.Id, normalized);
        }
    }
}
```

- [ ] **Step 3: Add placement system**

Create `PlantPlacementSystem.cs` with:

- `Initialize(PlantConfig[] availablePlants)`
- `SelectPlant(PlantConfig config)`
- cooldown ticking in `Update`
- mouse world-to-grid placement using `Camera.main.ScreenToWorldPoint(Input.mousePosition)`
- sun spend through `SunSystem.Wallet`
- prefab spawn through `PoolManager`
- `Plant.Initialize(config, coordinate, gridSystem)`

- [ ] **Step 4: Add attack and sun production**

Create `PlantAttackController.cs` so shooter plants periodically call `LaneTargetService.FindFirstEnemyInLane(row, minX, maxX)`.

Create `SunProducer.cs` so producer plants call `SunSystem.SpawnSun(config.SunProduceAmount, transform.position + Vector3.up * 0.3f)` every `SunProduceInterval`.

- [ ] **Step 5: Commit**

```bash
git add Assets/_Project/Scripts/Plants Assets/_Project/Scripts/Placement
git commit -m "feat: add plant placement and behavior"
```

## Task 7: Enemies and Combat

**Files:**
- Create: `Assets/_Project/Scripts/Combat/DamageInfo.cs`
- Create: `Assets/_Project/Scripts/Combat/LaneTargetService.cs`
- Create: `Assets/_Project/Scripts/Combat/Projectile.cs`
- Create: `Assets/_Project/Scripts/Enemies/Enemy.cs`
- Create: `Assets/_Project/Scripts/Enemies/EnemyHealth.cs`
- Create: `Assets/_Project/Scripts/Enemies/EnemyMovement.cs`
- Create: `Assets/_Project/Scripts/Enemies/EnemyAttackController.cs`

- [ ] **Step 1: Add damage info**

Create `DamageInfo.cs`:

```csharp
using LawnDefense.Data;

namespace LawnDefense.Combat
{
    public struct DamageInfo
    {
        public int Amount;
        public DamageType DamageType;
        public object Source;

        public DamageInfo(int amount, DamageType damageType, object source)
        {
            Amount = amount;
            DamageType = damageType;
            Source = source;
        }
    }
}
```

- [ ] **Step 2: Add enemy root, health, and movement**

`Enemy.Initialize(EnemyConfig config, int lane, float defeatX)` must set config, lane, health, movement speed, and defeat line.

`EnemyMovement` moves left while not blocked.

`EnemyHealth.TakeDamage(DamageInfo damageInfo)` reduces health and despawns or destroys at zero.

- [ ] **Step 3: Add lane target service**

Create `LaneTargetService.cs` as a MonoBehaviour registry:

- `Register(Enemy enemy)`
- `Unregister(Enemy enemy)`
- `FindFirstEnemyInLane(int lane, float minX, float maxX)`
- `GetAliveEnemyCount()`

Use a `List<Enemy>` and remove null or dead entries during queries.

- [ ] **Step 4: Add projectile**

`Projectile.Initialize(ProjectileConfig config, LaneTargetService targetService, PoolManager poolManager, int lane, object source)` must move right, query targets in its lane within hit radius, apply damage, then despawn.

- [ ] **Step 5: Add enemy attack**

`EnemyAttackController` must ray/check a small distance ahead for `Plant`, stop movement while a plant is present, and call `PlantHealth.TakeDamage(config.AttackDamage)` on interval.

- [ ] **Step 6: Commit**

```bash
git add Assets/_Project/Scripts/Combat Assets/_Project/Scripts/Enemies
git commit -m "feat: add enemies and lane combat"
```

## Task 8: Wave System and Win/Loss

**Files:**
- Create: `Assets/_Project/Scripts/Waves/WaveSystem.cs`

- [ ] **Step 1: Add wave scheduler**

Create `WaveSystem.cs` with:

- serialized references to `PoolManager`, `LaneTargetService`, `GridSystem`, and `GameStateController`
- `Initialize(LevelConfig config)`
- coroutine spawning for each `WaveEntry`
- lane choice for `Specific`, `RandomAny`, and `RandomFromAllowed`
- enemy initialization
- defeat when an enemy crosses the left boundary
- victory when all scheduled enemies spawned and `LaneTargetService.GetAliveEnemyCount() == 0`

- [ ] **Step 2: Add enemy defeat callback**

Expose `WaveSystem.NotifyEnemyReachedGoal(Enemy enemy)` and call it from `EnemyMovement` when `transform.position.x <= defeatX`.

- [ ] **Step 3: Add progress event**

Raise `GameEvents.RaiseWaveProgressChanged(spawnedCount / totalCount)` whenever an enemy spawns.

- [ ] **Step 4: Commit**

```bash
git add Assets/_Project/Scripts/Waves Assets/_Project/Scripts/Enemies
git commit -m "feat: add wave spawning and level outcomes"
```

## Task 9: UI and Camera

**Files:**
- Create: `Assets/_Project/Scripts/UI/GameHudView.cs`
- Create: `Assets/_Project/Scripts/UI/PlantCardView.cs`
- Create: `Assets/_Project/Scripts/UI/ResultView.cs`
- Create: `Assets/_Project/Scripts/Camera/CameraFitController.cs`

- [ ] **Step 1: Add HUD**

`GameHudView` subscribes to `GameEvents.SunChanged`, `WaveProgressChanged`, and `GameStateChanged`.

Serialized fields:

- `Text sunText`
- `Slider waveProgressSlider`
- `GameObject resultPanel`
- `Text resultText`

- [ ] **Step 2: Add plant card view**

`PlantCardView` displays icon, cost, cooldown fill, and calls `PlantPlacementSystem.SelectPlant(config)` on button click.

Serialized fields:

- `Image iconImage`
- `Text costText`
- `Image cooldownFill`
- `Button button`

- [ ] **Step 3: Add result view**

`ResultView` shows victory or defeat and exposes `Restart()` using `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex)`.

- [ ] **Step 4: Add camera fit**

`CameraFitController.Fit(GridSystem gridSystem)` sets the main orthographic camera to view the configured grid bounds with padding.

- [ ] **Step 5: Commit**

```bash
git add Assets/_Project/Scripts/UI Assets/_Project/Scripts/Camera
git commit -m "feat: add prototype UI and camera fitting"
```

## Task 10: Editor Wiring and Validation

**Files:**
- Create in Unity editor: `Assets/_Project/Scenes/Main.unity`
- Create in Unity editor: plant, enemy, projectile, sun, grid, UI prefabs under `Assets/_Project/Prefabs/`
- Create in Unity editor: config assets under `Assets/_Project/Data/`
- Optional Create: `docs/manual-test-checklist.md`

- [ ] **Step 1: Create placeholder art**

Use simple original colored sprites or Unity primitive sprite shapes. Do not import commercial game assets or trademarked content.

- [ ] **Step 2: Create prefabs**

Create these prefabs and attach matching scripts:

- Shooter plant: `Plant`, `PlantHealth`, `PlantAttackController`
- Producer plant: `Plant`, `PlantHealth`, `SunProducer`
- Blocker plant: `Plant`, `PlantHealth`
- Normal enemy: `Enemy`, `EnemyHealth`, `EnemyMovement`, `EnemyAttackController`
- Heavy enemy: `Enemy`, `EnemyHealth`, `EnemyMovement`, `EnemyAttackController`
- Projectile: `Projectile`
- Sun collectible: `SunCollectible`

- [ ] **Step 3: Create config assets**

Create:

- 3 `PlantConfig` assets with original display names.
- 2 `EnemyConfig` assets with original display names.
- 1 `ProjectileConfig` asset.
- 1 `WaveConfig` with at least 8 total enemies.
- 1 `LevelConfig` using 5 rows, 9 columns, 50 initial sun.

- [ ] **Step 4: Create main scene**

Create a main scene with:

- `GameBootstrap`
- `GameStateController`
- `PoolManager`
- `GridSystem`
- `SunSystem`
- `PlantPlacementSystem`
- `LaneTargetService`
- `WaveSystem`
- `CameraFitController`
- Canvas with HUD and plant cards

Wire serialized references in the Inspector.

- [ ] **Step 5: Manual validation**

Run Play Mode and verify:

1. Initial sun appears.
2. A plant can be placed on an empty grid cell.
3. Sun cannot go negative.
4. Card cooldown blocks immediate repeat placement.
5. Natural sun appears and can be collected.
6. Producer plant creates sun.
7. Enemies spawn by wave and move left.
8. Shooter plant damages same-lane enemies.
9. Enemy dies at zero health.
10. Enemy attacks a blocking plant.
11. Enemy reaching the left boundary triggers defeat.
12. Clearing all waves triggers victory.
13. Console has no new errors or missing references.

- [ ] **Step 6: Commit editor assets**

```bash
git add Assets ProjectSettings Packages docs/manual-test-checklist.md
git commit -m "feat: wire playable tower defense prototype"
```

## Self-Review

- Spec coverage: the plan covers source structure, ScriptableObject data, grid placement, sun economy, plant behavior, enemy behavior, projectile combat, waves, UI, camera, object pool, and manual validation.
- Known boundary: this repository currently lacks a Unity project. Script files and folders can be created from the shell, but scene, prefab, sprite, and `.asset` authoring should be done through Unity editor 1.8.5 to preserve `.meta` references.
- Placeholder scan: no unresolved placeholder markers are used.
- Type consistency: namespaces use `LawnDefense.*`; data classes use `PlantConfig`, `EnemyConfig`, `ProjectileConfig`, `LevelConfig`, and `WaveConfig`; runtime systems use the same names across tasks.
