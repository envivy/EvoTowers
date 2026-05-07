using System;
using System.Collections.Generic;

namespace EvoTowers.Task1
{
    [Serializable]
    public class WaveDefinition
    {
        public string displayName = "Wave";
        public int clearReward = 30;
        public bool isBossWave;
        public bool triggersUpgradeDraft = true;
        public List<WaveSpawnGroup> groups = new List<WaveSpawnGroup>();
    }

    [Serializable]
    public class WaveSpawnGroup
    {
        public EnemyType enemyType = EnemyType.Basic;
        public int count = 10;
        public float interval = 0.75f;
        public float delayBeforeGroup = 0f;
    }
}
