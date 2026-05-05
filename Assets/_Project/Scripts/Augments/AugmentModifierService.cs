using System.Collections.Generic;
using LawnDefense.Data;
using UnityEngine;

namespace LawnDefense.Augments
{
    public sealed class AugmentModifierService
    {
        private readonly List<AugmentConfig> activeAugments = new List<AugmentConfig>();

        public IReadOnlyList<AugmentConfig> ActiveAugments => activeAugments;

        public void SetActiveAugments(IEnumerable<AugmentConfig> augments)
        {
            activeAugments.Clear();
            if (augments == null)
            {
                return;
            }

            foreach (AugmentConfig augment in augments)
            {
                if (augment != null)
                {
                    activeAugments.Add(augment);
                }
            }
        }

        public int GetInitialSun(int baseValue)
        {
            return Mathf.Max(0, Mathf.RoundToInt(ApplyFlatAndPercent(baseValue, AugmentEffectType.InitialSun)));
        }

        public int GetPlantCost(PlantConfig config)
        {
            int baseValue = config != null ? config.SunCost : 0;
            return Mathf.Max(0, Mathf.RoundToInt(ApplyFlatAndPercent(baseValue, AugmentEffectType.PlantCost)));
        }

        public float GetPlantCooldown(PlantConfig config)
        {
            float baseValue = config != null ? config.Cooldown : 0f;
            return Mathf.Max(0.1f, ApplyFlatAndPercent(baseValue, AugmentEffectType.PlantCooldown));
        }

        public int GetPlantMaxHealth(PlantConfig config)
        {
            int baseValue = config != null ? config.MaxHealth : 1;
            return Mathf.Max(1, Mathf.RoundToInt(ApplyFlatAndPercent(baseValue, AugmentEffectType.PlantMaxHealth)));
        }

        public float GetPlantAttackInterval(PlantConfig config)
        {
            float baseValue = config != null ? config.AttackInterval : 0f;
            return Mathf.Max(0.1f, ApplyFlatAndPercent(baseValue, AugmentEffectType.PlantAttackInterval));
        }

        public int GetProjectileDamage(ProjectileConfig config)
        {
            int baseValue = config != null ? config.Damage : 0;
            return Mathf.Max(1, Mathf.RoundToInt(ApplyFlatAndPercent(baseValue, AugmentEffectType.ProjectileDamage)));
        }

        public float GetNaturalSunInterval(float baseValue)
        {
            return Mathf.Max(0.5f, ApplyFlatAndPercent(baseValue, AugmentEffectType.NaturalSunInterval));
        }

        public int GetEnemyRewardSun(int baseValue)
        {
            return Mathf.Max(0, Mathf.RoundToInt(ApplyFlatAndPercent(baseValue, AugmentEffectType.EnemyRewardSun)));
        }

        public float GetFirstDefeatDelay()
        {
            return Mathf.Max(0f, SumFlat(AugmentEffectType.FirstDefeatDelay));
        }

        public int GetPreparedFieldSunCount()
        {
            return Mathf.Max(0, Mathf.RoundToInt(SumFlat(AugmentEffectType.PreparedFieldSun)));
        }

        private float ApplyFlatAndPercent(float baseValue, AugmentEffectType effectType)
        {
            float flat = 0f;
            float percent = 0f;

            for (int i = 0; i < activeAugments.Count; i++)
            {
                AugmentConfig augment = activeAugments[i];
                if (augment == null || augment.EffectType != effectType)
                {
                    continue;
                }

                flat += augment.FlatValue;
                percent += augment.PercentValue;
            }

            if (flat < 0f && percent < 0f)
            {
                return baseValue + Mathf.Min(baseValue * percent, flat);
            }

            return baseValue * (1f + percent) + flat;
        }

        private float SumFlat(AugmentEffectType effectType)
        {
            float value = 0f;
            for (int i = 0; i < activeAugments.Count; i++)
            {
                AugmentConfig augment = activeAugments[i];
                if (augment != null && augment.EffectType == effectType)
                {
                    value += augment.FlatValue;
                }
            }

            return value;
        }
    }
}
