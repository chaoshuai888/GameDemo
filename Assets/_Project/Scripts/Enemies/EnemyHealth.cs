using LawnDefense.Combat;
using UnityEngine;

namespace LawnDefense.Enemies
{
    public sealed class EnemyHealth : MonoBehaviour
    {
        private Enemy owner;

        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        public void Initialize(Enemy enemy, int maxHealth)
        {
            owner = enemy;
            CurrentHealth = Mathf.Max(1, maxHealth);
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (damageInfo.Amount <= 0 || !IsAlive)
            {
                return;
            }

            CurrentHealth -= damageInfo.Amount;
            if (CurrentHealth > 0)
            {
                return;
            }

            CurrentHealth = 0;
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
