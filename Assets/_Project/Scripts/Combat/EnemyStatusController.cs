using UnityEngine;

namespace LawnDefense.Combat
{
    public sealed class EnemyStatusController : MonoBehaviour
    {
        private float slowMultiplier = 1f;
        private float slowRemaining;
        private float speedMultiplier = 1f;
        private float speedRemaining;

        public float MovementMultiplier => Mathf.Max(0.05f, slowMultiplier * speedMultiplier);

        public void ResetStatuses()
        {
            slowMultiplier = 1f;
            slowRemaining = 0f;
            speedMultiplier = 1f;
            speedRemaining = 0f;
        }

        private void Update()
        {
            TickSlow(Time.deltaTime);
            TickSpeedBoost(Time.deltaTime);
        }

        public void ApplySlow(float percent, float duration)
        {
            if (percent <= 0f || duration <= 0f)
            {
                return;
            }

            slowMultiplier = Mathf.Clamp01(1f - percent);
            slowRemaining = Mathf.Max(slowRemaining, duration);
        }

        public void ApplySpeedBoost(float multiplier, float duration)
        {
            if (multiplier <= 1f || duration <= 0f)
            {
                return;
            }

            speedMultiplier = Mathf.Max(speedMultiplier, multiplier);
            speedRemaining = Mathf.Max(speedRemaining, duration);
        }

        private void TickSlow(float deltaTime)
        {
            if (slowRemaining <= 0f)
            {
                slowMultiplier = 1f;
                return;
            }

            slowRemaining = Mathf.Max(0f, slowRemaining - deltaTime);
        }

        private void TickSpeedBoost(float deltaTime)
        {
            if (speedRemaining <= 0f)
            {
                speedMultiplier = 1f;
                return;
            }

            speedRemaining = Mathf.Max(0f, speedRemaining - deltaTime);
        }
    }
}
