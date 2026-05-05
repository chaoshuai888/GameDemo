using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LawnDefense.Augments;
using LawnDefense.Combat;
using LawnDefense.Data;
using LawnDefense.Enemies;
using LawnDefense.Plants;
using UnityEditor;
using UnityEngine;

namespace LawnDefense.EditorTools
{
    public static class ExpandedContentBuilder
    {
        private const string ProjectRoot = "Assets/_Project";
        private const string ArtPath = ProjectRoot + "/Art/Placeholders/Expanded";
        private const string AugmentDataPath = ProjectRoot + "/Data/Augments";
        private const string PlantDataPath = ProjectRoot + "/Data/Plants/Expanded";
        private const string EnemyDataPath = ProjectRoot + "/Data/Enemies/Expanded";
        private const string ProjectileDataPath = ProjectRoot + "/Data/Projectiles/Expanded";
        private const string WaveDataPath = ProjectRoot + "/Data/Waves/Expanded";
        private const string LevelDataPath = ProjectRoot + "/Data/Levels";
        private const string PlantPrefabPath = ProjectRoot + "/Prefabs/Plants/Expanded";
        private const string EnemyPrefabPath = ProjectRoot + "/Prefabs/Enemies/Expanded";
        private const string ProjectilePrefabPath = ProjectRoot + "/Prefabs/Projectiles";

        [MenuItem("LawnDefense/Build Expanded Content")]
        public static void BuildExpandedContent()
        {
            BuildExpandedContent(true);
        }

        public static void BuildExpandedContentForSceneBuild()
        {
            BuildExpandedContent(false);
        }

