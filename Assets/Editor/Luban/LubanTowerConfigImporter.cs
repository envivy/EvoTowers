using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using EvoTowers.Task1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EvoTowers.EditorTools
{
    public static class LubanTowerConfigImporter
    {
        private const string TowerCsvPath = "Tools/Luban/Datas/Tower.csv";
        private const string BattleScenePath = "Assets/Scenes/Battle.unity";

        [MenuItem("EvoTowers/Luban/Import Tower Configs To Battle Scene")]
        public static void ImportToBattleScene()
        {
            if (!File.Exists(BattleScenePath))
            {
                EditorUtility.DisplayDialog("Luban", $"Battle scene not found:\n{BattleScenePath}", "OK");
                return;
            }

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(BattleScenePath);
            GameManager gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                EditorUtility.DisplayDialog("Luban", "No GameManager found in Battle scene.", "OK");
                return;
            }

            int count = ApplyTowerConfigs(new SerializedObject(gameManager));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();
            Debug.Log($"Imported {count} tower configs from {TowerCsvPath} into {BattleScenePath}.");
        }

        public static int ApplyTowerConfigs(SerializedObject gameManager)
        {
            List<TowerRow> rows = LoadTowerRows();
            SerializedProperty towers = gameManager.FindProperty("towerConfigs");
            towers.arraySize = rows.Count;

            for (int i = 0; i < rows.Count; i++)
            {
                ApplyTowerConfig(towers.GetArrayElementAtIndex(i), rows[i]);
            }

            gameManager.ApplyModifiedPropertiesWithoutUndo();
            return rows.Count;
        }

        private static List<TowerRow> LoadTowerRows()
        {
            if (!File.Exists(TowerCsvPath))
            {
                throw new FileNotFoundException("Tower table not found.", TowerCsvPath);
            }

            string[] lines = ReadAllLinesShared(TowerCsvPath);
            if (lines.Length < 4)
            {
                throw new InvalidDataException($"Tower table has no data rows: {TowerCsvPath}");
            }

            string[] headers = SplitCsvLine(lines[0]);
            Dictionary<string, int> columnMap = BuildColumnMap(headers);
            List<TowerRow> rows = new List<TowerRow>();

            for (int i = 3; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                string[] values = SplitCsvLine(lines[i]);
                rows.Add(ParseTowerRow(values, columnMap, i + 1));
            }

            return rows;
        }

        private static string[] ReadAllLinesShared(string path)
        {
            const int maxAttempts = 5;
            IOException lastException = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true))
                    {
                        List<string> lines = new List<string>();
                        while (!reader.EndOfStream)
                        {
                            lines.Add(reader.ReadLine());
                        }

                        return lines.ToArray();
                    }
                }
                catch (IOException exception)
                {
                    lastException = exception;
                    Thread.Sleep(50 * attempt);
                }
            }

            throw new IOException($"Could not read tower table after {maxAttempts} attempts. Close any app editing {path}, then run Create Battle Scene again.", lastException);
        }

        private static Dictionary<string, int> BuildColumnMap(string[] headers)
        {
            Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 1; i < headers.Length; i++)
            {
                map[headers[i]] = i;
            }

            return map;
        }

        private static TowerRow ParseTowerRow(string[] values, Dictionary<string, int> columnMap, int lineNumber)
        {
            string typeName = Get(values, columnMap, "Type");
            if (!Enum.TryParse(typeName, true, out TowerType type))
            {
                throw new InvalidDataException($"Unknown TowerType '{typeName}' at {TowerCsvPath}:{lineNumber}");
            }

            return new TowerRow(
                type,
                Get(values, columnMap, "Name"),
                ParseInt(values, columnMap, "Cost"),
                ParseFloat(values, columnMap, "Damage"),
                ParseFloat(values, columnMap, "Range"),
                ParseFloat(values, columnMap, "AttackInterval"),
                ParseBool(values, columnMap, "UseDamageOverTime"),
                ParseFloat(values, columnMap, "DotDamagePerSecond"),
                ParseFloat(values, columnMap, "DotDuration"),
                Get(values, columnMap, "SpritePath"),
                Get(values, columnMap, "VisualPrefabPath"),
                Get(values, columnMap, "ProjectilePrefabPath"),
                Get(values, columnMap, "AttackEffectPrefabPath"),
                new Vector2(ParseFloat(values, columnMap, "VisualScaleX"), ParseFloat(values, columnMap, "VisualScaleY")));
        }

        private static void ApplyTowerConfig(SerializedProperty property, TowerRow row)
        {
            property.FindPropertyRelative("type").enumValueIndex = (int)row.Type;
            property.FindPropertyRelative("displayName").stringValue = row.Name;
            property.FindPropertyRelative("cost").intValue = row.Cost;
            property.FindPropertyRelative("range").floatValue = row.Range;
            property.FindPropertyRelative("damage").floatValue = row.Damage;
            property.FindPropertyRelative("attackInterval").floatValue = row.AttackInterval;
            property.FindPropertyRelative("useDamageOverTime").boolValue = row.UseDamageOverTime;
            property.FindPropertyRelative("dotDamagePerSecond").floatValue = row.DotDamagePerSecond;
            property.FindPropertyRelative("dotDuration").floatValue = row.DotDuration;
            property.FindPropertyRelative("sprite").objectReferenceValue = LoadAsset<Sprite>(row.SpritePath);
            property.FindPropertyRelative("visualPrefab").objectReferenceValue = LoadAsset<GameObject>(row.VisualPrefabPath);
            property.FindPropertyRelative("projectilePrefab").objectReferenceValue = LoadAsset<GameObject>(row.ProjectilePrefabPath);
            property.FindPropertyRelative("attackEffectPrefab").objectReferenceValue = LoadAsset<GameObject>(row.AttackEffectPrefabPath);
            property.FindPropertyRelative("visualScale").vector2Value = row.VisualScale;
        }

        private static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                Debug.LogWarning($"Tower table references missing asset: {path}");
            }

            return asset;
        }

        private static string Get(string[] values, Dictionary<string, int> columnMap, string column)
        {
            if (!columnMap.TryGetValue(column, out int index) || index >= values.Length)
            {
                return string.Empty;
            }

            return values[index].Trim();
        }

        private static int ParseInt(string[] values, Dictionary<string, int> columnMap, string column)
        {
            return int.Parse(Get(values, columnMap, column), CultureInfo.InvariantCulture);
        }

        private static float ParseFloat(string[] values, Dictionary<string, int> columnMap, string column)
        {
            return float.Parse(Get(values, columnMap, column), CultureInfo.InvariantCulture);
        }

        private static bool ParseBool(string[] values, Dictionary<string, int> columnMap, string column)
        {
            return bool.Parse(Get(values, columnMap, column));
        }

        private static string[] SplitCsvLine(string line)
        {
            return line.Split(',');
        }

        private readonly struct TowerRow
        {
            public TowerType Type { get; }
            public string Name { get; }
            public int Cost { get; }
            public float Damage { get; }
            public float Range { get; }
            public float AttackInterval { get; }
            public bool UseDamageOverTime { get; }
            public float DotDamagePerSecond { get; }
            public float DotDuration { get; }
            public string SpritePath { get; }
            public string VisualPrefabPath { get; }
            public string ProjectilePrefabPath { get; }
            public string AttackEffectPrefabPath { get; }
            public Vector2 VisualScale { get; }

            public TowerRow(
                TowerType type,
                string name,
                int cost,
                float damage,
                float range,
                float attackInterval,
                bool useDamageOverTime,
                float dotDamagePerSecond,
                float dotDuration,
                string spritePath,
                string visualPrefabPath,
                string projectilePrefabPath,
                string attackEffectPrefabPath,
                Vector2 visualScale)
            {
                Type = type;
                Name = name;
                Cost = cost;
                Damage = damage;
                Range = range;
                AttackInterval = attackInterval;
                UseDamageOverTime = useDamageOverTime;
                DotDamagePerSecond = dotDamagePerSecond;
                DotDuration = dotDuration;
                SpritePath = spritePath;
                VisualPrefabPath = visualPrefabPath;
                ProjectilePrefabPath = projectilePrefabPath;
                AttackEffectPrefabPath = attackEffectPrefabPath;
                VisualScale = visualScale;
            }
        }
    }
}
