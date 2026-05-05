using System;
using System.Collections.Generic;
using LawnDefense.Augments;
using LawnDefense.Combat;
using LawnDefense.Data;
using LawnDefense.Enemies;
using LawnDefense.Placement;
using LawnDefense.Plants;
using UnityEditor;
using UnityEngine;

namespace LawnDefense.EditorTools
{
    public static class ExpandedContentVerifier
    {
        [MenuItem("LawnDefense/Verify Expanded Content")]
        public static void VerifyExpandedContent()
        {
            List<string> errors = new List<string>();

            VerifyAugments(errors);
            VerifyLevel(errors);
            VerifyPlantPrefabs(errors);
            VerifyEnemyPrefabs(errors);
            VerifyModifierMath(errors);
            VerifyAugmentSelectionFlow(errors);
            VerifyPlantSelectionFlow(errors);

            if (errors.Count > 0)
            {
                for (int i = 0; i < errors.Count; i++)
                {
                    Debug.LogError(errors[i]);
                }

                throw new InvalidOperationException("Expanded content verification failed with " + errors.Count + " issue(s).");
            }

            Debug.Log("Expanded content verification passed.");
        }

        private static void VerifyAugments(List<string> errors)
        {
            string[] guids = AssetDatabase.FindAssets("t:AugmentConfig", new[] { "Assets/_Project/Data/Augments" });
            if (guids.Length != 10)
            {
                errors.Add("Expected 10 augment configs, found " + guids.Length + ".");
            }
        }

        private static void VerifyLevel(List<string> errors)
        {
            LevelConfig level = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/_Project/Data/Levels/ExpandedPrototypeLevel.asset");
            if (level == null)
            {
                errors.Add("Missing ExpandedPrototypeLevel.asset.");
                return;
            }

            if (level.AvailablePlants == null || level.AvailablePlants.Length < 8)
            {
                errors.Add("ExpandedPrototypeLevel must expose at least 8 plants.");
            }

            if (level.AvailableAugments == null || level.AvailableAugments.Length != 10)
            {
                errors.Add("ExpandedPrototypeLevel must expose exactly 10 augments.");
            }

            if (level.WaveConfig == null || level.WaveConfig.Entries == null || level.WaveConfig.Entries.Length < 6)
            {
                errors.Add("ExpandedPrototypeLevel must reference an expanded wave with at least 6 entries.");
            }
        }

        private static void VerifyPlantPrefabs(List<string> errors)
        {
            RequireComponent<Plant>("Assets/_Project/Prefabs/Plants/Expanded/MistSprout.prefab", errors);
            RequireComponent<PlantAttackController>("Assets/_Project/Prefabs/Plants/Expanded/MistSprout.prefab", errors);
            RequireComponent<PlantAttackController>("Assets/_Project/Prefabs/Plants/Expanded/ThornPod.prefab", errors);
            RequireComponent<SunProducer>("Assets/_Project/Prefabs/Plants/Expanded/BloomBattery.prefab", errors);
            RequireComponent<PlantHealth>("Assets/_Project/Prefabs/Plants/Expanded/BarkBastion.prefab", errors);
            RequireComponent<SporeMine>("Assets/_Project/Prefabs/Plants/Expanded/SporeMine.prefab", errors);
        }

        private static void VerifyEnemyPrefabs(List<string> errors)
        {
            RequireComponent<EnemyStatusController>("Assets/_Project/Prefabs/Enemies/Expanded/MossSkitter.prefab", errors);
            RequireComponent<EnemyArmor>("Assets/_Project/Prefabs/Enemies/Expanded/ShellbackShambler.prefab", errors);
            RequireComponent<EnemyDeathReward>("Assets/_Project/Prefabs/Enemies/Expanded/BloomCarrier.prefab", errors);
            RequireComponent<EnemyLaneAura>("Assets/_Project/Prefabs/Enemies/Expanded/RotHowler.prefab", errors);
        }

