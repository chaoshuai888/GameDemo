using LawnDefense.Augments;
using LawnDefense.Combat;
using LawnDefense.Core;
using LawnDefense.Data;
using UnityEngine;

namespace LawnDefense.Plants
{
    public sealed class PlantAttackController : MonoBehaviour
    {
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private LaneTargetService targetService;
        [SerializeField] private Transform firePoint;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private float targetProbeHeight = 0.2f;

        private Plant owner;
        private float timer;

        public void ConfigureRuntimeServices(PoolManager ownerPoolManager, LaneTargetService ownerTargetService)
        {
            poolManager = ownerPoolManager;
            targetService = ownerTargetService;
        }

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

            float attackInterval = AugmentSystem.Modifiers.GetPlantAttackInterval(owner.Config);
            if (attackInterval <= 0f ||
                owner.Config.ProjectileConfig == null ||
                owner.Config.ProjectileConfig.Prefab == null)
            {
                return;
            }

            timer += Time.deltaTime;

            Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * targetProbeHeight;
            if (!HasTarget(origin))
            {
                return;
            }

            if (timer < attackInterval)
            {
                return;
            }

            timer = 0f;
            GameObject projectile = null;
            if (poolManager != null)
            {
                projectile = poolManager.Spawn(owner.Config.ProjectileConfig.Prefab, origin, Quaternion.identity);
            }
            else
            {
                projectile = Instantiate(owner.Config.ProjectileConfig.Prefab, origin, Quaternion.identity);
            }

            if (projectile != null && projectile.TryGetComponent(out Projectile projectileComponent))
            {
                projectileComponent.Initialize(
                    owner.Config.ProjectileConfig,
                    targetService,
                    poolManager,
                    owner.Coordinate.Row,
                    owner);
            }
        }

        private bool HasTarget(Vector3 origin)
        {
            if (targetService != null)
            {
                return targetService.FindFirstEnemyInLane(
                    owner.Coordinate.Row,
                    origin.x,
                    origin.x + owner.Config.AttackRange) != null;
            }

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right, owner.Config.AttackRange, targetMask);
            return hit.collider != null;
        }
    }
}
