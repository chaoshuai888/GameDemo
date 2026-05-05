using LawnDefense.Combat;
using LawnDefense.Core;
using LawnDefense.Data;
using LawnDefense.Sun;
using LawnDefense.Waves;
using UnityEngine;

namespace LawnDefense.Enemies
{
    public sealed class Enemy : MonoBehaviour
    {
        [SerializeField] private PoolManager poolManager;

        private EnemyHealth health;
        private EnemyMovement movement;
        private EnemyAttackController attackController;
        private EnemyStatusController statusController;
        private EnemyDeathReward deathReward;
        private EnemyLaneAura laneAura;
        private WaveSystem waveSystem;
        private SunSystem sunSystem;
        private bool deathRewardGranted;

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
            statusController = GetComponentInChildren<EnemyStatusController>(true);
            deathReward = GetComponentInChildren<EnemyDeathReward>(true);
            laneAura = GetComponentInChildren<EnemyLaneAura>(true);
            deathRewardGranted = false;

            if (health != null)
            {
                health.Initialize(this, config != null ? config.MaxHealth : 1);
            }

            if (movement != null)
            {
                movement.Initialize(this, config != null ? config.MoveSpeed : 0f, defeatX);
                movement.SetWaveSystem(waveSystem);
            }

            if (attackController != null)
            {
                attackController.Initialize(this);
            }

            if (statusController != null)
            {
                statusController.ResetStatuses();
            }

            EnemyArmor armor = GetComponentInChildren<EnemyArmor>(true);
            if (armor != null)
            {
                armor.Initialize(config != null ? config.Armor : 0);
            }

            if (deathReward != null)
            {
                deathReward.Initialize(config);
            }

            if (laneAura != null)
            {
                laneAura.Initialize(this, config);
            }
        }

        public void SetPoolManager(PoolManager ownerPool)
        {
            poolManager = ownerPool;
        }

        public void SetWaveSystem(WaveSystem ownerWaveSystem)
        {
            waveSystem = ownerWaveSystem;
            if (movement != null)
            {
                movement.SetWaveSystem(waveSystem);
            }
        }

        public void SetSunSystem(SunSystem ownerSunSystem)
        {
            sunSystem = ownerSunSystem;
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
            GrantDeathReward();

            if (poolManager != null)
            {
                poolManager.Despawn(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ApplySlow(float percent, float duration)
        {
            if (statusController != null)
            {
                statusController.ApplySlow(percent, duration);
            }
        }

        public void ApplySpeedBoost(float multiplier, float duration)
        {
            if (statusController != null)
            {
                statusController.ApplySpeedBoost(multiplier, duration);
            }
        }

        public float GetMovementMultiplier()
        {
            return statusController != null ? statusController.MovementMultiplier : 1f;
        }

        public void SetMovementBlocked(bool blocked)
        {
            if (movement != null)
            {
                movement.SetBlocked(blocked);
            }
        }

        private void GrantDeathReward()
        {
            if (deathRewardGranted)
            {
                return;
            }

            deathRewardGranted = true;
            if (deathReward != null)
            {
                deathReward.Grant(Config, sunSystem);
            }
            else if (sunSystem != null && Config != null)
            {
                int reward = LawnDefense.Augments.AugmentSystem.Modifiers.GetEnemyRewardSun(Config.RewardSun);
                if (reward > 0)
                {
                    sunSystem.Wallet.Add(reward);
                }
            }
        }
    }
}
