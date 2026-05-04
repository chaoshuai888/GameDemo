using LawnDefense.Plants;
using UnityEngine;

namespace LawnDefense.Enemies
{
    public sealed class EnemyAttackController : MonoBehaviour
    {
        [SerializeField] private float attackProbeDistance = 0.35f;
        [SerializeField] private Vector2 attackProbeSize = new Vector2(0.35f, 0.6f);
        [SerializeField] private LayerMask plantMask;

        private Enemy owner;
        private float timer;

        public void Initialize(Enemy enemy)
        {
            owner = enemy;
            timer = 0f;
        }

        private void Update()
        {
            if (owner == null || owner.Config == null || !owner.IsAlive)
            {
                return;
            }

            PlantHealth target = FindPlantAhead();
            owner.SetMovementBlocked(target != null);

            if (target == null)
            {
                timer = 0f;
                return;
            }

            timer += Time.deltaTime;
            if (timer < owner.Config.AttackInterval)
            {
                return;
            }

            timer = 0f;
            target.TakeDamage(owner.Config.AttackDamage);
        }

        private PlantHealth FindPlantAhead()
        {
            Vector2 center = transform.position + Vector3.left * attackProbeDistance;
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, attackProbeSize, 0f, plantMask);

            for (int i = 0; i < hits.Length; i++)
            {
                PlantHealth plant = hits[i] != null ? hits[i].GetComponentInParent<PlantHealth>() : null;
                if (plant != null)
                {
                    return plant;
                }
            }

            return null;
        }
    }
}
