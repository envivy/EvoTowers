using UnityEngine;

namespace EvoTowers.Task1
{
    [CreateAssetMenu(menuName = "EvoTowers/Week2/Upgrade Definition", fileName = "UpgradeDefinition")]
    public class UpgradeDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea(2, 4)] public string description;
        public string rarity = "Common";
        public UpgradeType type = UpgradeType.TowerStat;
        public UpgradeTarget target = UpgradeTarget.Global;
        public StatModifierType modifierType = StatModifierType.DamagePercent;
        public float value = 0.1f;
        public bool isRepeatable = true;
        public CommanderId commanderRestriction = CommanderId.None;
    }
}
