using System;

namespace LawnDefense.Core
{
    public static class GameEvents
    {
        public static event Action<int> SunChanged;
        public static event Action<GameState> GameStateChanged;
        public static event Action<float> WaveProgressChanged;
        public static event Action<string, float> PlantCardCooldownChanged;

        public static void RaiseSunChanged(int value) => SunChanged?.Invoke(value);
        public static void RaiseGameStateChanged(GameState state) => GameStateChanged?.Invoke(state);
        public static void RaiseWaveProgressChanged(float progress) => WaveProgressChanged?.Invoke(progress);
        public static void RaisePlantCardCooldownChanged(string plantId, float normalizedRemaining) =>
            PlantCardCooldownChanged?.Invoke(plantId, normalizedRemaining);
    }
}