        private static void BuildExpandedContent(bool refreshAssetDatabase)
        {
            EnsureFolders();
            ContentSprites sprites = CreateSprites();
            ContentPrefabs prefabs = CreatePrefabs(sprites);
            ProjectileConfigs projectiles = CreateProjectiles(prefabs);
            PlantConfigs plants = CreatePlants(prefabs, sprites, projectiles);
            EnemyConfigs enemies = CreateEnemies(prefabs);
            AugmentConfig[] augments = CreateAugments(sprites);
            WaveConfig wave = CreateWave(enemies);
            CreateLevel(plants, augments, wave);

            AssetDatabase.SaveAssets();
            if (refreshAssetDatabase)
            {
                AssetDatabase.Refresh();
            }

            Debug.Log("Expanded plant, enemy, wave, and augment content generated.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder(ArtPath);
            EnsureFolder(AugmentDataPath);
            EnsureFolder(PlantDataPath);
            EnsureFolder(EnemyDataPath);
            EnsureFolder(ProjectileDataPath);
            EnsureFolder(WaveDataPath);
            EnsureFolder(LevelDataPath);
            EnsureFolder(PlantPrefabPath);
            EnsureFolder(EnemyPrefabPath);
            EnsureFolder(ProjectilePrefabPath);
        }

        private static ContentSprites CreateSprites()
        {
            return new ContentSprites
            {
                MistSprout = CreateSprite("MistSprout", new Color(0.43f, 0.78f, 0.74f), new Color(0.12f, 0.4f, 0.42f)),
                ThornPod = CreateSprite("ThornPod", new Color(0.22f, 0.72f, 0.28f), new Color(0.08f, 0.28f, 0.08f)),
                BloomBattery = CreateSprite("BloomBattery", new Color(0.96f, 0.8f, 0.28f), new Color(0.55f, 0.28f, 0.08f)),
                BarkBastion = CreateSprite("BarkBastion", new Color(0.5f, 0.35f, 0.22f), new Color(0.24f, 0.14f, 0.08f)),
                SporeMine = CreateSprite("SporeMine", new Color(0.74f, 0.52f, 0.86f), new Color(0.32f, 0.16f, 0.42f)),
                Skitter = CreateSprite("MossSkitter", new Color(0.52f, 0.68f, 0.36f), new Color(0.2f, 0.32f, 0.12f)),
                Shellback = CreateSprite("ShellbackShambler", new Color(0.42f, 0.5f, 0.48f), new Color(0.14f, 0.18f, 0.18f)),
                Carrier = CreateSprite("BloomCarrier", new Color(0.7f, 0.62f, 0.28f), new Color(0.38f, 0.22f, 0.06f)),
                Howler = CreateSprite("RotHowler", new Color(0.55f, 0.34f, 0.48f), new Color(0.22f, 0.08f, 0.18f)),
                MistProjectile = CreateSprite("MistProjectile", new Color(0.7f, 0.92f, 0.95f), new Color(0.2f, 0.55f, 0.6f)),
                ThornProjectile = CreateSprite("ThornProjectile", new Color(0.48f, 0.9f, 0.32f), new Color(0.12f, 0.36f, 0.08f)),
                SporeBurst = CreateSprite("SporeBurst", new Color(0.86f, 0.62f, 0.92f), new Color(0.4f, 0.18f, 0.48f)),
                Augment = CreateSprite("AugmentIcon", new Color(0.38f, 0.72f, 0.95f), new Color(0.12f, 0.24f, 0.46f))
            };
        }

        private static ContentPrefabs CreatePrefabs(ContentSprites sprites)
        {
            return new ContentPrefabs
            {
                MistSprout = CreatePlantPrefab("MistSprout", sprites.MistSprout, PlantRole.Shooter, false),
                ThornPod = CreatePlantPrefab("ThornPod", sprites.ThornPod, PlantRole.Shooter, false),
                BloomBattery = CreatePlantPrefab("BloomBattery", sprites.BloomBattery, PlantRole.Producer, false),
                BarkBastion = CreatePlantPrefab("BarkBastion", sprites.BarkBastion, PlantRole.Blocker, false),
                SporeMine = CreatePlantPrefab("SporeMine", sprites.SporeMine, PlantRole.Blocker, true),
                Skitter = CreateEnemyPrefab("MossSkitter", sprites.Skitter, false, false, false),
                Shellback = CreateEnemyPrefab("ShellbackShambler", sprites.Shellback, true, false, false),
                Carrier = CreateEnemyPrefab("BloomCarrier", sprites.Carrier, false, true, false),
                Howler = CreateEnemyPrefab("RotHowler", sprites.Howler, false, false, true),
                MistProjectile = CreateProjectilePrefab("MistProjectile", sprites.MistProjectile),
                ThornProjectile = CreateProjectilePrefab("ThornProjectile", sprites.ThornProjectile)
            };
        }

        private static ProjectileConfigs CreateProjectiles(ContentPrefabs prefabs)
        {
            ProjectileConfig mist = LoadOrCreate<ProjectileConfig>(ProjectileDataPath + "/MistProjectile.asset");
            mist.Id = "mist_projectile";
            mist.Prefab = prefabs.MistProjectile;
            mist.Speed = 6f;
            mist.Damage = 14;
            mist.HitRadius = 0.25f;
            mist.CanPierce = false;
            mist.MaxPierceCount = 0;
            mist.SlowPercent = 0.35f;
            mist.SlowDuration = 2.4f;
            EditorUtility.SetDirty(mist);

            ProjectileConfig thorn = LoadOrCreate<ProjectileConfig>(ProjectileDataPath + "/ThornProjectile.asset");
            thorn.Id = "thorn_projectile";
            thorn.Prefab = prefabs.ThornProjectile;
            thorn.Speed = 7.5f;
            thorn.Damage = 18;
            thorn.HitRadius = 0.22f;
            thorn.CanPierce = true;
            thorn.MaxPierceCount = 2;
            thorn.SlowPercent = 0f;
            thorn.SlowDuration = 0f;
            EditorUtility.SetDirty(thorn);

            return new ProjectileConfigs { Mist = mist, Thorn = thorn };
        }

        private static PlantConfigs CreatePlants(ContentPrefabs prefabs, ContentSprites sprites, ProjectileConfigs projectiles)
        {
            PlantConfig mist = CreatePlant("MistSprout", "mist_sprout", "Mist Sprout", sprites.MistSprout, prefabs.MistSprout, PlantRole.Shooter, 75, 5f, 90);
            mist.AttackInterval = 1.6f;
            mist.ProjectileConfig = projectiles.Mist;
            EditorUtility.SetDirty(mist);

            PlantConfig thorn = CreatePlant("ThornPod", "thorn_pod", "Thorn Pod", sprites.ThornPod, prefabs.ThornPod, PlantRole.Shooter, 100, 6f, 95);
            thorn.AttackInterval = 1.45f;
            thorn.ProjectileConfig = projectiles.Thorn;
            EditorUtility.SetDirty(thorn);

            PlantConfig battery = CreatePlant("BloomBattery", "bloom_battery", "Bloom Battery", sprites.BloomBattery, prefabs.BloomBattery, PlantRole.Producer, 75, 8f, 85);
            battery.SunProduceInterval = 9f;
            battery.SunProduceAmount = 50;
            EditorUtility.SetDirty(battery);

            PlantConfig bastion = CreatePlant("BarkBastion", "bark_bastion", "Bark Bastion", sprites.BarkBastion, prefabs.BarkBastion, PlantRole.Blocker, 125, 10f, 540);
            EditorUtility.SetDirty(bastion);

            PlantConfig mine = CreatePlant("SporeMine", "spore_mine", "Spore Mine", sprites.SporeMine, prefabs.SporeMine, PlantRole.Blocker, 50, 9f, 45);
            mine.TriggerRadius = 0.55f;
            mine.AreaRadius = 1.1f;
            mine.AreaDamage = 110;
            EditorUtility.SetDirty(mine);

            return new PlantConfigs { Mist = mist, Thorn = thorn, Battery = battery, Bastion = bastion, Mine = mine };
        }

        private static EnemyConfigs CreateEnemies(ContentPrefabs prefabs)
        {
            EnemyConfig skitter = CreateEnemy("MossSkitter", "moss_skitter", "Moss Skitter", prefabs.Skitter, 55, 0.72f, 14, 0);
            EnemyConfig shellback = CreateEnemy("ShellbackShambler", "shellback_shambler", "Shellback Shambler", prefabs.Shellback, 150, 0.32f, 22, 0);
            shellback.Armor = 8;
            EditorUtility.SetDirty(shellback);

            EnemyConfig carrier = CreateEnemy("BloomCarrier", "bloom_carrier", "Bloom Carrier", prefabs.Carrier, 115, 0.38f, 18, 15);
            carrier.BonusRewardSun = 15;
            EditorUtility.SetDirty(carrier);

            EnemyConfig howler = CreateEnemy("RotHowler", "rot_howler", "Rot Howler", prefabs.Howler, 260, 0.28f, 32, 0);
            howler.LaneAuraSpeedMultiplier = 1.25f;
            howler.LaneAuraDuration = 5f;
            EditorUtility.SetDirty(howler);

            return new EnemyConfigs { Skitter = skitter, Shellback = shellback, Carrier = carrier, Howler = howler };
        }

        private static AugmentConfig[] CreateAugments(ContentSprites sprites)
        {
            return new[]
            {
                CreateAugment("first_light", "First Light", "Start with 50 extra sun.", AugmentRarity.Common, AugmentEffectType.InitialSun, 50f, 0f, sprites.Augment),
                CreateAugment("rapid_germination", "Rapid Germination", "All plant cooldowns are 15% shorter.", AugmentRarity.Rare, AugmentEffectType.PlantCooldown, 0f, -0.15f, sprites.Augment),
                CreateAugment("dense_bark", "Dense Bark", "All plants gain 20% max health.", AugmentRarity.Common, AugmentEffectType.PlantMaxHealth, 0f, 0.2f, sprites.Augment),
                CreateAugment("sharp_seeds", "Sharp Seeds", "Projectiles deal 15% more damage.", AugmentRarity.Rare, AugmentEffectType.ProjectileDamage, 0f, 0.15f, sprites.Augment),
                CreateAugment("golden_drip", "Golden Drip", "Natural sun falls 20% more often.", AugmentRarity.Rare, AugmentEffectType.NaturalSunInterval, 0f, -0.2f, sprites.Augment),
                CreateAugment("frugal_roots", "Frugal Roots", "Plants cost 10% less sun, with at least 5 sun saved.", AugmentRarity.Common, AugmentEffectType.PlantCost, -5f, -0.1f, sprites.Augment),
                CreateAugment("focused_rows", "Focused Rows", "Shooter attack intervals are 10% shorter.", AugmentRarity.Rare, AugmentEffectType.PlantAttackInterval, 0f, -0.1f, sprites.Augment),
                CreateAugment("prepared_field", "Prepared Field", "Two small sun pickups appear at the start.", AugmentRarity.Common, AugmentEffectType.PreparedFieldSun, 2f, 0f, sprites.Augment),
                CreateAugment("last_stand", "Last Stand", "The first breach delays defeat by 3 seconds.", AugmentRarity.Epic, AugmentEffectType.FirstDefeatDelay, 3f, 0f, sprites.Augment),
                CreateAugment("bounty_moss", "Bounty Moss", "Enemy sun rewards are increased by 5.", AugmentRarity.Common, AugmentEffectType.EnemyRewardSun, 5f, 0f, sprites.Augment)
            };
        }

        private static WaveConfig CreateWave(EnemyConfigs enemies)
        {
            EnemyConfig walker = AssetDatabase.LoadAssetAtPath<EnemyConfig>(ProjectRoot + "/Data/Prototype/MossWalker.asset");
            EnemyConfig brute = AssetDatabase.LoadAssetAtPath<EnemyConfig>(ProjectRoot + "/Data/Prototype/MossBrute.asset");
            WaveConfig wave = LoadOrCreate<WaveConfig>(WaveDataPath + "/ExpandedPrototypeWave.asset");
            wave.Entries = new[]
            {
                new WaveEntry { EnemyConfig = walker != null ? walker : enemies.Skitter, SpawnTime = 1f, Count = 3, Interval = 2f, LaneMode = LaneMode.RandomAny },
                new WaveEntry { EnemyConfig = enemies.Skitter, SpawnTime = 6f, Count = 4, Interval = 1.4f, LaneMode = LaneMode.RandomAny },
                new WaveEntry { EnemyConfig = enemies.Carrier, SpawnTime = 12f, Count = 2, Interval = 4f, LaneMode = LaneMode.RandomFromAllowed, AllowedLanes = new[] { 1, 2, 3 } },
                new WaveEntry { EnemyConfig = enemies.Shellback, SpawnTime = 26f, Count = 3, Interval = 5f, LaneMode = LaneMode.RandomAny },
                new WaveEntry { EnemyConfig = enemies.Skitter, SpawnTime = 42f, Count = 8, Interval = 1.1f, LaneMode = LaneMode.RandomAny, IsMajorWave = true },
                new WaveEntry { EnemyConfig = brute != null ? brute : enemies.Shellback, SpawnTime = 56f, Count = 1, Interval = 1f, LaneMode = LaneMode.RandomAny, IsMajorWave = true },
                new WaveEntry { EnemyConfig = enemies.Howler, SpawnTime = 64f, Count = 1, Interval = 1f, LaneMode = LaneMode.Specific, SpecificLane = 2, IsMajorWave = true },
                new WaveEntry { EnemyConfig = enemies.Shellback, SpawnTime = 68f, Count = 2, Interval = 5f, LaneMode = LaneMode.RandomFromAllowed, AllowedLanes = new[] { 1, 2, 3 } }
            };
            EditorUtility.SetDirty(wave);
            return wave;
        }

        private static void CreateLevel(PlantConfigs plants, AugmentConfig[] augments, WaveConfig wave)
        {
            PlantConfig shooter = AssetDatabase.LoadAssetAtPath<PlantConfig>(ProjectRoot + "/Data/Prototype/SproutBlaster.asset");
            PlantConfig producer = AssetDatabase.LoadAssetAtPath<PlantConfig>(ProjectRoot + "/Data/Prototype/Sunbud.asset");
            PlantConfig blocker = AssetDatabase.LoadAssetAtPath<PlantConfig>(ProjectRoot + "/Data/Prototype/Stoneleaf.asset");
            List<PlantConfig> availablePlants = new List<PlantConfig>
            {
                shooter,
                producer,
                blocker,
                plants.Mist,
                plants.Thorn,
                plants.Battery,
                plants.Bastion,
                plants.Mine
            };
            availablePlants.RemoveAll(plant => plant == null);

            LevelConfig level = LoadOrCreate<LevelConfig>(LevelDataPath + "/ExpandedPrototypeLevel.asset");
            level.Id = "expanded_prototype_level";
            level.DisplayName = "Expanded Prototype Lawn";
            level.Rows = 5;
            level.Columns = 9;
            level.InitialSun = 150;
            level.NaturalSunInterval = 6f;
            level.NaturalSunAmount = 25;
            level.AvailablePlants = availablePlants.ToArray();
            level.AvailableAugments = augments;
            level.WaveConfig = wave;
            EditorUtility.SetDirty(level);
        }

        private static PlantConfig CreatePlant(string assetName, string id, string displayName, Sprite icon, GameObject prefab, PlantRole role, int cost, float cooldown, int health)
        {
            PlantConfig config = LoadOrCreate<PlantConfig>(PlantDataPath + "/" + assetName + ".asset");
            config.Id = id;
            config.DisplayName = displayName;
            config.Icon = icon;
            config.Prefab = prefab;
            config.Role = role;
            config.SunCost = cost;
            config.Cooldown = cooldown;
            config.MaxHealth = health;
            config.AttackRange = 9f;
            return config;
        }

        private static EnemyConfig CreateEnemy(string assetName, string id, string displayName, GameObject prefab, int health, float speed, int damage, int reward)
        {
            EnemyConfig config = LoadOrCreate<EnemyConfig>(EnemyDataPath + "/" + assetName + ".asset");
            config.Id = id;
            config.DisplayName = displayName;
            config.Prefab = prefab;
            config.MaxHealth = health;
            config.MoveSpeed = speed;
            config.AttackDamage = damage;
            config.AttackInterval = 1f;
            config.RewardSun = reward;
            config.EnemyTags = new[] { "ground" };
            EditorUtility.SetDirty(config);
            return config;
        }

        private static AugmentConfig CreateAugment(string id, string displayName, string description, AugmentRarity rarity, AugmentEffectType effectType, float flat, float percent, Sprite icon)
        {
            AugmentConfig config = LoadOrCreate<AugmentConfig>(AugmentDataPath + "/" + ToAssetName(displayName) + ".asset");
            config.Id = id;
            config.DisplayName = displayName;
            config.Description = description;
            config.Rarity = rarity;
            config.EffectType = effectType;
            config.FlatValue = flat;
            config.PercentValue = percent;
            config.Icon = icon;
            EditorUtility.SetDirty(config);
            return config;
        }

        private static GameObject CreatePlantPrefab(string name, Sprite sprite, PlantRole role, bool mine)
        {
            GameObject root = new GameObject(name);
            root.AddComponent<Plant>();
            root.AddComponent<PlantHealth>();
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 5;

            BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.72f, 0.82f);

            if (role == PlantRole.Shooter)
            {
                PlantAttackController attack = root.AddComponent<PlantAttackController>();
                GameObject firePoint = new GameObject("FirePoint");
                firePoint.transform.SetParent(root.transform, false);
                firePoint.transform.localPosition = new Vector3(0.42f, 0.12f, 0f);
                SetField(attack, "firePoint", firePoint.transform);
                SetField(attack, "targetMask", LayerMaskEverything());
            }
            else if (role == PlantRole.Producer)
            {
                root.AddComponent<SunProducer>();
            }

            if (mine)
            {
                root.AddComponent<SporeMine>();
            }

            return SavePrefab(root, PlantPrefabPath + "/" + name + ".prefab");
        }

