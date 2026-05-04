using LawnDefense.Core;
using LawnDefense.Data;
using UnityEngine;

namespace LawnDefense.Plants
{
    public sealed class PlantAttackController : MonoBehaviour
    {
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private Transform firePoint;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private float targetProbeHeight = 0.2f;

        private Plant owner;
        private float timer;

        public void Initialize(Plant plant)
        {
            owner = plant;
            timer = 0f;
        }

        private void Update()
        {
            if (owner == null || owner.Config == null || owner.Config.Role != PlantRole.Shooter)
            {
                return;
            }

            if (owner.Config.AttackInterval <= 0f ||
                owner.Config.ProjectileConfig == null ||
                owner.Config.ProjectileConfig.Prefab == null)
            {
                return;
            }

            timer += Time.deltaTime;

            Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * targetProbeHeight;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right, owner.Config.AttackRange, targetMask);
            if (hit.collider == null)
            {
                return;
            }

            if (timer < owner.Config.AttackInterval)
            {
                return;
            }

            timer = 0f;
            if (poolManager != null)
            {
                poolManager.Spawn(owner.Config.ProjectileConfig.Prefab, origin, Quaternion.identity);
            }
            else
            {
                Instantiate(owner.Config.ProjectileConfig.Prefab, origin, Quaternion.identity);
            }
        }
    }
}
