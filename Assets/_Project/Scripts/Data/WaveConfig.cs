using System;
using UnityEngine;

namespace LawnDefense.Data
{
    [CreateAssetMenu(menuName = "LawnDefense/Wave Config")]
    public sealed class WaveConfig : ScriptableObject
    {
        public WaveEntry[] Entries;
    }

    [Serializable]
    public sealed class WaveEntry
    {
        public EnemyConfig EnemyConfig;
        public float SpawnTime;
        public int Count = 1;
        public float Interval = 1f;
        public LaneMode LaneMode = LaneMode.RandomAny;
        public int SpecificLane;
        public int[] AllowedLanes;
        public bool IsMajorWave;
    }
}
