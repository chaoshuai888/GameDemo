using LawnDefense.Core;
using LawnDefense.Data;
using LawnDefense.Enemies;
using UnityEngine;

namespace LawnDefense.Combat
{
    public sealed class Projectile : MonoBehaviour, IPoolable
    {
        private ProjectileConfig config;
        private LaneTargetService targetService;
        private PoolManager poolManager;
        private int lane;
        private object source;
        private int pierceCount;
        private bool initialized;

        public void Initialize(
            ProjectileConfig projectileConfig,
            LaneTargetService laneTargetService,
            PoolManager ownerPool,
            int targetLane,
            object damageSource)
        {
            config = projectileConfig;
            targetService = laneTargetService;
            poolManager = ownerPool;
            lane = targetLane;
            source = damageSource;
            pierceCount = 0;
            initialized = true;
        }

        private void Update()
        {
            if (!initialized || config == null)
            {
                return;
            }

            transform.position += Vector3.right * config.Speed * Time.deltaTime;
            TryHitTarget();
        }

        private void TryHitTarget()
        {
            if (targetService == null)
            {
                return;
            }

            float x = transform.position.x;
            Enemy enemy = targetService.FindFirstEnemyInLane(lane, x - config.HitRadius, x + config.HitRadius);
            if (enemy == null)
            {
                return;
            }

            enemy.TakeDamage(new DamageInfo(config.Damage, DamageType.Normal, source));

            if (!config.CanPierce)
            {
                DespawnSelf();
                return;
            }

            pierceCount++;
            if (pierceCount >= config.MaxPierceCount)
            {
                DespawnSelf();
            }
        }

        private void DespawnSelf()
        {
            if (poolManager != null)
            {
                poolManager.Despawn(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OnSpawned()
        {
            pierceCount = 0;
        }

        public void OnDespawned()
        {
            config = null;
            targetService = null;
            poolManager = null;
            source = null;
            initialized = false;
            pierceCount = 0;
        }
    }
}
