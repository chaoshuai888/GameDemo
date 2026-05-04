using UnityEngine;

namespace LawnDefense.Plants
{
    public sealed class PlantHealth : MonoBehaviour
    {
        private Plant owner;

        public int CurrentHealth { get; private set; }

        public void Initialize(Plant plant, int maxHealth)
        {
            owner = plant;
            CurrentHealth = Mathf.Max(1, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CurrentHealth -= amount;
            if (CurrentHealth > 0)
            {
                return;
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
