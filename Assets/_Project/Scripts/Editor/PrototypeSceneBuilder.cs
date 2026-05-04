using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LawnDefense.CameraTools;
using LawnDefense.Combat;
using LawnDefense.Core;
using LawnDefense.Data;
using LawnDefense.Enemies;
using LawnDefense.Grid;
using LawnDefense.Placement;
using LawnDefense.Plants;
using LawnDefense.Sun;
using LawnDefense.UI;
using LawnDefense.Waves;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LawnDefense.EditorTools
{
    public static class PrototypeSceneBuilder
    {
        private const string ProjectRoot = "Assets/_Project";
        private const string ArtPath = ProjectRoot + "/Art/Placeholders";
        private const string DataPath = ProjectRoot + "/Data/Prototype";
        private const string PrefabPath = ProjectRoot + "/Prefabs/Prototype";
        private const string ScenePath = ProjectRoot + "/Scenes/Main.unity";

        [MenuItem("LawnDefense/Build Prototype Scene")]
        public static void BuildAndValidate()
        {
            EnsureProjectFolders();
            SpriteSet sprites = CreateSprites();
            PrefabSet prefabs = CreatePrefabs(sprites);
            ConfigSet configs = CreateConfigs(prefabs, sprites);
            BuildScene(configs, prefabs, sprites);
            ValidateGeneratedContent();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Prototype scene build and validation completed.");
        }

        [MenuItem("LawnDefense/Validate Prototype Content")]
        public static void ValidateGeneratedContent()
        {
            List<string> errors = new List<string>();

            LevelConfig level = AssetDatabase.LoadAssetAtPath<LevelConfig>(DataPath + "/PrototypeLevel.asset");
            if (level == null)
            {
                errors.Add("Missing PrototypeLevel.asset.");
            }
            else
            {
                if (level.AvailablePlants == null || level.AvailablePlants.Length < 3)
                {
                    errors.Add("PrototypeLevel must expose at least three plant configs.");
                }

                if (level.WaveConfig == null || level.WaveConfig.Entries == null || level.WaveConfig.Entries.Length == 0)
                {
                    errors.Add("PrototypeLevel must reference a non-empty WaveConfig.");
                }
            }

            ValidatePrefab<Plant>(PrefabPath + "/SproutBlaster.prefab", errors);
            ValidatePrefab<Plant>(PrefabPath + "/Sunbud.prefab", errors);
            ValidatePrefab<Plant>(PrefabPath + "/Stoneleaf.prefab", errors);
            ValidatePrefab<Enemy>(PrefabPath + "/MossWalker.prefab", errors);
            ValidatePrefab<Enemy>(PrefabPath + "/MossBrute.prefab", errors);
            ValidatePrefab<Projectile>(PrefabPath + "/SeedProjectile.prefab", errors);
            ValidatePrefab<SunCollectible>(PrefabPath + "/SunCollectible.prefab", errors);

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                errors.Add("Missing Main.unity scene.");
            }
            else
            {
                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RequireSceneComponent<GameBootstrap>(scene, errors);
                RequireSceneComponent<GridSystem>(scene, errors);
                RequireSceneComponent<SunSystem>(scene, errors);
                RequireSceneComponent<PlantPlacementSystem>(scene, errors);
                RequireSceneComponent<WaveSystem>(scene, errors);
                RequireSceneComponent<GameHudView>(scene, errors);
            }

            if (errors.Count > 0)
            {
                for (int i = 0; i < errors.Count; i++)
                {
                    Debug.LogError(errors[i]);
                }

                throw new InvalidOperationException("Prototype content validation failed with " + errors.Count + " issue(s).");
            }

            Debug.Log("Prototype content validation passed.");
        }

        private static void EnsureProjectFolders()
        {
            EnsureFolder(ProjectRoot);
            EnsureFolder(ProjectRoot + "/Art");
            EnsureFolder(ArtPath);
            EnsureFolder(ProjectRoot + "/Data");
            EnsureFolder(DataPath);
            EnsureFolder(ProjectRoot + "/Prefabs");
            EnsureFolder(PrefabPath);
            EnsureFolder(ProjectRoot + "/Scenes");
            EnsureFolder(ProjectRoot + "/Scripts/Editor");
        }

        private static SpriteSet CreateSprites()
        {
            return new SpriteSet
            {
                Cell = CreateSprite("GrassCell", new Color(0.28f, 0.62f, 0.28f), new Color(0.18f, 0.42f, 0.2f)),
                Shooter = CreateSprite("SproutBlaster", new Color(0.15f, 0.65f, 0.32f), new Color(0.06f, 0.34f, 0.17f)),
                Producer = CreateSprite("Sunbud", new Color(0.96f, 0.76f, 0.18f), new Color(0.85f, 0.42f, 0.1f)),
                Blocker = CreateSprite("Stoneleaf", new Color(0.42f, 0.54f, 0.5f), new Color(0.2f, 0.28f, 0.26f)),
                Enemy = CreateSprite("MossWalker", new Color(0.42f, 0.5f, 0.36f), new Color(0.24f, 0.28f, 0.22f)),
                Brute = CreateSprite("MossBrute", new Color(0.52f, 0.47f, 0.34f), new Color(0.26f, 0.22f, 0.15f)),
                Projectile = CreateSprite("SeedProjectile", new Color(0.38f, 0.82f, 0.24f), new Color(0.18f, 0.44f, 0.1f)),
                Sun = CreateSprite("SunCollectible", new Color(1f, 0.9f, 0.24f), new Color(1f, 0.5f, 0.05f))
            };
        }

        private static PrefabSet CreatePrefabs(SpriteSet sprites)
        {
            return new PrefabSet
            {
                Shooter = CreatePlantPrefab("SproutBlaster", sprites.Shooter, PlantRole.Shooter),
                Producer = CreatePlantPrefab("Sunbud", sprites.Producer, PlantRole.Producer),
                Blocker = CreatePlantPrefab("Stoneleaf", sprites.Blocker, PlantRole.Blocker),
                Enemy = CreateEnemyPrefab("MossWalker", sprites.Enemy, new Vector2(0.7f, 0.8f), 0.35f),
                Brute = CreateEnemyPrefab("MossBrute", sprites.Brute, new Vector2(0.85f, 0.95f), 0.42f),
                Projectile = CreateProjectilePrefab("SeedProjectile", sprites.Projectile),
                Sun = CreateSunPrefab("SunCollectible", sprites.Sun)
            };
        }

        private static ConfigSet CreateConfigs(PrefabSet prefabs, SpriteSet sprites)
        {
            ProjectileConfig projectile = LoadOrCreate<ProjectileConfig>(DataPath + "/SeedProjectile.asset");
            projectile.Id = "seed_projectile";
            projectile.Prefab = prefabs.Projectile;
            projectile.Speed = 7f;
            projectile.Damage = 25;
            projectile.HitRadius = 0.25f;
            projectile.CanPierce = false;
            projectile.MaxPierceCount = 0;
            EditorUtility.SetDirty(projectile);

            PlantConfig shooter = LoadOrCreate<PlantConfig>(DataPath + "/SproutBlaster.asset");
            shooter.Id = "sprout_blaster";
            shooter.DisplayName = "Sprout Blaster";
            shooter.Icon = sprites.Shooter;
            shooter.Prefab = prefabs.Shooter;
            shooter.SunCost = 50;
            shooter.Cooldown = 4f;
            shooter.MaxHealth = 100;
            shooter.Role = PlantRole.Shooter;
            shooter.AttackInterval = 1.2f;
            shooter.AttackRange = 9f;
            shooter.ProjectileConfig = projectile;
            EditorUtility.SetDirty(shooter);

            PlantConfig producer = LoadOrCreate<PlantConfig>(DataPath + "/Sunbud.asset");
            producer.Id = "sunbud";
            producer.DisplayName = "Sunbud";
            producer.Icon = sprites.Producer;
            producer.Prefab = prefabs.Producer;
            producer.SunCost = 25;
            producer.Cooldown = 5f;
            producer.MaxHealth = 80;
            producer.Role = PlantRole.Producer;
            producer.SunProduceInterval = 6f;
            producer.SunProduceAmount = 25;
            EditorUtility.SetDirty(producer);

            PlantConfig blocker = LoadOrCreate<PlantConfig>(DataPath + "/Stoneleaf.asset");
            blocker.Id = "stoneleaf";
            blocker.DisplayName = "Stoneleaf";
            blocker.Icon = sprites.Blocker;
            blocker.Prefab = prefabs.Blocker;
            blocker.SunCost = 50;
            blocker.Cooldown = 7f;
            blocker.MaxHealth = 320;
            blocker.Role = PlantRole.Blocker;
            EditorUtility.SetDirty(blocker);

            EnemyConfig walker = LoadOrCreate<EnemyConfig>(DataPath + "/MossWalker.asset");
            walker.Id = "moss_walker";
            walker.DisplayName = "Moss Walker";
            walker.Prefab = prefabs.Enemy;
            walker.MaxHealth = 90;
            walker.MoveSpeed = 0.45f;
            walker.AttackDamage = 20;
            walker.AttackInterval = 1f;
            walker.RewardSun = 0;
            walker.EnemyTags = new[] { "ground" };
            EditorUtility.SetDirty(walker);

            EnemyConfig brute = LoadOrCreate<EnemyConfig>(DataPath + "/MossBrute.asset");
            brute.Id = "moss_brute";
            brute.DisplayName = "Moss Brute";
            brute.Prefab = prefabs.Brute;
            brute.MaxHealth = 180;
            brute.MoveSpeed = 0.3f;
            brute.AttackDamage = 30;
            brute.AttackInterval = 1.2f;
            brute.RewardSun = 0;
            brute.EnemyTags = new[] { "ground", "tough" };
            EditorUtility.SetDirty(brute);

            WaveConfig wave = LoadOrCreate<WaveConfig>(DataPath + "/PrototypeWave.asset");
            wave.Entries = new[]
            {
                new WaveEntry { EnemyConfig = walker, SpawnTime = 1f, Count = 2, Interval = 3f, LaneMode = LaneMode.Specific, SpecificLane = 2 },
                new WaveEntry { EnemyConfig = walker, SpawnTime = 6f, Count = 4, Interval = 2.2f, LaneMode = LaneMode.RandomFromAllowed, AllowedLanes = new[] { 1, 2, 3 } },
                new WaveEntry { EnemyConfig = brute, SpawnTime = 14f, Count = 1, Interval = 1f, LaneMode = LaneMode.Specific, SpecificLane = 2, IsMajorWave = true }
            };
            EditorUtility.SetDirty(wave);

            LevelConfig level = LoadOrCreate<LevelConfig>(DataPath + "/PrototypeLevel.asset");
            level.Id = "prototype_level";
            level.DisplayName = "Prototype Lawn";
            level.Rows = 5;
            level.Columns = 9;
            level.InitialSun = 125;
            level.NaturalSunInterval = 6f;
            level.NaturalSunAmount = 25;
            level.AvailablePlants = new[] { shooter, producer, blocker };
            level.WaveConfig = wave;
            EditorUtility.SetDirty(level);

            return new ConfigSet
            {
                Projectile = projectile,
                Shooter = shooter,
                Producer = producer,
                Blocker = blocker,
                Walker = walker,
                Brute = brute,
                Wave = wave,
                Level = level
            };
        }

        private static void BuildScene(ConfigSet configs, PrefabSet prefabs, SpriteSet sprites)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = CreateCamera();
            GameObject root = new GameObject("GameRoot");
            PoolManager poolManager = root.AddComponent<PoolManager>();
            GridSystem gridSystem = root.AddComponent<GridSystem>();
            SunSystem sunSystem = root.AddComponent<SunSystem>();
            LaneTargetService targetService = root.AddComponent<LaneTargetService>();
            PlantPlacementSystem placementSystem = root.AddComponent<PlantPlacementSystem>();
            GameStateController gameStateController = root.AddComponent<GameStateController>();
            WaveSystem waveSystem = root.AddComponent<WaveSystem>();
            CameraFitController cameraFit = root.AddComponent<CameraFitController>();
            GameBootstrap bootstrap = root.AddComponent<GameBootstrap>();

            SetField(gridSystem, "origin", new Vector2(-4f, -2f));
            SetField(gridSystem, "cellSize", new Vector2(1f, 1f));

            SetField(sunSystem, "poolManager", poolManager);
            SetField(sunSystem, "sunPrefab", prefabs.Sun);
            SetField(sunSystem, "spawnXRange", new Vector2(-3.8f, 3.8f));
            SetField(sunSystem, "spawnY", 3.4f);
            SetField(sunSystem, "stopYMin", -1.6f);
            SetField(sunSystem, "stopYMax", 2.3f);

            SetField(placementSystem, "worldCamera", camera);
            SetField(placementSystem, "gridSystem", gridSystem);
            SetField(placementSystem, "sunSystem", sunSystem);
            SetField(placementSystem, "poolManager", poolManager);
            SetField(placementSystem, "targetService", targetService);

            SetField(waveSystem, "poolManager", poolManager);
            SetField(waveSystem, "targetService", targetService);
            SetField(waveSystem, "gridSystem", gridSystem);
            SetField(waveSystem, "gameStateController", gameStateController);
            SetField(waveSystem, "spawnXOffset", 1.5f);
            SetField(waveSystem, "defeatXOffset", 1.2f);

            SetField(cameraFit, "targetCamera", camera);
            SetField(cameraFit, "gridSystem", gridSystem);
            SetField(cameraFit, "padding", 1.2f);

            SetField(bootstrap, "levelConfig", configs.Level);
            SetField(bootstrap, "gridSystem", gridSystem);
            SetField(bootstrap, "sunSystem", sunSystem);
            SetField(bootstrap, "plantPlacementSystem", placementSystem);
            SetField(bootstrap, "waveSystem", waveSystem);
            SetField(bootstrap, "cameraFitController", cameraFit);
            SetField(bootstrap, "gameStateController", gameStateController);

            CreateGridVisuals(sprites.Cell);
            CreateHud(configs, placementSystem);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.18f, 0.16f);
            camera.orthographic = true;
            camera.orthographicSize = 4f;
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static void CreateGridVisuals(Sprite cellSprite)
        {
            GameObject parent = new GameObject("GridVisuals");
            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 9; column++)
                {
                    GameObject cell = new GameObject("Cell_" + row + "_" + column);
                    cell.transform.SetParent(parent.transform, false);
                    cell.transform.position = new Vector3(-4f + column, -2f + row, 0.2f);
                    cell.transform.localScale = new Vector3(0.96f, 0.96f, 1f);

                    SpriteRenderer renderer = cell.AddComponent<SpriteRenderer>();
                    renderer.sprite = cellSprite;
                    renderer.sortingOrder = -10;
                    renderer.color = (row + column) % 2 == 0
                        ? Color.white
                        : new Color(0.88f, 1f, 0.88f, 1f);
                }
            }
        }

        private static void CreateHud(ConfigSet configs, PlantPlacementSystem placementSystem)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            GameObject hud = new GameObject("HUD");
            hud.transform.SetParent(canvasObject.transform, false);
            RectTransform hudRect = hud.AddComponent<RectTransform>();
            Stretch(hudRect);
            GameHudView hudView = hud.AddComponent<GameHudView>();

            Text sunText = CreateText("SunText", hud.transform, font, "125", 28, TextAnchor.MiddleLeft);
            SetAnchor(sunText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(80f, -34f), new Vector2(120f, 44f));

            Text sunIcon = CreateText("SunIcon", hud.transform, font, "Sun", 20, TextAnchor.MiddleLeft);
            SetAnchor(sunIcon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -34f), new Vector2(70f, 40f));

            Slider progress = CreateSlider("WaveProgress", hud.transform);
            SetAnchor(progress.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-180f, -34f), new Vector2(280f, 24f));

            GameObject resultPanel = CreatePanel("ResultPanel", hud.transform, new Color(0f, 0f, 0f, 0.72f));
            RectTransform resultRect = resultPanel.GetComponent<RectTransform>();
            SetAnchor(resultRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360f, 160f));
            resultPanel.SetActive(false);

            Text resultText = CreateText("ResultText", resultPanel.transform, font, "Victory", 38, TextAnchor.MiddleCenter);
            Stretch(resultText.rectTransform);

            GameObject cardBar = new GameObject("PlantCards");
            cardBar.transform.SetParent(hud.transform, false);
            RectTransform cardBarRect = cardBar.AddComponent<RectTransform>();
            SetAnchor(cardBarRect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(208f, 48f), new Vector2(390f, 92f));

            CreatePlantCard(configs.Shooter, placementSystem, cardBar.transform, font, new Vector2(-130f, 0f));
            CreatePlantCard(configs.Producer, placementSystem, cardBar.transform, font, Vector2.zero);
            CreatePlantCard(configs.Blocker, placementSystem, cardBar.transform, font, new Vector2(130f, 0f));

            SetField(hudView, "sunText", sunText);
            SetField(hudView, "waveProgressSlider", progress);
            SetField(hudView, "resultPanel", resultPanel);
            SetField(hudView, "resultText", resultText);
        }

        private static void CreatePlantCard(
            PlantConfig config,
            PlantPlacementSystem placementSystem,
            Transform parent,
            Font font,
            Vector2 anchoredPosition)
        {
            GameObject card = new GameObject(config.Id + "_Card");
            card.transform.SetParent(parent, false);
            RectTransform rect = card.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(116f, 76f);
            rect.anchoredPosition = anchoredPosition;

            Image background = card.AddComponent<Image>();
            background.color = new Color(0.12f, 0.18f, 0.15f, 0.92f);
            Button button = card.AddComponent<Button>();
            PlantCardView cardView = card.AddComponent<PlantCardView>();

            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(card.transform, false);
            Image icon = iconObject.AddComponent<Image>();
            icon.sprite = config.Icon;
            icon.preserveAspect = true;
            SetAnchor(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(34f, 8f), new Vector2(48f, 48f));

            Text cost = CreateText("Cost", card.transform, font, config.SunCost.ToString(), 18, TextAnchor.MiddleCenter);
            SetAnchor(cost.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-32f, 8f), new Vector2(46f, 30f));

            GameObject cooldownObject = new GameObject("CooldownFill");
            cooldownObject.transform.SetParent(card.transform, false);
            Image cooldown = cooldownObject.AddComponent<Image>();
            cooldown.color = new Color(0f, 0f, 0f, 0.52f);
            cooldown.type = Image.Type.Filled;
            cooldown.fillMethod = Image.FillMethod.Vertical;
            cooldown.fillOrigin = (int)Image.OriginVertical.Bottom;
            Stretch(cooldown.rectTransform);

            Text label = CreateText("Label", card.transform, font, config.DisplayName, 12, TextAnchor.MiddleCenter);
            SetAnchor(label.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(108f, 20f));

            SetField(cardView, "config", config);
            SetField(cardView, "placementSystem", placementSystem);
            SetField(cardView, "iconImage", icon);
            SetField(cardView, "costText", cost);
            SetField(cardView, "cooldownFill", cooldown);
            SetField(cardView, "button", button);
        }

        private static GameObject CreatePlantPrefab(string name, Sprite sprite, PlantRole role)
        {
            GameObject root = new GameObject(name);
            root.AddComponent<Plant>();
            root.AddComponent<PlantHealth>();
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 5;

            BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.7f, 0.8f);

            if (role == PlantRole.Shooter)
            {
                PlantAttackController attack = root.AddComponent<PlantAttackController>();
                GameObject firePoint = new GameObject("FirePoint");
                firePoint.transform.SetParent(root.transform, false);
                firePoint.transform.localPosition = new Vector3(0.42f, 0.12f, 0f);
                SetField(attack, "firePoint", firePoint.transform);
                SetField(attack, "targetMask", LayerMaskEverything());
                SetField(attack, "targetProbeHeight", 0.12f);
            }
            else if (role == PlantRole.Producer)
            {
                root.AddComponent<SunProducer>();
            }

            return SavePrefab(root, PrefabPath + "/" + name + ".prefab");
        }

        private static GameObject CreateEnemyPrefab(string name, Sprite sprite, Vector2 colliderSize, float probeDistance)
        {
            GameObject root = new GameObject(name);
            root.AddComponent<Enemy>();
            root.AddComponent<EnemyHealth>();
            root.AddComponent<EnemyMovement>();
            EnemyAttackController attack = root.AddComponent<EnemyAttackController>();
            SetField(attack, "plantMask", LayerMaskEverything());
            SetField(attack, "attackProbeDistance", probeDistance);
            SetField(attack, "attackProbeSize", new Vector2(0.45f, 0.7f));

            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 6;

            BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
            collider.size = colliderSize;
            return SavePrefab(root, PrefabPath + "/" + name + ".prefab");
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
            return SavePrefab(root, PrefabPath + "/" + name + ".prefab");
        }

        private static GameObject CreateSunPrefab(string name, Sprite sprite)
        {
            GameObject root = new GameObject(name);
            root.AddComponent<SunCollectible>();
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 20;

            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.32f;
            return SavePrefab(root, PrefabPath + "/" + name + ".prefab");
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
            string absoluteTexturePath = AssetPathToAbsolutePath(texturePath);
            int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = x < 3 || y < 3 || x >= size - 3 || y >= size - 3;
                    pixels[y * size + x] = isBorder ? border : fill;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(absoluteTexturePath, texture.EncodeToPNG());
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

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            panel.AddComponent<RectTransform>();
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Text CreateText(string name, Transform parent, Font font, string text, int size, TextAnchor anchor)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            Text label = textObject.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.alignment = anchor;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }

        private static Slider CreateSlider(string name, Transform parent)
        {
            GameObject sliderObject = new GameObject(name);
            sliderObject.transform.SetParent(parent, false);
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;

            GameObject background = CreatePanel("Background", sliderObject.transform, new Color(0f, 0f, 0f, 0.45f));
            Stretch(background.GetComponent<RectTransform>());

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            Stretch(fillAreaRect);

            GameObject fill = CreatePanel("Fill", fillArea.transform, new Color(0.95f, 0.72f, 0.18f, 1f));
            Stretch(fill.GetComponent<RectTransform>());

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.targetGraphic = background.GetComponent<Image>();
            return slider;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetAnchor(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
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

            string folderName = Path.GetFileName(assetPath);
            string parentPath = string.IsNullOrEmpty(parent) ? "Assets" : parent;
            if (!AssetDatabase.IsValidFolder(assetPath))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        private static string AssetPathToAbsolutePath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                throw new ArgumentException("Expected an Assets-relative path.", "assetPath");
            }

            string relativeToAssets = assetPath.Substring("Assets/".Length);
            return Path.Combine(Application.dataPath, relativeToAssets);
        }

        private static void SetField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }

            field.SetValue(target, value);
            UnityEngine.Object unityObject = target as UnityEngine.Object;
            if (unityObject != null)
            {
                EditorUtility.SetDirty(unityObject);
            }
        }

        private static LayerMask LayerMaskEverything()
        {
            LayerMask mask = new LayerMask();
            mask.value = ~0;
            return mask;
        }

        private static void ValidatePrefab<T>(string path, List<string> errors) where T : Component
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                errors.Add("Missing prefab: " + path);
                return;
            }

            if (prefab.GetComponentInChildren<T>(true) == null)
            {
                errors.Add("Prefab lacks component " + typeof(T).Name + ": " + path);
            }
        }

        private static void RequireSceneComponent<T>(Scene scene, List<string> errors) where T : Component
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].GetComponentInChildren<T>(true) != null)
                {
                    return;
                }
            }

            errors.Add("Scene lacks component " + typeof(T).Name + ".");
        }

        private sealed class SpriteSet
        {
            public Sprite Cell;
            public Sprite Shooter;
            public Sprite Producer;
            public Sprite Blocker;
            public Sprite Enemy;
            public Sprite Brute;
            public Sprite Projectile;
            public Sprite Sun;
        }

        private sealed class PrefabSet
        {
            public GameObject Shooter;
            public GameObject Producer;
            public GameObject Blocker;
            public GameObject Enemy;
            public GameObject Brute;
            public GameObject Projectile;
            public GameObject Sun;
        }

        private sealed class ConfigSet
        {
            public ProjectileConfig Projectile;
            public PlantConfig Shooter;
            public PlantConfig Producer;
            public PlantConfig Blocker;
            public EnemyConfig Walker;
            public EnemyConfig Brute;
            public WaveConfig Wave;
            public LevelConfig Level;
        }
    }
}
