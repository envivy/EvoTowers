using System;
using UnityEngine;

namespace EvoTowers.Task1
{
    [Serializable]
    public class TowerConfig
    {
        public TowerType type = TowerType.Arrow;
        public string displayName = "Arrow Tower";
        public int cost = 100;
        public float range = 2.8f;
        public float damage = 20f;
        public float attackInterval = 0.75f;
        public bool useDamageOverTime;
        public float dotDamagePerSecond = 8f;
        public float dotDuration = 2f;
        public Sprite sprite;
        public GameObject visualPrefab;
        public GameObject projectilePrefab;
        public GameObject attackEffectPrefab;
        public Vector2 visualScale = Vector2.one;

        [NonSerialized] public Sprite cachedProjectileSprite;
    }
}
