using System;
using UnityEngine;

namespace EvoTowers.Task1
{
    [Serializable]
    public class TowerEvolutionDefinition
    {
        public string id;
        public TowerType baseTowerType = TowerType.Arrow;
        public TowerBranchId branchId = TowerBranchId.None;
        public string displayName;
        [TextArea(1, 3)] public string gameplayTags;
        [TextArea(2, 5)] public string description;
        public int evolveCost = 120;
        public int requiredLevel = 3;
        public int requiredExperience = 100;
        public float damageMultiplier = 1f;
        public float attackSpeedMultiplier = 1f;
        public float rangeMultiplier = 1f;
        public TowerTargetPriority targetPriority = TowerTargetPriority.ClosestToGoal;
        public TowerTag[] tags = Array.Empty<TowerTag>();
        public Sprite sprite;
        public GameObject visualPrefab;
        public GameObject projectilePrefab;
        public GameObject attackEffectPrefab;
        public Color accentColor = Color.white;
        public float eliteBonusDamage = 0f;
        public float explosionRadiusMultiplier = 1f;
        public float explosionEdgeDamagePercent = 0.6f;
        public float burnDuration = 0f;
        public float burnDamageMultiplier = 0f;
        public int maxBurnStacks = 1;
        public float slowPercent = 0f;
        public float slowDuration = 0f;
        public float splashRadius = 0f;
        public int maxComboStacks = 0;
        public float comboAttackSpeedPerStack = 0f;
        public int chainTargets = 0;
        public float chainDamageFalloff = 0f;
        public float chainSearchRange = 0f;
        public float chargeDelay = 0f;

        public bool HasTag(TowerTag tag)
        {
            if (tags == null)
            {
                return false;
            }

            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == tag)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
