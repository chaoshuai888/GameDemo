using UnityEngine;

namespace LawnDefense.Enemies
{
    public sealed class EnemyArmor : MonoBehaviour
    {
        [SerializeField] private int armor;

        public void Initialize(int value)
        {
            armor = Mathf.Max(0, value);
        }

        public int ReduceDamage(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            return Mathf.Max(1, amount - armor);
        }
    }
}
