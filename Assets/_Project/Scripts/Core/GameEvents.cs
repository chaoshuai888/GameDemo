using System;
using LawnDefense.Augments;

namespace LawnDefense.Core
{
    public static class GameEvents
    {
        public static event Action<int> SunChanged;
        public static event Action<GameState> GameStateChanged;
        public static event Action<float> WaveProgressChanged;
        public static event Action<string, float> PlantCardCooldownChanged;
        public static event Action<LawnDefense.Data.PlantConfig> PlantSelectionChanged;
        public static event Action<AugmentConfig[]> AugmentChoicesOffered;
        public static event Action<AugmentConfig> AugmentSelected;
        public static event Action AugmentSelectionCompleted;

        public static void RaiseSunChanged(int value) => SunChanged?.Invoke(value);
        public static void RaiseGameStateChanged(GameState state) => GameStateChanged?.Invoke(state);
        public static void RaiseWaveProgressChanged(float progress) => WaveProgressChanged?.Invoke(progress);
        public static void RaisePlantCardCooldownChanged(string plantId, float normalizedRemaining) =>
            PlantCardCooldownChanged?.Invoke(plantId, normalizedRemaining);
        public static void RaisePlantSelectionChanged(LawnDefense.Data.PlantConfig plantConfig) =>
            PlantSelectionChanged?.Invoke(plantConfig);
        public static void RaiseAugmentChoicesOffered(AugmentConfig[] choices) =>
            AugmentChoicesOffered?.Invoke(choices);
        public static void RaiseAugmentSelected(AugmentConfig augment) => AugmentSelected?.Invoke(augment);
        public static void RaiseAugmentSelectionCompleted() => AugmentSelectionCompleted?.Invoke();
    }
}
