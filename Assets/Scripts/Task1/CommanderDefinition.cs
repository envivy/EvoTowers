using UnityEngine;

namespace EvoTowers.Task1
{
    [CreateAssetMenu(menuName = "EvoTowers/Week2/Commander Definition", fileName = "CommanderDefinition")]
    public class CommanderDefinition : ScriptableObject
    {
        public CommanderId id = CommanderId.None;
        public string displayName;
        [TextArea(2, 4)] public string description;
        public Sprite portrait;
        public UpgradeTarget primaryTarget = UpgradeTarget.Global;
        public float baseDamagePercent;
        public float baseAttackSpeedPercent;
        public float baseCritChanceFlat;
        public float baseBurnDamagePercent;
        public float baseBurnDurationPercent;
    }
}
