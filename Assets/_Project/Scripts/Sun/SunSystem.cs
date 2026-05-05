using LawnDefense.Augments;
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
            if (config == null)
            {
                Debug.LogError("SunSystem requires a LevelConfig to initialize.", this);
                wallet.Set(0);
                return;
            }

            wallet.Set(AugmentSystem.Modifiers.GetInitialSun(config.InitialSun));
            SpawnPreparedFieldSun(config);
        }

        private void Update()
        {
            if (levelConfig == null)
            {
                return;
            }

            float interval = AugmentSystem.Modifiers.GetNaturalSunInterval(levelConfig.NaturalSunInterval);
            if (interval <= 0f)
            {
                return;
            }

            timer += Time.deltaTime;
            if (timer >= interval)
            {
                timer = 0f;
                SpawnSun(levelConfig.NaturalSunAmount, new Vector3(Random.Range(spawnXRange.x, spawnXRange.y), spawnY, 0f));
            }
        }

        public void SpawnSun(int amount, Vector3 position)
        {
            if (poolManager == null)
            {
                return;
            }

            GameObject instance = poolManager.Spawn(sunPrefab, position, Quaternion.identity);
            if (instance != null && instance.TryGetComponent(out SunCollectible collectible))
            {
                collectible.Initialize(this, amount, Random.Range(stopYMin, stopYMax));
            }
        }

        private void SpawnPreparedFieldSun(LevelConfig config)
        {
            int count = AugmentSystem.Modifiers.GetPreparedFieldSunCount();
            if (count <= 0 || config == null)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 position = new Vector3(Random.Range(spawnXRange.x, spawnXRange.y), Random.Range(stopYMin, stopYMax), 0f);
                SpawnSun(config.NaturalSunAmount, position);
            }
        }

        public void Collect(SunCollectible collectible, int amount)
        {
            wallet.Add(amount);

            if (poolManager != null && collectible != null)
            {
                poolManager.Despawn(collectible.gameObject);
            }
        }
    }
}
