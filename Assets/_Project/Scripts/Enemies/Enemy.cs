using LawnDefense.Combat;
using LawnDefense.Core;
using LawnDefense.Data;
using UnityEngine;

namespace LawnDefense.Enemies
{
    public sealed class Enemy : MonoBehaviour
    {
        [SerializeField] private PoolManager poolManager;

        private EnemyHealth health;
        private EnemyMovement movement;
        private EnemyAttackController attackController;

        public EnemyConfig Config { get; private set; }
        public int Lane { get; private set; }
        public float DefeatX { get; private set; }
        public bool IsAlive => health != null && health.IsAlive;

        public void Initialize(EnemyConfig config, int lane, float defeatX)
        {
            Config = config;
            Lane = lane;
            DefeatX = defeatX;

            health = GetComponentInChildren<EnemyHealth>(true);
            movement = GetComponentInChildren<EnemyMovement>(true);
            attackController = GetComponentInChildren<EnemyAttackController>(true);

            if (health != null)
            {
                health.Initialize(this, config != null ? config.MaxHealth : 1);
            }

            if (movement != null)
            {
                movement.Initialize(this, config != null ? config.MoveSpeed : 0f, defeatX);
            }

            if (attackController != null)
            {
                attackController.Initialize(this);
            }
        }

        public void SetPoolManager(PoolManager ownerPool)
        {
            poolManager = ownerPool;
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (health != null)
            {
                health.TakeDamage(damageInfo);
            }
        }

        public void Die()
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

        public void SetMovementBlocked(bool blocked)
        {
            if (movement != null)
            {
                movement.SetBlocked(blocked);
            }
        }
    }
}
