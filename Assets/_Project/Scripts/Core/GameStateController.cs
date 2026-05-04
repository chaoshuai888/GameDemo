using UnityEngine;

namespace LawnDefense.Core
{
    public sealed class GameStateController : MonoBehaviour
    {
        public GameState CurrentState { get; private set; } = GameState.Preparing;

        public void SetState(GameState state)
        {
            if (CurrentState == state)
            {
                return;
            }

            CurrentState = state;
            Time.timeScale = state == GameState.Paused ? 0f : 1f;
            GameEvents.RaiseGameStateChanged(state);
        }
    }
}
