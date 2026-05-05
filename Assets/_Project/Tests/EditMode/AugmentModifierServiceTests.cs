using LawnDefense.Augments;
using LawnDefense.Data;
using NUnit.Framework;
using UnityEngine;

namespace LawnDefense.Tests.EditMode
{
    public sealed class AugmentModifierServiceTests
    {
        [Test]
        public void PlantCostUsesPercentAndFlatMinimumReduction()
        {
            PlantConfig plant = ScriptableObject.CreateInstance<PlantConfig>();
            plant.SunCost = 50;

            AugmentConfig augment = ScriptableObject.CreateInstance<AugmentConfig>();
            augment.EffectType = AugmentEffectType.PlantCost;
            augment.PercentValue = -0.1f;
            augment.FlatValue = -5f;

            AugmentModifierService service = new AugmentModifierService();
            service.SetActiveAugments(new[] { augment });

            Assert.AreEqual(45, service.GetPlantCost(plant));
        }

        [Test]
        public void PositivePercentModifiersStackAdditively()
        {
            PlantConfig plant = ScriptableObject.CreateInstance<PlantConfig>();
            plant.MaxHealth = 100;

            AugmentConfig first = ScriptableObject.CreateInstance<AugmentConfig>();
            first.EffectType = AugmentEffectType.PlantMaxHealth;
            first.PercentValue = 0.2f;

            AugmentConfig second = ScriptableObject.CreateInstance<AugmentConfig>();
            second.EffectType = AugmentEffectType.PlantMaxHealth;
            second.PercentValue = 0.1f;

            AugmentModifierService service = new AugmentModifierService();
            service.SetActiveAugments(new[] { first, second });

            Assert.AreEqual(130, service.GetPlantMaxHealth(plant));
        }

        [Test]
        public void NaturalSunIntervalNeverDropsBelowMinimum()
        {
            AugmentConfig augment = ScriptableObject.CreateInstance<AugmentConfig>();
            augment.EffectType = AugmentEffectType.NaturalSunInterval;
            augment.PercentValue = -0.95f;

            AugmentModifierService service = new AugmentModifierService();
            service.SetActiveAugments(new[] { augment });

            Assert.AreEqual(0.5f, service.GetNaturalSunInterval(6f));
        }
    }
}
