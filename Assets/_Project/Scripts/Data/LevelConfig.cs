using UnityEngine;

namespace LawnDefense.Data
{
    [CreateAssetMenu(menuName = "LawnDefense/Level Config")]
    public sealed class LevelConfig : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public int Rows = 5;
        public int Columns = 9;
        public int InitialSun = 50;
        public float NaturalSunInterval = 7f;
        public int NaturalSunAmount = 25;
        public PlantConfig[] AvailablePlants;
        public WaveConfig WaveConfig;
    }
}
