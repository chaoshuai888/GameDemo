using LawnDefense.Combat;
using LawnDefense.Data;
using UnityEngine;

namespace LawnDefense.Enemies
{
    public sealed class EnemyLaneAura : MonoBehaviour
    {
        private Enemy owner;
        private float multiplier = 1f;
        private float duration;
        private bool applied;

        public void Initialize(Enemy enemy, EnemyConfig config)
        {
            owner = enemy;
            multiplier = config != null ? config.LaneAuraSpeedMultiplier : 1f;
            duration = config != null ? config.LaneAuraDuration : 0f;
            applied = false;
            Apply();
        }

        private void Apply()
        {
            if (applied || owner == null || multiplier <= 1f || duration <= 0f)
            {
                return;
            }

            applied = true;
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            for (int i = 0; i < enemies.Length; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy != null && enemy != owner && enemy.Lane == owner.Lane && enemy.IsAlive)
                {
                    enemy.ApplySpeedBoost(multiplier, duration);
                }
            }
        }
    }
}
