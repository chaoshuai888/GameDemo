using UnityEngine;

namespace LawnDefense.Data
{
    [CreateAssetMenu(menuName = "LawnDefense/Enemy Config")]
    public sealed class EnemyConfig : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public GameObject Prefab;
        public int MaxHealth = 100;
        public float MoveSpeed = 0.7f;
        public int AttackDamage = 20;
        public float AttackInterval = 1f;
        public int RewardSun;
        public string[] EnemyTags;
    }
}