        private static GameObject CreateEnemyPrefab(string name, Sprite sprite, bool armor, bool reward, bool aura)
        {
            GameObject root = new GameObject(name);
            root.AddComponent<Enemy>();
            root.AddComponent<EnemyHealth>();
            root.AddComponent<EnemyMovement>();
            root.AddComponent<EnemyStatusController>();

            EnemyAttackController attack = root.AddComponent<EnemyAttackController>();
            SetField(attack, "plantMask", LayerMaskEverything());

            if (armor)
            {
                root.AddComponent<EnemyArmor>();
            }

            if (reward)
            {
                root.AddComponent<EnemyDeathReward>();
            }

            if (aura)
            {
                root.AddComponent<EnemyLaneAura>();
            }

            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 6;
            BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.75f, 0.85f);
            return SavePrefab(root, EnemyPrefabPath + "/" + name + ".prefab");
        }

        private static GameObject CreateProjectilePrefab(string name, Sprite sprite)
        {
            GameObject root = new GameObject(name);
            root.AddComponent<Projectile>();
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 8;
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.12f;
            return SavePrefab(root, ProjectilePrefabPath + "/" + name + ".prefab");
        }

        private static GameObject SavePrefab(GameObject instance, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            UnityEngine.Object.DestroyImmediate(instance);
            return prefab;
        }

