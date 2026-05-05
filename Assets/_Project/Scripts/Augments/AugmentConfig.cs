using UnityEngine;

namespace LawnDefense.Augments
{
    [CreateAssetMenu(menuName = "LawnDefense/Augment Config")]
    public sealed class AugmentConfig : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea(2, 4)] public string Description;
        public Sprite Icon;
        public AugmentRarity Rarity;
        public AugmentEffectType EffectType;
        public float FlatValue;
        public float PercentValue;
    }
}
