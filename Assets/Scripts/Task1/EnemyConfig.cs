using System;
using UnityEngine;

namespace EvoTowers.Task1
{
    [Serializable]
    public class EnemyConfig
    {
        public EnemyType type = EnemyType.Basic;
        public string displayName = "Basic Enemy";
        public float maxHealth = 60f;
        public float moveSpeed = 2f;
        public int goldReward = 20;
        public int lifeDamage = 1;
        [Range(0f, 0.9f)] public float armorPercent;
        public bool isElite;
        public bool isBoss;
        public bool isHealer;
        public float healRange = 1.4f;
        public float healInterval = 3f;
        public float healAmount = 20f;
        public Sprite sprite;
        public RuntimeAnimatorController animatorController;
        public Vector2 visualScale = Vector2.one;
    }
}