        private static Sprite CreateSprite(string name, Color fill, Color border)
        {
            string texturePath = ArtPath + "/" + name + ".png";
            string absolutePath = AssetPathToAbsolutePath(texturePath);
            Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[64 * 64];

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    bool edge = x < 3 || y < 3 || x >= 61 || y >= 61;
                    pixels[y * 64 + x] = edge ? border : fill;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 64f;
                importer.filterMode = FilterMode.Point;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(parent))
            {
                parent = parent.Replace('\\', '/');
                EnsureFolder(parent);
            }

            string folder = Path.GetFileName(assetPath);
            string parentPath = string.IsNullOrEmpty(parent) ? "Assets" : parent;
            if (!AssetDatabase.IsValidFolder(assetPath))
            {
                AssetDatabase.CreateFolder(parentPath, folder);
            }
        }

        private static string AssetPathToAbsolutePath(string assetPath)
        {
            string relative = assetPath.Substring("Assets/".Length);
            return Path.Combine(Application.dataPath, relative);
        }

        private static void SetField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }

            field.SetValue(target, value);
        }

        private static string ToAssetName(string displayName)
        {
            return displayName.Replace(" ", string.Empty);
        }

        private static LayerMask LayerMaskEverything()
        {
            LayerMask mask = new LayerMask();
            mask.value = ~0;
            return mask;
        }

        private sealed class ContentSprites
        {
            public Sprite MistSprout;
            public Sprite ThornPod;
            public Sprite BloomBattery;
            public Sprite BarkBastion;
            public Sprite SporeMine;
            public Sprite Skitter;
            public Sprite Shellback;
            public Sprite Carrier;
            public Sprite Howler;
            public Sprite MistProjectile;
            public Sprite ThornProjectile;
            public Sprite SporeBurst;
            public Sprite Augment;
        }

        private sealed class ContentPrefabs
        {
            public GameObject MistSprout;
            public GameObject ThornPod;
            public GameObject BloomBattery;
            public GameObject BarkBastion;
            public GameObject SporeMine;
            public GameObject Skitter;
            public GameObject Shellback;
            public GameObject Carrier;
            public GameObject Howler;
            public GameObject MistProjectile;
            public GameObject ThornProjectile;
        }

        private sealed class ProjectileConfigs
        {
            public ProjectileConfig Mist;
            public ProjectileConfig Thorn;
        }

        private sealed class PlantConfigs
        {
            public PlantConfig Mist;
            public PlantConfig Thorn;
            public PlantConfig Battery;
            public PlantConfig Bastion;
            public PlantConfig Mine;
        }

        private sealed class EnemyConfigs
        {
            public EnemyConfig Skitter;
            public EnemyConfig Shellback;
            public EnemyConfig Carrier;
            public EnemyConfig Howler;
        }
    }
}
