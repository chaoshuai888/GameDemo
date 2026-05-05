using System;
using System.Collections;
using LawnDefense.Augments;
using LawnDefense.Combat;
using LawnDefense.Core;
using LawnDefense.Data;
using LawnDefense.Enemies;
using LawnDefense.Grid;
using LawnDefense.Sun;
using UnityEngine;

namespace LawnDefense.Waves
{
    public sealed class WaveSystem : MonoBehaviour
    {
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private LaneTargetService targetService;
        [SerializeField] private GridSystem gridSystem;
        [SerializeField] private GameStateController gameStateController;
        [SerializeField] private SunSystem sunSystem;
        [SerializeField] private float spawnXOffset = 1.5f;
        [SerializeField] private float defeatXOffset = 1f;

        private LevelConfig levelConfig;
        private WaveEntry[] entries;
        private int totalEnemies;
        private int spawnedEnemies;
        private int completedEntries;
        private bool spawningComplete;
        private bool levelEnded;
        private bool firstDefeatDelayUsed;

        public void ConfigureRuntimeServices(SunSystem ownerSunSystem)
        {
            sunSystem = ownerSunSystem;
        }

        public void Initialize(LevelConfig config)
        {
            levelConfig = config;
            WaveEntry[] sourceEntries = config != null && config.WaveConfig != null ? config.WaveConfig.Entries : null;
            entries = sourceEntries != null ? (WaveEntry[])sourceEntries.Clone() : null;
            totalEnemies = CountEnemies(entries);
            spawnedEnemies = 0;
            completedEntries = 0;
            spawningComplete = totalEnemies == 0;
            levelEnded = false;
            firstDefeatDelayUsed = false;

            if (entries == null || entries.Length == 0)
            {
                GameEvents.RaiseWaveProgressChanged(1f);
                return;
            }

            Array.Sort(entries, CompareSpawnTime);

            for (int i = 0; i < entries.Length; i++)
            {
                StartCoroutine(SpawnEntry(entries[i]));
            }
        }

        private void Update()
        {
            if (levelEnded || !spawningComplete)
            {
                return;
            }

            if (targetService == null || targetService.GetAliveEnemyCount() == 0)
            {
                EndLevel(GameState.Victory);
            }
        }

        public void NotifyEnemyReachedGoal(Enemy enemy)
        {
            if (levelEnded)
            {
                return;
            }

            float delay = AugmentSystem.Modifiers.GetFirstDefeatDelay();
            if (!firstDefeatDelayUsed && delay > 0f)
            {
                firstDefeatDelayUsed = true;
                StartCoroutine(EndDefeatAfterDelay(delay));
                return;
            }

            EndLevel(GameState.Defeat);
        }

        private IEnumerator SpawnEntry(WaveEntry entry)
        {
            if (entry == null || entry.EnemyConfig == null || entry.Count <= 0)
            {
                CompleteEntry();
                yield break;
            }

            if (entry.SpawnTime > 0f)
            {
                yield return new WaitForSeconds(entry.SpawnTime);
            }

            for (int i = 0; i < entry.Count; i++)
            {
                SpawnEnemy(entry);
                if (entry.Interval > 0f && i < entry.Count - 1)
                {
                    yield return new WaitForSeconds(entry.Interval);
                }
            }

            CompleteEntry();
        }

        private void SpawnEnemy(WaveEntry entry)
        {
            if (entry.EnemyConfig.Prefab == null || gridSystem == null)
            {
                AdvanceProgress();
                return;
            }

            int lane = ChooseLane(entry);
            Vector3 spawnPosition = GetSpawnPosition(lane);
            GameObject instance = poolManager != null
                ? poolManager.Spawn(entry.EnemyConfig.Prefab, spawnPosition, Quaternion.identity)
                : Instantiate(entry.EnemyConfig.Prefab, spawnPosition, Quaternion.identity);

            AdvanceProgress();

            if (instance == null || !instance.TryGetComponent(out Enemy enemy))
            {
                return;
            }

            enemy.SetPoolManager(poolManager);
            enemy.SetWaveSystem(this);
            enemy.SetSunSystem(sunSystem);
            enemy.Initialize(entry.EnemyConfig, lane, GetDefeatX(lane));

            if (targetService != null)
            {
                targetService.Register(enemy);
            }
        }

        private int ChooseLane(WaveEntry entry)
        {
            int rows = gridSystem != null ? Mathf.Max(1, gridSystem.Rows) : Mathf.Max(1, levelConfig != null ? levelConfig.Rows : 5);

            if (entry.LaneMode == LaneMode.Specific)
            {
                return Mathf.Clamp(entry.SpecificLane, 0, rows - 1);
            }

            if (entry.LaneMode == LaneMode.RandomFromAllowed && entry.AllowedLanes != null && entry.AllowedLanes.Length > 0)
            {
                int lane = entry.AllowedLanes[UnityEngine.Random.Range(0, entry.AllowedLanes.Length)];
                return Mathf.Clamp(lane, 0, rows - 1);
            }

            return UnityEngine.Random.Range(0, rows);
        }

        private Vector3 GetSpawnPosition(int lane)
        {
            if (gridSystem == null)
            {
                return new Vector3(5f, lane, 0f);
            }

            int column = Mathf.Max(0, gridSystem.Columns - 1);
            Vector3 edge = gridSystem.GridToWorld(new GridCoordinate(lane, column));
            edge.x += gridSystem.CellSize.x * spawnXOffset;
            return edge;
        }

        private float GetDefeatX(int lane)
        {
            if (gridSystem == null)
            {
                return -5f;
            }

            Vector3 edge = gridSystem.GridToWorld(new GridCoordinate(lane, 0));
            return edge.x - gridSystem.CellSize.x * defeatXOffset;
        }

        private void AdvanceProgress()
        {
            spawnedEnemies++;
            float progress = totalEnemies <= 0 ? 1f : Mathf.Clamp01((float)spawnedEnemies / totalEnemies);
            GameEvents.RaiseWaveProgressChanged(progress);
        }

        private void CompleteEntry()
        {
            completedEntries++;
            spawningComplete = entries == null || completedEntries >= entries.Length;
        }

        private void EndLevel(GameState result)
        {
            levelEnded = true;
            if (gameStateController != null)
            {
                gameStateController.SetState(result);
            }
            else
            {
                GameEvents.RaiseGameStateChanged(result);
            }
        }

        private IEnumerator EndDefeatAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (!levelEnded)
            {
                EndLevel(GameState.Defeat);
            }
        }

        private static int CountEnemies(WaveEntry[] waveEntries)
        {
            if (waveEntries == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < waveEntries.Length; i++)
            {
                if (waveEntries[i] != null && waveEntries[i].EnemyConfig != null)
                {
                    count += Mathf.Max(0, waveEntries[i].Count);
                }
            }

            return count;
        }

        private static int CompareSpawnTime(WaveEntry left, WaveEntry right)
        {
            float leftTime = left != null ? left.SpawnTime : 0f;
            float rightTime = right != null ? right.SpawnTime : 0f;
            return leftTime.CompareTo(rightTime);
        }
    }
}