        private static void VerifyModifierMath(List<string> errors)
        {
            PlantConfig plant = ScriptableObject.CreateInstance<PlantConfig>();
            plant.SunCost = 50;
            plant.MaxHealth = 100;
            plant.Cooldown = 5f;

            ProjectileConfig projectile = ScriptableObject.CreateInstance<ProjectileConfig>();
            projectile.Damage = 20;

            AugmentConfig cost = ScriptableObject.CreateInstance<AugmentConfig>();
            cost.EffectType = AugmentEffectType.PlantCost;
            cost.FlatValue = -5f;
            cost.PercentValue = -0.1f;

            AugmentConfig health = ScriptableObject.CreateInstance<AugmentConfig>();
            health.EffectType = AugmentEffectType.PlantMaxHealth;
            health.PercentValue = 0.2f;

            AugmentConfig damage = ScriptableObject.CreateInstance<AugmentConfig>();
            damage.EffectType = AugmentEffectType.ProjectileDamage;
            damage.PercentValue = 0.15f;

            AugmentModifierService service = new AugmentModifierService();
            service.SetActiveAugments(new[] { cost, health, damage });

            if (service.GetPlantCost(plant) != 45)
            {
                errors.Add("Expected frugal plant cost to be 45.");
            }

            if (service.GetPlantMaxHealth(plant) != 120)
            {
                errors.Add("Expected dense bark health to be 120.");
            }

            if (service.GetProjectileDamage(projectile) != 23)
            {
                errors.Add("Expected sharp seed damage to round to 23.");
            }

            UnityEngine.Object.DestroyImmediate(plant);
            UnityEngine.Object.DestroyImmediate(projectile);
            UnityEngine.Object.DestroyImmediate(cost);
            UnityEngine.Object.DestroyImmediate(health);
            UnityEngine.Object.DestroyImmediate(damage);
        }

        private static void VerifyAugmentSelectionFlow(List<string> errors)
        {
            LevelConfig level = ScriptableObject.CreateInstance<LevelConfig>();
            AugmentConfig[] augments = new AugmentConfig[4];
            for (int i = 0; i < augments.Length; i++)
            {
                augments[i] = ScriptableObject.CreateInstance<AugmentConfig>();
                augments[i].Id = "augment_" + i;
                augments[i].EffectType = i == 0 ? AugmentEffectType.InitialSun : AugmentEffectType.PlantMaxHealth;
                augments[i].FlatValue = i == 0 ? 25f : 0f;
            }

            level.AvailableAugments = augments;
            GameObject host = new GameObject("AugmentSystemVerifier");
            AugmentSystem system = host.AddComponent<AugmentSystem>();

            bool offered = false;
            bool completed = false;
            AugmentConfig[] offeredChoices = null;

            Action<AugmentConfig[]> handleOffered = choices =>
            {
                offered = true;
                offeredChoices = choices;
            };
            Action handleCompleted = () => completed = true;

            LawnDefense.Core.GameEvents.AugmentChoicesOffered += handleOffered;
            LawnDefense.Core.GameEvents.AugmentSelectionCompleted += handleCompleted;

            bool waitingForSelection = system.BeginSelection(level, () => completed = true);
            if (!waitingForSelection)
            {
                errors.Add("AugmentSystem should wait for selection when augments are available.");
            }

            if (!offered || offeredChoices == null || offeredChoices.Length != 3)
            {
                errors.Add("AugmentSystem should offer exactly 3 choices.");
            }
            else
            {
                system.SelectAugment(offeredChoices[0]);
                if (AugmentSystem.Modifiers.ActiveAugments.Count != 1)
                {
                    errors.Add("Selecting an augment should activate exactly one augment.");
                }
            }

            if (!completed)
            {
                errors.Add("Selecting an augment should complete selection.");
            }

            LawnDefense.Core.GameEvents.AugmentChoicesOffered -= handleOffered;
            LawnDefense.Core.GameEvents.AugmentSelectionCompleted -= handleCompleted;

            UnityEngine.Object.DestroyImmediate(host);
            UnityEngine.Object.DestroyImmediate(level);
            for (int i = 0; i < augments.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(augments[i]);
            }
        }

        private static void VerifyPlantSelectionFlow(List<string> errors)
        {
            PlantConfig plant = ScriptableObject.CreateInstance<PlantConfig>();
            plant.Id = "selection_verifier";
            plant.Cooldown = 1f;

            GameObject host = new GameObject("PlantSelectionVerifier");
            PlantPlacementSystem placementSystem = host.AddComponent<PlantPlacementSystem>();

            PlantConfig selected = null;
            Action<PlantConfig> handleSelectionChanged = config => selected = config;
            LawnDefense.Core.GameEvents.PlantSelectionChanged += handleSelectionChanged;

            placementSystem.Initialize(new[] { plant });
            placementSystem.SelectPlant(plant);

            if (selected != plant)
            {
                errors.Add("Selecting a ready plant should raise PlantSelectionChanged with that PlantConfig.");
            }

            LawnDefense.Core.GameEvents.PlantSelectionChanged -= handleSelectionChanged;
            UnityEngine.Object.DestroyImmediate(host);
            UnityEngine.Object.DestroyImmediate(plant);
        }

        private static void RequireComponent<T>(string path, List<string> errors) where T : Component
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                errors.Add("Missing prefab: " + path);
                return;
            }

            if (prefab.GetComponentInChildren<T>(true) == null)
            {
                errors.Add("Prefab lacks " + typeof(T).Name + ": " + path);
            }
        }
    }
}
