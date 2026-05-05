using UnityEngine;

namespace LawnDefense.Data
{
    [CreateAssetMenu(menuName = "LawnDefense/Projectile Config")]
    public sealed class ProjectileConfig : ScriptableObject
    {
        public string Id;
        public GameObject Prefab;
        public float Speed = 6f;
        public int Damage = 20;
        public float HitRadius = 0.2f;
        public bool CanPierce;
        public int MaxPierceCount;
        public float SlowPercent;
        public float SlowDuration;
        public GameObject HitEffectPrefab;
    }
}
