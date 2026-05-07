using System.Collections.Generic;
using EvoTowers.EditorTools;
using EvoTowers.Task1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EvoTowers.Task1.Editor
{
    public static class Task1SceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Battle.unity";
        private const string DataFolder = "Assets/Resources/Week2Data";

        [MenuItem("EvoTowers/Task3/Create Week3 Battle Scene")]
        public static void CreateWeek3BattleScene()
        {
            CreateWeek2BattleScene();
        }

        [MenuItem("EvoTowers/Task2/Create Week2 Battle Scene")]
        public static void CreateWeek2BattleScene()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject gameRoot = new GameObject("GameRoot");
            GameObject mapRoot = new GameObject("MapRoot");
            GameObject pathRoot = new GameObject("PathRoot");
            GameObject slotRoot = new GameObject("BuildSlotRoot");
            GameObject managers = new GameObject("Managers");
            GameObject enemyRoot = new GameObject("EnemyRoot");

            mapRoot.transform.SetParent(gameRoot.transform);
            pathRoot.transform.SetParent(gameRoot.transform);
            slotRoot.transform.SetParent(gameRoot.transform);
            managers.transform.SetParent(gameRoot.transform);
            enemyRoot.transform.SetParent(gameRoot.transform);

            CreateCamera();
            CreateEventSystem();
            CreateMap(mapRoot.transform);
            PathRoute route = CreatePath(pathRoot.transform);
            CreateBuildSlots(slotRoot.transform);

            GameManager gameManager = managers.AddComponent<GameManager>();
            BuildManager buildManager = managers.AddComponent<BuildManager>();
            managers.AddComponent<GameAudio>();
            managers.AddComponent<TowerSelectionManager>();
            WaveManager waveManager = managers.AddComponent<WaveManager>();
            Week2BattleFlow battleFlow = managers.AddComponent<Week2BattleFlow>();

            SerializedObject gameManagerSo = new SerializedObject(gameManager);
            ConfigureGameManager(gameManagerSo);
            gameManagerSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject waveManagerSo = new SerializedObject(waveManager);
            waveManagerSo.FindProperty("route").objectReferenceValue = route;
            waveManagerSo.FindProperty("enemyPrefab").objectReferenceValue = CreateEnemyPrefab();
            waveManagerSo.FindProperty("enemyRoot").objectReferenceValue = enemyRoot.transform;
            ConfigureWaves(waveManagerSo);
            waveManagerSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject battleFlowSo = new SerializedObject(battleFlow);
            battleFlowSo.FindProperty("waveManager").objectReferenceValue = waveManager;
            battleFlowSo.ApplyModifiedPropertiesWithoutUndo();

            _ = buildManager;
            CreateHud(waveManager, battleFlow);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeObject = gameRoot;
            Debug.Log($"Created Week 2 battle scene at {ScenePath}.");
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.15f;
            camera.backgroundColor = new Color(0.08f, 0.12f, 0.12f);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0.15f, -10f);
        }

        private static void CreateEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static void CreateMap(Transform parent)
        {
            Sprite backgroundBack = LoadSprite("Assets/Arts/Pictures/NormalMordel/Game/1/BG0.PNG");
            Sprite backgroundFront = LoadSprite("Assets/Arts/Pictures/NormalMordel/Game/1/BG1.PNG");

            CreateMapLayer(parent, "BackgroundBack", backgroundBack, -20, new Vector3(2.6f, 2.6f, 1f), new Color(0.18f, 0.34f, 0.25f));
            CreateMapLayer(parent, "BackgroundFront", backgroundFront, -12, new Vector3(2.6f, 2.6f, 1f), Color.white);

            CreateDecoration(parent, "RockCluster_LeftTop", "Assets/Arts/Pictures/NormalMordel/Game/1/Items/Object01-hd_4.PNG", new Vector3(-6.8f, 4.2f, 0f), new Vector3(0.95f, 0.95f, 1f), -11);
            CreateDecoration(parent, "Mushroom_Left", "Assets/Arts/Pictures/NormalMordel/Game/1/Items/Object01-hd_0.PNG", new Vector3(-6.5f, 1.0f, 0f), new Vector3(0.8f, 0.8f, 1f), -11);
            CreateDecoration(parent, "TreeCenter", "Assets/Arts/Pictures/NormalMordel/Game/1/Items/Object01-hd_1.PNG", new Vector3(-0.1f, 2.65f, 0f), new Vector3(1.0f, 1.0f, 1f), -11);
            CreateDecoration(parent, "StoneCorner", "Assets/Arts/Pictures/NormalMordel/Game/1/Items/Object01-hd_5.PNG", new Vector3(1.8f, 0.35f, 0f), new Vector3(0.95f, 0.95f, 1f), -11);
            CreateDecoration(parent, "StumpBottom", "Assets/Arts/Pictures/NormalMordel/Game/1/Items/Object01-hd_6.PNG", new Vector3(3.9f, -2.6f, 0f), new Vector3(0.85f, 0.85f, 1f), -11);
            CreateDecoration(parent, "FlowerRight", "Assets/Arts/Pictures/NormalMordel/Game/1/Items/Object01-hd_8.PNG", new Vector3(5.8f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 1f), -11);

            CreatePathOverlay(parent);
        }

        private static void CreateMapLayer(Transform parent, string name, Sprite sprite, int sortingOrder, Vector3 scale, Color fallbackColor)
        {
            GameObject layerObject = new GameObject(name);
            layerObject.transform.SetParent(parent, false);
            SpriteRenderer renderer = layerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite != null ? sprite : CreateFallbackSprite();
            renderer.color = sprite != null ? Color.white : fallbackColor;
            renderer.sortingOrder = sortingOrder;
            layerObject.transform.localScale = scale;
        }

        private static void CreateDecoration(Transform parent, string name, string spritePath, Vector3 position, Vector3 scale, int sortingOrder)
        {
            Sprite sprite = LoadSprite(spritePath);
            if (sprite == null)
            {
                return;
            }

            GameObject decoration = new GameObject(name);
            decoration.transform.SetParent(parent, false);
            decoration.transform.localPosition = position;
            decoration.transform.localScale = scale;

            SpriteRenderer renderer = decoration.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
        }

        private static void CreatePathOverlay(Transform parent)
        {
            Vector3[] routePoints =
            {
                new Vector3(-7.2f, 3.2f, 0f),
                new Vector3(-4.8f, 3.2f, 0f),
                new Vector3(-4.8f, 1.1f, 0f),
                new Vector3(-1.2f, 1.1f, 0f),
                new Vector3(-1.2f, -1.5f, 0f),
                new Vector3(3.8f, -1.5f, 0f),
                new Vector3(6.7f, -3.1f, 0f)
            };

            for (int i = 0; i < routePoints.Length - 1; i++)
            {
                GameObject segment = new GameObject($"PathOverlay_{i + 1:00}");
                segment.transform.SetParent(parent, false);
                LineRenderer line = segment.AddComponent<LineRenderer>();
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.useWorldSpace = false;
                line.positionCount = 2;
                line.startWidth = 0.88f;
                line.endWidth = 0.88f;
                line.startColor = new Color(0.63f, 0.48f, 0.3f, 0.55f);
                line.endColor = new Color(0.63f, 0.48f, 0.3f, 0.55f);
                line.sortingOrder = -13;
                line.numCapVertices = 8;
                line.SetPosition(0, routePoints[i]);
                line.SetPosition(1, routePoints[i + 1]);
            }
        }

        private static PathRoute CreatePath(Transform parent)
        {
            Vector3[] points =
            {
                new Vector3(-7.2f, 3.2f, 0f),
                new Vector3(-4.8f, 3.2f, 0f),
                new Vector3(-4.8f, 1.1f, 0f),
                new Vector3(-1.2f, 1.1f, 0f),
                new Vector3(-1.2f, -1.5f, 0f),
                new Vector3(3.8f, -1.5f, 0f),
                new Vector3(6.7f, -3.1f, 0f)
            };

            List<Transform> waypointTransforms = new List<Transform>();
            for (int i = 0; i < points.Length; i++)
            {
                GameObject waypoint = new GameObject($"Waypoint_{i + 1:00}");
                waypoint.transform.SetParent(parent);
                waypoint.transform.position = points[i];
                waypointTransforms.Add(waypoint.transform);
            }

            GameObject spawn = new GameObject("EnemySpawnPoint");
            spawn.transform.SetParent(parent);
            spawn.transform.position = points[0];

            GameObject goal = new GameObject("GoalPoint");
            goal.transform.SetParent(parent);
            goal.transform.position = points[points.Length - 1];

            PathRoute route = parent.gameObject.AddComponent<PathRoute>();
            route.SetWaypoints(waypointTransforms);
            return route;
        }

        private static void CreateBuildSlots(Transform parent)
        {
            Vector3[] slots =
            {
                new Vector3(-6.4f, 2.2f, 0f),
                new Vector3(-5.9f, 4.2f, 0f),
                new Vector3(-5.7f, 0.0f, 0f),
                new Vector3(-3.8f, 1.9f, 0f),
                new Vector3(-2.6f, 0.1f, 0f),
                new Vector3(0.0f, 2.0f, 0f),
                new Vector3(0.2f, -0.8f, 0f),
                new Vector3(2.3f, -0.4f, 0f),
                new Vector3(4.2f, -2.6f, 0f),
                new Vector3(5.4f, -1.0f, 0f)
            };

            Sprite gridSprite = LoadSprite("Assets/Arts/Pictures/NormalMordel/Game/Tower/1/CanClick0.PNG");
            GameObject buildEffect = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Arts/Prefabs/Game/BuildEffect.prefab");

            for (int i = 0; i < slots.Length; i++)
            {
                GameObject slot = new GameObject($"BuildSlot_{i + 1:00}");
                slot.transform.SetParent(parent, false);
                slot.transform.localPosition = slots[i];

                SpriteRenderer renderer = slot.AddComponent<SpriteRenderer>();
                renderer.sprite = gridSprite != null ? gridSprite : CreateFallbackSprite();
                renderer.sortingOrder = -8;
                renderer.color = new Color(1f, 1f, 1f, 0.95f);
                slot.transform.localScale = Vector3.one * 0.78f;

                CircleCollider2D collider = slot.AddComponent<CircleCollider2D>();
                collider.radius = 0.48f;

                BuildSlot buildSlot = slot.AddComponent<BuildSlot>();
                SerializedObject so = new SerializedObject(buildSlot);
                so.FindProperty("markerRenderer").objectReferenceValue = renderer;
                so.FindProperty("buildEffectPrefab").objectReferenceValue = buildEffect;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static GameObject CreateEnemyPrefab()
        {
            const string prefabPath = "Assets/Resources/Task1Enemy.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            GameObject enemy = new GameObject("Task1Enemy");
            SpriteRenderer renderer = enemy.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 10;
            renderer.sprite = LoadSprite("Assets/Arts/Pictures/NormalMordel/Game/1/Monster/1-1.PNG") ?? CreateFallbackSprite();

            enemy.AddComponent<EnemyPathFollower>();
            EnemyHealth health = enemy.AddComponent<EnemyHealth>();

            GameObject healthRoot = new GameObject("HealthBar");
            healthRoot.transform.SetParent(enemy.transform, false);
            healthRoot.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            healthRoot.transform.localScale = Vector3.one;

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(healthRoot.transform, false);
            LineRenderer background = backgroundObject.AddComponent<LineRenderer>();
            ConfigureHealthLine(background, new Color(0.05f, 0.05f, 0.05f, 0.85f), 0.075f, 29);
            background.SetPosition(0, new Vector3(-0.3f, 0f, 0f));
            background.SetPosition(1, new Vector3(0.3f, 0f, 0f));

            GameObject fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(healthRoot.transform, false);
            LineRenderer fill = fillObject.AddComponent<LineRenderer>();
            ConfigureHealthLine(fill, new Color(0.2f, 0.95f, 0.25f, 0.95f), 0.055f, 30);
            fill.SetPosition(0, new Vector3(-0.28f, 0f, 0f));
            fill.SetPosition(1, new Vector3(0.28f, 0f, 0f));

            SerializedObject so = new SerializedObject(health);
            so.FindProperty("healthFill").objectReferenceValue = fill;
            so.FindProperty("spriteRenderer").objectReferenceValue = renderer;
            so.FindProperty("deathEffectPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Arts/Prefabs/Game/DestoryEffect.prefab");
            so.ApplyModifiedPropertiesWithoutUndo();

            EnsureFolder("Assets/Resources");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, prefabPath);
            Object.DestroyImmediate(enemy);
            return prefab;
        }

        private static void ConfigureHealthLine(LineRenderer line, Color color, float width, int sortingOrder)
        {
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.numCapVertices = 2;
            line.sortingOrder = sortingOrder;
            line.material = new Material(Shader.Find("Sprites/Default"));
        }

        private static void ConfigureGameManager(SerializedObject so)
        {
            EnsureFolder(DataFolder);
            so.FindProperty("startingGold").intValue = 180;
            so.FindProperty("startingLives").intValue = 20;

            LubanTowerConfigImporter.ApplyTowerConfigs(so);

            SerializedProperty enemies = so.FindProperty("enemyConfigs");
            enemies.arraySize = 5;
            SetEnemy(enemies.GetArrayElementAtIndex(0), EnemyType.Basic, "Grunt", 100f, 1.0f, 5, 1, 0f, false, false,
                LoadSprite("Assets/Arts/Pictures/NormalMordel/Game/1/Monster/1-1.PNG"), new Vector2(0.75f, 0.75f));
            SetEnemy(enemies.GetArrayElementAtIndex(1), EnemyType.Fast, "Runner", 70f, 1.6f, 6, 1, 0f, false, false,
                LoadSprite("Assets/Arts/Pictures/NormalMordel/Game/1/Monster/2-1.PNG"), new Vector2(0.65f, 0.65f));
            SetEnemy(enemies.GetArrayElementAtIndex(2), EnemyType.Armored, "Armored", 260f, 0.75f, 10, 2, 0.2f, false, false,
                LoadSprite("Assets/Arts/Pictures/NormalMordel/Game/1/Monster/3-1.PNG"), new Vector2(0.9f, 0.9f));
            SetEnemy(enemies.GetArrayElementAtIndex(3), EnemyType.Healer, "Healer", 120f, 0.9f, 12, 1, 0f, true, false,
                LoadSprite("Assets/Arts/Pictures/NormalMordel/Game/1/Monster/4-1.PNG"), new Vector2(0.8f, 0.8f));
            SetEnemy(enemies.GetArrayElementAtIndex(4), EnemyType.Boss, "Molten Behemoth", 3800f, 0.45f, 0, 10, 0.15f, false, true,
                LoadSprite("Assets/Arts/Pictures/NormalMordel/Game/1/Monster/12-1.PNG"), new Vector2(1.45f, 1.45f));

            SerializedProperty commanders = so.FindProperty("commanderDefinitions");
            List<CommanderDefinition> commanderDefinitions = EnsureCommanderDefinitions();
            commanders.arraySize = commanderDefinitions.Count;
            for (int i = 0; i < commanderDefinitions.Count; i++)
            {
                commanders.GetArrayElementAtIndex(i).objectReferenceValue = commanderDefinitions[i];
            }

            SerializedProperty upgrades = so.FindProperty("upgradeDefinitions");
            List<UpgradeDefinition> upgradeDefinitions = EnsureUpgradeDefinitions();
            upgrades.arraySize = upgradeDefinitions.Count;
            for (int i = 0; i < upgradeDefinitions.Count; i++)
            {
                upgrades.GetArrayElementAtIndex(i).objectReferenceValue = upgradeDefinitions[i];
            }

            ConfigureEvolutions(so.FindProperty("towerEvolutions"));
        }

        private static void SetEnemy(SerializedProperty property, EnemyType type, string name, float health, float speed, int reward, int leak, float armor, bool healer, bool boss, Sprite sprite, Vector2 scale)
        {
            property.FindPropertyRelative("type").enumValueIndex = (int)type;
            property.FindPropertyRelative("displayName").stringValue = name;
            property.FindPropertyRelative("maxHealth").floatValue = health;
            property.FindPropertyRelative("moveSpeed").floatValue = speed;
            property.FindPropertyRelative("goldReward").intValue = reward;
            property.FindPropertyRelative("lifeDamage").intValue = leak;
            property.FindPropertyRelative("armorPercent").floatValue = armor;
            property.FindPropertyRelative("isElite").boolValue = type == EnemyType.Armored || type == EnemyType.Boss;
            property.FindPropertyRelative("isBoss").boolValue = boss;
            property.FindPropertyRelative("isHealer").boolValue = healer;
            property.FindPropertyRelative("healRange").floatValue = 1.45f;
            property.FindPropertyRelative("healInterval").floatValue = 3f;
            property.FindPropertyRelative("healAmount").floatValue = 20f;
            property.FindPropertyRelative("sprite").objectReferenceValue = sprite;
            property.FindPropertyRelative("visualScale").vector2Value = scale;
        }

        private static void ConfigureEvolutions(SerializedProperty evolutions)
        {
            evolutions.arraySize = 6;
            SetEvolution(evolutions.GetArrayElementAtIndex(0), "archer_sniper", TowerType.Arrow, TowerBranchId.Sniper, "Sniper Tower", "Single, High DMG, Elite", "Charges briefly, prefers high-health enemies, and deals extra damage to elites.", 120, 2.5f, 0.55f, 1.2f, TowerTargetPriority.MarkedThenHighestHealth, new Color(1f, 0.9f, 0.35f, 1f), new[] { TowerTag.Physical, TowerTag.Archer, TowerTag.SingleTarget }, eliteBonus: 0.3f, charge: 0.22f);
            SetEvolution(evolutions.GetArrayElementAtIndex(1), "archer_rapid", TowerType.Arrow, TowerBranchId.RapidShot, "Rapid Shot Tower", "Fast, Combo, Clear", "Fires rapidly and gains attack speed while staying on the same target.", 110, 0.7f, 2.2f, 0.9f, TowerTargetPriority.MarkedThenClosestToGoal, new Color(0.55f, 1f, 0.58f, 1f), new[] { TowerTag.Physical, TowerTag.Archer, TowerTag.SingleTarget }, comboStacks: 5, comboSpeed: 0.06f);
            SetEvolution(evolutions.GetArrayElementAtIndex(2), "flame_blast", TowerType.Flame, TowerBranchId.BlastFlame, "Blast Flame Tower", "Area, Burst, Splash", "Creates a large explosion with center-to-edge damage falloff.", 130, 1.8f, 0.7f, 1f, TowerTargetPriority.ClosestToGoal, new Color(1f, 0.34f, 0.06f, 1f), new[] { TowerTag.Magic, TowerTag.Fire, TowerTag.Area }, explosionRadius: 1.4f, edge: 0.6f);
            SetEvolution(evolutions.GetArrayElementAtIndex(3), "flame_burning", TowerType.Flame, TowerBranchId.Burning, "Burning Tower", "DOT, Fire, Refresh", "Applies refreshable burning damage over time with capped stacks.", 120, 0.75f, 1.1f, 1f, TowerTargetPriority.ClosestToGoal, new Color(1f, 0.55f, 0.18f, 1f), new[] { TowerTag.Magic, TowerTag.Fire, TowerTag.DamageOverTime }, burnDuration: 4f, burnDamage: 0.25f, burnStacks: 3);
            SetEvolution(evolutions.GetArrayElementAtIndex(4), "magic_frost", TowerType.Magic, TowerBranchId.Frost, "Frost Tower", "Control, Slow, Splash", "Deals lighter damage and refreshes a small splash slow.", 110, 0.65f, 0.9f, 1.1f, TowerTargetPriority.ClosestToGoal, new Color(0.38f, 0.78f, 1f, 1f), new[] { TowerTag.Magic, TowerTag.Ice, TowerTag.Control }, slow: 0.3f, slowDuration: 2f, splash: 0.75f);
            SetEvolution(evolutions.GetArrayElementAtIndex(5), "magic_chain", TowerType.Magic, TowerBranchId.ChainLightning, "Chain Lightning Tower", "Bounce, Group, Lightning", "Bounces through nearby enemies without repeating targets.", 125, 0.9f, 1f, 1f, TowerTargetPriority.ClosestToGoal, new Color(0.7f, 0.85f, 1f, 1f), new[] { TowerTag.Magic, TowerTag.Lightning, TowerTag.Area }, chainTargets: 3, chainFalloff: 0.2f, chainRange: 1.45f);
        }

        private static void SetEvolution(SerializedProperty property, string id, TowerType baseType, TowerBranchId branch, string name, string tags, string description, int cost, float damage, float attackSpeed, float range, TowerTargetPriority priority, Color color, TowerTag[] tagList, float eliteBonus = 0f, float explosionRadius = 1f, float edge = 0.6f, float burnDuration = 0f, float burnDamage = 0f, int burnStacks = 1, float slow = 0f, float slowDuration = 0f, float splash = 0f, int comboStacks = 0, float comboSpeed = 0f, int chainTargets = 0, float chainFalloff = 0f, float chainRange = 0f, float charge = 0f)
        {
            property.FindPropertyRelative("id").stringValue = id;
            property.FindPropertyRelative("baseTowerType").enumValueIndex = (int)baseType;
            property.FindPropertyRelative("branchId").enumValueIndex = (int)branch;
            property.FindPropertyRelative("displayName").stringValue = name;
            property.FindPropertyRelative("gameplayTags").stringValue = tags;
            property.FindPropertyRelative("description").stringValue = description;
            property.FindPropertyRelative("evolveCost").intValue = cost;
            property.FindPropertyRelative("requiredLevel").intValue = 3;
            property.FindPropertyRelative("requiredExperience").intValue = 100;
            property.FindPropertyRelative("damageMultiplier").floatValue = damage;
            property.FindPropertyRelative("attackSpeedMultiplier").floatValue = attackSpeed;
            property.FindPropertyRelative("rangeMultiplier").floatValue = range;
            property.FindPropertyRelative("targetPriority").enumValueIndex = (int)priority;
            property.FindPropertyRelative("accentColor").colorValue = color;
            SerializedProperty tagsProperty = property.FindPropertyRelative("tags");
            tagsProperty.arraySize = tagList.Length;
            for (int i = 0; i < tagList.Length; i++)
            {
                tagsProperty.GetArrayElementAtIndex(i).enumValueIndex = (int)tagList[i];
            }

            property.FindPropertyRelative("eliteBonusDamage").floatValue = eliteBonus;
            property.FindPropertyRelative("explosionRadiusMultiplier").floatValue = explosionRadius;
            property.FindPropertyRelative("explosionEdgeDamagePercent").floatValue = edge;
            property.FindPropertyRelative("burnDuration").floatValue = burnDuration;
            property.FindPropertyRelative("burnDamageMultiplier").floatValue = burnDamage;
            property.FindPropertyRelative("maxBurnStacks").intValue = burnStacks;
            property.FindPropertyRelative("slowPercent").floatValue = slow;
            property.FindPropertyRelative("slowDuration").floatValue = slowDuration;
            property.FindPropertyRelative("splashRadius").floatValue = splash;
            property.FindPropertyRelative("maxComboStacks").intValue = comboStacks;
            property.FindPropertyRelative("comboAttackSpeedPerStack").floatValue = comboSpeed;
            property.FindPropertyRelative("chainTargets").intValue = chainTargets;
            property.FindPropertyRelative("chainDamageFalloff").floatValue = chainFalloff;
            property.FindPropertyRelative("chainSearchRange").floatValue = chainRange;
            property.FindPropertyRelative("chargeDelay").floatValue = charge;
        }

        private static List<CommanderDefinition> EnsureCommanderDefinitions()
        {
            return new List<CommanderDefinition>
            {
                EnsureCommanderDefinition("FlameWarden", CommanderId.FlameWarden, "Flame Warden", "Flame towers apply stronger burn pressure and longer burn effects.", UpgradeTarget.FlameTower, 0.15f, 0f, 0f, 0.35f, 0.5f),
                EnsureCommanderDefinition("RangerCaptain", CommanderId.RangerCaptain, "Ranger Captain", "Arrow towers fire faster and gain a small crit chance bonus.", UpgradeTarget.ArrowTower, 0f, 0.15f, 0.1f, 0f, 0f)
            };
        }

        private static CommanderDefinition EnsureCommanderDefinition(string assetName, CommanderId id, string displayName, string description, UpgradeTarget target, float damagePercent, float attackSpeedPercent, float critChance, float burnDamagePercent, float burnDurationPercent)
        {
            string path = $"{DataFolder}/{assetName}.asset";
            CommanderDefinition definition = AssetDatabase.LoadAssetAtPath<CommanderDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CommanderDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.id = id;
            definition.displayName = displayName;
            definition.description = description;
            definition.primaryTarget = target;
            definition.baseDamagePercent = damagePercent;
            definition.baseAttackSpeedPercent = attackSpeedPercent;
            definition.baseCritChanceFlat = critChance;
            definition.baseBurnDamagePercent = burnDamagePercent;
            definition.baseBurnDurationPercent = burnDurationPercent;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static List<UpgradeDefinition> EnsureUpgradeDefinitions()
        {
            return new List<UpgradeDefinition>
            {
                EnsureUpgradeDefinition("ArrowDamageUp", "arrow_damage_up", "Arrow Damage Up", "Arrow towers deal 20% more damage.", UpgradeTarget.ArrowTower, StatModifierType.DamagePercent, 0.2f),
                EnsureUpgradeDefinition("ArrowAttackSpeedUp", "arrow_attack_speed_up", "Arrow Attack Speed Up", "Arrow towers attack 25% faster.", UpgradeTarget.ArrowTower, StatModifierType.AttackSpeedPercent, 0.25f),
                EnsureUpgradeDefinition("ArrowRangeUp", "arrow_range_up", "Arrow Range Up", "Arrow towers gain +0.5 range.", UpgradeTarget.ArrowTower, StatModifierType.RangeFlat, 0.5f),
                EnsureUpgradeDefinition("ArrowCritChanceUp", "arrow_crit_up", "Arrow Critical Chance Up", "Arrow towers gain +10% crit chance.", UpgradeTarget.ArrowTower, StatModifierType.CritChanceFlat, 0.1f, false, CommanderId.RangerCaptain),
                EnsureUpgradeDefinition("FlameDamageUp", "flame_damage_up", "Flame Damage Up", "Flame towers deal 20% more damage.", UpgradeTarget.FlameTower, StatModifierType.DamagePercent, 0.2f),
                EnsureUpgradeDefinition("BurnDurationUp", "burn_duration_up", "Burn Duration Up", "Flame tower burns last 40% longer.", UpgradeTarget.FlameTower, StatModifierType.BurnDurationPercent, 0.4f),
                EnsureUpgradeDefinition("BurnDamageUp", "burn_damage_up", "Burn Damage Up", "Burn damage increases by 35%.", UpgradeTarget.FlameTower, StatModifierType.BurnDamagePercent, 0.35f, false, CommanderId.FlameWarden),
                EnsureUpgradeDefinition("FlameRangeUp", "flame_range_up", "Flame Range Up", "Flame towers gain +0.45 range.", UpgradeTarget.FlameTower, StatModifierType.RangeFlat, 0.45f),
                EnsureUpgradeDefinition("AllTowersDamageUp", "all_towers_damage_up", "All Towers Damage Up", "All towers deal 12% more damage.", UpgradeTarget.Global, StatModifierType.DamagePercent, 0.12f),
                EnsureUpgradeDefinition("AllTowersAttackSpeedUp", "all_towers_attack_speed_up", "All Towers Attack Speed Up", "All towers attack 12% faster.", UpgradeTarget.Global, StatModifierType.AttackSpeedPercent, 0.12f),
                EnsureUpgradeDefinition("GainExtraGold", "gain_extra_gold", "Gain Extra Gold", "Gain 80 extra gold instantly.", UpgradeTarget.Global, StatModifierType.BonusGoldFlat, 80f),
                EnsureUpgradeDefinition("BaseHealthUp", "base_health_up", "Base Health Up", "Gain +3 lives.", UpgradeTarget.Global, StatModifierType.BaseHealthFlat, 3f)
            };
        }

        private static UpgradeDefinition EnsureUpgradeDefinition(string assetName, string id, string displayName, string description, UpgradeTarget target, StatModifierType modifierType, float value, bool repeatable = true, CommanderId commanderRestriction = CommanderId.None)
        {
            string path = $"{DataFolder}/{assetName}.asset";
            UpgradeDefinition definition = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<UpgradeDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.id = id;
            definition.displayName = displayName;
            definition.description = description;
            definition.type = modifierType == StatModifierType.BonusGoldFlat || modifierType == StatModifierType.BaseHealthFlat ? UpgradeType.Utility : UpgradeType.TowerStat;
            definition.target = target;
            definition.modifierType = modifierType;
            definition.value = value;
            definition.isRepeatable = repeatable;
            definition.commanderRestriction = commanderRestriction;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void ConfigureWaves(SerializedObject so)
        {
            SerializedProperty waves = so.FindProperty("waves");
            waves.arraySize = 10;
            SetWave(waves.GetArrayElementAtIndex(0), "Wave 1", 25, false, false, new[] { new WaveGroupSpec(EnemyType.Basic, 12, 1.0f, 0f) });
            SetWave(waves.GetArrayElementAtIndex(1), "Wave 2", 30, false, false, new[] { new WaveGroupSpec(EnemyType.Basic, 18, 0.9f, 0f) });
            SetWave(waves.GetArrayElementAtIndex(2), "Wave 3", 35, false, true, new[] { new WaveGroupSpec(EnemyType.Basic, 14, 0.85f, 0f), new WaveGroupSpec(EnemyType.Fast, 8, 0.65f, 3f) });
            SetWave(waves.GetArrayElementAtIndex(3), "Wave 4", 40, false, false, new[] { new WaveGroupSpec(EnemyType.Armored, 8, 1.4f, 0f) });
            SetWave(waves.GetArrayElementAtIndex(4), "Wave 5", 45, false, false, new[] { new WaveGroupSpec(EnemyType.Basic, 16, 0.75f, 0f), new WaveGroupSpec(EnemyType.Fast, 10, 0.55f, 2f), new WaveGroupSpec(EnemyType.Armored, 5, 1.2f, 5f) });
            SetWave(waves.GetArrayElementAtIndex(5), "Wave 6", 50, false, true, new[] { new WaveGroupSpec(EnemyType.Basic, 28, 0.45f, 0f), new WaveGroupSpec(EnemyType.Healer, 3, 3.0f, 4f) });
            SetWave(waves.GetArrayElementAtIndex(6), "Wave 7", 50, false, false, new[] { new WaveGroupSpec(EnemyType.Fast, 18, 0.45f, 0f), new WaveGroupSpec(EnemyType.Armored, 9, 1.0f, 3f) });
            SetWave(waves.GetArrayElementAtIndex(7), "Wave 8", 55, false, true, new[] { new WaveGroupSpec(EnemyType.Basic, 24, 0.45f, 0f), new WaveGroupSpec(EnemyType.Fast, 14, 0.4f, 2f), new WaveGroupSpec(EnemyType.Armored, 8, 0.9f, 5f), new WaveGroupSpec(EnemyType.Healer, 3, 2.5f, 7f) });
            SetWave(waves.GetArrayElementAtIndex(8), "Wave 9", 60, false, false, new[] { new WaveGroupSpec(EnemyType.Armored, 14, 0.75f, 0f), new WaveGroupSpec(EnemyType.Healer, 4, 2.2f, 3f), new WaveGroupSpec(EnemyType.Fast, 12, 0.35f, 8f) });
            SetWave(waves.GetArrayElementAtIndex(9), "Wave 10 - Boss", 0, true, false, new[] { new WaveGroupSpec(EnemyType.Boss, 1, 0f, 0f), new WaveGroupSpec(EnemyType.Basic, 16, 0.55f, 4f), new WaveGroupSpec(EnemyType.Fast, 12, 0.45f, 10f) });
        }

        private static void SetWave(SerializedProperty property, string name, int clearReward, bool bossWave, bool upgradeDraft, WaveGroupSpec[] groups)
        {
            property.FindPropertyRelative("displayName").stringValue = name;
            property.FindPropertyRelative("clearReward").intValue = clearReward;
            property.FindPropertyRelative("isBossWave").boolValue = bossWave;
            property.FindPropertyRelative("triggersUpgradeDraft").boolValue = upgradeDraft;
            SerializedProperty groupList = property.FindPropertyRelative("groups");
            groupList.arraySize = groups.Length;

            for (int i = 0; i < groups.Length; i++)
            {
                SerializedProperty group = groupList.GetArrayElementAtIndex(i);
                group.FindPropertyRelative("enemyType").enumValueIndex = (int)groups[i].Type;
                group.FindPropertyRelative("count").intValue = groups[i].Count;
                group.FindPropertyRelative("interval").floatValue = groups[i].Interval;
                group.FindPropertyRelative("delayBeforeGroup").floatValue = groups[i].Delay;
            }
        }

        private static void CreateHud(WaveManager waveManager, Week2BattleFlow battleFlow)
        {
            GameObject uiRoot = new GameObject("UIRoot");
            Canvas canvas = uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = uiRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            uiRoot.AddComponent<GraphicRaycaster>();

            RectTransform rootRect = uiRoot.GetComponent<RectTransform>();
            rootRect.localScale = Vector3.one;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Text goldText = CreateText(uiRoot.transform, "GoldText", "Gold: 500", font, new Vector2(18f, -18f));
            Text livesText = CreateText(uiRoot.transform, "LivesText", "Lives: 20", font, new Vector2(18f, -48f));
            Text waveText = CreateText(uiRoot.transform, "WaveText", "Wave: 0/10", font, new Vector2(18f, -78f));
            Text enemiesText = CreateText(uiRoot.transform, "EnemyCountText", "Enemies: 0", font, new Vector2(18f, -108f));
            Text selectedText = CreateText(uiRoot.transform, "SelectedTowerText", "Tower: Arrow (100)", font, new Vector2(18f, -138f));
            Text commanderInfoText = CreateText(uiRoot.transform, "CommanderInfoText", "Commander: None", font, new Vector2(18f, -168f));
            Text resultText = CreateText(uiRoot.transform, "ResultText", string.Empty, font, new Vector2(0f, -42f), TextAnchor.UpperCenter);
            resultText.fontSize = 34;
            Text feedbackText = CreateText(uiRoot.transform, "FeedbackText", string.Empty, font, new Vector2(0f, -88f), TextAnchor.UpperCenter);
            feedbackText.fontSize = 22;

            Button startButton = CreateButton(uiRoot.transform, "StartWaveButton", "Start Game", font, new Vector2(-18f, -18f), TextAnchor.UpperRight);
            Button arrowButton = CreateButton(uiRoot.transform, "ArrowTowerButton", "Arrow", font, new Vector2(-18f, -66f), TextAnchor.UpperRight);
            Button flameButton = CreateButton(uiRoot.transform, "FlameTowerButton", "Flame", font, new Vector2(-18f, -114f), TextAnchor.UpperRight);
            Button magicButton = CreateButton(uiRoot.transform, "MagicTowerButton", "Magic", font, new Vector2(-18f, -162f), TextAnchor.UpperRight);
            GameObject resultPanel = CreateResultPanel(uiRoot.transform, font, out Text resultTitleText, out Text resultSummaryText, out Button restartButton, out Button backButton);
            GameObject commanderPanel = CreateCommanderPanel(uiRoot.transform, font,
                out Button flameWardenButton, out Button rangerCaptainButton,
                out Text flameWardenNameText, out Text flameWardenDescText,
                out Text rangerCaptainNameText, out Text rangerCaptainDescText);
            GameObject upgradePanel = CreateUpgradePanel(uiRoot.transform, font,
                out Button optionButtonA, out Button optionButtonB, out Button optionButtonC,
                out Text optionTitleA, out Text optionTitleB, out Text optionTitleC,
                out Text optionDescA, out Text optionDescB, out Text optionDescC);

            BattleHud hud = uiRoot.AddComponent<BattleHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("goldText").objectReferenceValue = goldText;
            so.FindProperty("livesText").objectReferenceValue = livesText;
            so.FindProperty("waveText").objectReferenceValue = waveText;
            so.FindProperty("enemyCountText").objectReferenceValue = enemiesText;
            so.FindProperty("selectedTowerText").objectReferenceValue = selectedText;
            so.FindProperty("commanderInfoText").objectReferenceValue = commanderInfoText;
            so.FindProperty("resultText").objectReferenceValue = resultText;
            so.FindProperty("feedbackText").objectReferenceValue = feedbackText;
            so.FindProperty("startWaveButton").objectReferenceValue = startButton;
            so.FindProperty("arrowTowerButton").objectReferenceValue = arrowButton;
            so.FindProperty("flameTowerButton").objectReferenceValue = flameButton;
            so.FindProperty("magicTowerButton").objectReferenceValue = magicButton;
            so.FindProperty("resultPanel").objectReferenceValue = resultPanel;
            so.FindProperty("resultTitleText").objectReferenceValue = resultTitleText;
            so.FindProperty("resultSummaryText").objectReferenceValue = resultSummaryText;
            so.FindProperty("restartButton").objectReferenceValue = restartButton;
            so.FindProperty("backButton").objectReferenceValue = backButton;
            so.FindProperty("commanderPanel").objectReferenceValue = commanderPanel;
            so.FindProperty("flameWardenButton").objectReferenceValue = flameWardenButton;
            so.FindProperty("rangerCaptainButton").objectReferenceValue = rangerCaptainButton;
            so.FindProperty("flameWardenNameText").objectReferenceValue = flameWardenNameText;
            so.FindProperty("flameWardenDescText").objectReferenceValue = flameWardenDescText;
            so.FindProperty("rangerCaptainNameText").objectReferenceValue = rangerCaptainNameText;
            so.FindProperty("rangerCaptainDescText").objectReferenceValue = rangerCaptainDescText;
            so.FindProperty("upgradePanel").objectReferenceValue = upgradePanel;
            so.FindProperty("optionButtonA").objectReferenceValue = optionButtonA;
            so.FindProperty("optionButtonB").objectReferenceValue = optionButtonB;
            so.FindProperty("optionButtonC").objectReferenceValue = optionButtonC;
            so.FindProperty("optionTitleA").objectReferenceValue = optionTitleA;
            so.FindProperty("optionTitleB").objectReferenceValue = optionTitleB;
            so.FindProperty("optionTitleC").objectReferenceValue = optionTitleC;
            so.FindProperty("optionDescA").objectReferenceValue = optionDescA;
            so.FindProperty("optionDescB").objectReferenceValue = optionDescB;
            so.FindProperty("optionDescC").objectReferenceValue = optionDescC;
            so.FindProperty("waveManager").objectReferenceValue = waveManager;
            so.FindProperty("week2BattleFlow").objectReferenceValue = battleFlow;
            so.FindProperty("enableFallbackHud").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateCommanderPanel(Transform parent, Font font,
            out Button flameWardenButton, out Button rangerCaptainButton,
            out Text flameWardenNameText, out Text flameWardenDescText,
            out Text rangerCaptainNameText, out Text rangerCaptainDescText)
        {
            GameObject panelObject = CreatePanel(parent, "CommanderPanel", new Vector2(760f, 360f), new Color(0.04f, 0.08f, 0.12f, 0.94f));
            CreateText(panelObject.transform, "CommanderTitle", "Choose Commander", font, new Vector2(0f, -22f), TextAnchor.UpperCenter).fontSize = 30;

            GameObject flameCard = CreateSubPanel(panelObject.transform, "FlameCard", new Vector2(-180f, 25f), new Vector2(280f, 210f), new Color(0.28f, 0.16f, 0.1f, 0.96f));
            flameWardenNameText = CreateText(flameCard.transform, "FlameName", "Flame Warden", font, new Vector2(0f, -18f), TextAnchor.UpperCenter);
            flameWardenNameText.fontSize = 24;
            flameWardenDescText = CreateWrappedText(flameCard.transform, "FlameDesc", "Flame towers apply stronger burn pressure and longer burn effects.", font, new Vector2(0f, -60f), new Vector2(220f, 78f), 18);
            flameWardenButton = CreateButton(flameCard.transform, "FlameSelectButton", "Select", font, new Vector2(0f, -160f), TextAnchor.MiddleCenter);

            GameObject rangerCard = CreateSubPanel(panelObject.transform, "RangerCard", new Vector2(180f, 25f), new Vector2(280f, 210f), new Color(0.09f, 0.2f, 0.16f, 0.96f));
            rangerCaptainNameText = CreateText(rangerCard.transform, "RangerName", "Ranger Captain", font, new Vector2(0f, -18f), TextAnchor.UpperCenter);
            rangerCaptainNameText.fontSize = 24;
            rangerCaptainDescText = CreateWrappedText(rangerCard.transform, "RangerDesc", "Arrow towers fire faster and gain a small crit chance bonus.", font, new Vector2(0f, -60f), new Vector2(220f, 78f), 18);
            rangerCaptainButton = CreateButton(rangerCard.transform, "RangerSelectButton", "Select", font, new Vector2(0f, -160f), TextAnchor.MiddleCenter);

            return panelObject;
        }

        private static GameObject CreateUpgradePanel(Transform parent, Font font,
            out Button optionButtonA, out Button optionButtonB, out Button optionButtonC,
            out Text optionTitleA, out Text optionTitleB, out Text optionTitleC,
            out Text optionDescA, out Text optionDescB, out Text optionDescC)
        {
            GameObject panelObject = CreatePanel(parent, "UpgradePanel", new Vector2(920f, 340f), new Color(0.04f, 0.08f, 0.12f, 0.94f));
            CreateText(panelObject.transform, "UpgradeTitle", "Choose Upgrade", font, new Vector2(0f, -22f), TextAnchor.UpperCenter).fontSize = 30;

            CreateUpgradeCard(panelObject.transform, "OptionA", font, new Vector2(-280f, -52f),
                out optionButtonA, out optionTitleA, out optionDescA);
            CreateUpgradeCard(panelObject.transform, "OptionB", font, new Vector2(0f, -52f),
                out optionButtonB, out optionTitleB, out optionDescB);
            CreateUpgradeCard(panelObject.transform, "OptionC", font, new Vector2(280f, -52f),
                out optionButtonC, out optionTitleC, out optionDescC);

            panelObject.SetActive(false);
            return panelObject;
        }

        private static void CreateUpgradeCard(Transform parent, string name, Font font, Vector2 anchoredPosition,
            out Button button, out Text titleText, out Text descText)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.14f, 0.2f, 0.28f, 0.97f);
            button = buttonObject.AddComponent<Button>();

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(230f, 200f);
            rect.anchoredPosition = anchoredPosition;

            titleText = CreateText(buttonObject.transform, $"{name}_Title", "Upgrade", font, new Vector2(0f, -20f), TextAnchor.UpperCenter);
            titleText.fontSize = 22;

            descText = CreateWrappedText(buttonObject.transform, $"{name}_Desc", "Description", font, new Vector2(0f, -66f), new Vector2(192f, 96f), 17);
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size, Color color)
        {
            GameObject panelObject = new GameObject(name);
            panelObject.transform.SetParent(parent, false);
            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = color;

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = size;
            panelRect.anchoredPosition = Vector2.zero;
            return panelObject;
        }

        private static GameObject CreateSubPanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject subPanel = new GameObject(name);
            subPanel.transform.SetParent(parent, false);
            Image image = subPanel.AddComponent<Image>();
            image.color = color;

            RectTransform rect = subPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            return subPanel;
        }

        private static Text CreateWrappedText(Transform parent, string name, string value, Font font, Vector2 anchoredPosition, Vector2 size, int fontSize)
        {
            Text text = CreateText(parent, name, value, font, anchoredPosition, TextAnchor.UpperCenter);
            text.fontSize = fontSize;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            return text;
        }

        private static GameObject CreateResultPanel(Transform parent, Font font, out Text titleText, out Text summaryText, out Button restartButton, out Button backButton)
        {
            GameObject panelObject = new GameObject("ResultPanel");
            panelObject.transform.SetParent(parent, false);

            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0.04f, 0.08f, 0.12f, 0.9f);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(360f, 220f);
            panelRect.anchoredPosition = Vector2.zero;

            titleText = CreateText(panelObject.transform, "ResultTitle", "Victory", font, new Vector2(0f, -26f), TextAnchor.UpperCenter);
            titleText.fontSize = 34;

            summaryText = CreateText(panelObject.transform, "ResultSummary", "All waves have been cleared.", font, new Vector2(0f, -82f), TextAnchor.UpperCenter);
            summaryText.fontSize = 20;
            RectTransform summaryRect = summaryText.GetComponent<RectTransform>();
            summaryRect.sizeDelta = new Vector2(300f, 54f);

            restartButton = CreateButton(panelObject.transform, "RestartButton", "Restart", font, new Vector2(-78f, -154f), TextAnchor.MiddleCenter);
            backButton = CreateButton(panelObject.transform, "BackButton", "Back", font, new Vector2(78f, -154f), TextAnchor.MiddleCenter);

            panelObject.SetActive(false);
            return panelObject;
        }

        private static Text CreateText(Transform parent, string name, string value, Font font, Vector2 anchoredPosition, TextAnchor anchor = TextAnchor.UpperLeft)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = 22;
            text.color = Color.white;
            text.alignment = anchor;

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = AnchorFor(anchor);
            rect.anchorMax = AnchorFor(anchor);
            rect.pivot = PivotFor(anchor);
            rect.sizeDelta = new Vector2(260f, 32f);
            rect.anchoredPosition = anchoredPosition;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Font font, Vector2 anchoredPosition, TextAnchor anchor)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.15f, 0.35f, 0.55f, 0.95f);
            Button button = buttonObject.AddComponent<Button>();

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = AnchorFor(anchor);
            rect.anchorMax = AnchorFor(anchor);
            rect.pivot = PivotFor(anchor);
            rect.sizeDelta = new Vector2(150f, 38f);
            rect.anchoredPosition = anchoredPosition;

            Text text = CreateText(buttonObject.transform, "Label", label, font, Vector2.zero, TextAnchor.MiddleCenter);
            RectTransform labelRect = text.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            text.fontSize = 18;
            text.raycastTarget = false;

            return button;
        }

        private static Vector2 AnchorFor(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperRight:
                    return new Vector2(1f, 1f);
                case TextAnchor.UpperCenter:
                    return new Vector2(0.5f, 1f);
                case TextAnchor.MiddleCenter:
                    return new Vector2(0.5f, 0.5f);
                default:
                    return new Vector2(0f, 1f);
            }
        }

        private static Vector2 PivotFor(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperRight:
                    return new Vector2(1f, 1f);
                case TextAnchor.UpperCenter:
                    return new Vector2(0.5f, 1f);
                case TextAnchor.MiddleCenter:
                    return new Vector2(0.5f, 0.5f);
                default:
                    return new Vector2(0f, 1f);
            }
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateFallbackSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private readonly struct WaveGroupSpec
        {
            public readonly EnemyType Type;
            public readonly int Count;
            public readonly float Interval;
            public readonly float Delay;

            public WaveGroupSpec(EnemyType type, int count, float interval, float delay)
            {
                Type = type;
                Count = count;
                Interval = interval;
                Delay = delay;
            }
        }
    }
}
