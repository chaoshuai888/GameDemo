using UnityEngine;

namespace LawnDefense.Data
{
    [CreateAssetMenu(menuName = "LawnDefense/Plant Config")]
    public sealed class PlantConfig : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Sprite Icon;
        public GameObject Prefab;
        public int SunCost = 50;
        public float Cooldown = 5f;
        public int MaxHealth = 100;
        public PlantRole Role;
        public float AttackInterval = 1.5f;
        public float AttackRange = 9f;
        public ProjectileConfig ProjectileConfig;
        public float SunProduceInterval = 8f;
        public int SunProduceAmount = 25;
        public string[] PlantTags;
        public float TriggerRadius = 0.45f;
        public float AreaRadius = 0.9f;
        public int AreaDamage = 80;
    }
}
