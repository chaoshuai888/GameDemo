using LawnDefense.Combat;
using LawnDefense.Core;
using LawnDefense.Data;
using LawnDefense.Enemies;
using UnityEngine;

namespace LawnDefense.Plants
{
    public sealed class SporeMine : MonoBehaviour
    {
        private Plant owner;
        private LaneTargetService targetService;
        private bool triggered;

        public void ConfigureRuntimeServices(PoolManager ownerPoolManager, LaneTargetService ownerTargetService)
        {
            targetService = ownerTargetService;
        }

        public void Initialize(Plant plant)
        {
            owner = plant;
            triggered = false;
        }

        private void Update()
        {
            if (triggered || owner == null || owner.Config == null || targetService == null)
            {
                return;
            }

            float radius = Mathf.Max(0.1f, owner.Config.TriggerRadius);
            float x = transform.position.x;
            Enemy enemy = targetService.FindFirstEnemyInLane(owner.Coordinate.Row, x - radius, x + radius);
            if (enemy == null)
            {
                return;
            }

            Trigger();
        }

        private void Trigger()
        {
            triggered = true;
            float radius = Mathf.Max(0.1f, owner.Config.AreaRadius);
            int damage = Mathf.Max(1, owner.Config.AreaDamage);
            Enemy[] enemies = FindObjectsOfType<Enemy>();

            for (int i = 0; i < enemies.Length; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || enemy.Lane != owner.Coordinate.Row)
                {
                    continue;
                }

                if (Mathf.Abs(enemy.transform.position.x - transform.position.x) <= radius)
                {
                    enemy.TakeDamage(new DamageInfo(damage, DamageType.Normal, owner));
                }
            }

            if (owner != null)
            {
                owner.Die();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
