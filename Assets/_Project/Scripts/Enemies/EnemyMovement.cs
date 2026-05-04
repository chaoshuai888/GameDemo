using LawnDefense.Waves;
using UnityEngine;

namespace LawnDefense.Enemies
{
    public sealed class EnemyMovement : MonoBehaviour
    {
        private Enemy owner;
        private WaveSystem waveSystem;
        private float moveSpeed;
        private float defeatX;
        private bool blocked;

        public bool HasReachedGoal { get; private set; }

        public void Initialize(Enemy enemy, float speed, float goalX)
        {
            owner = enemy;
            moveSpeed = Mathf.Max(0f, speed);
            defeatX = goalX;
            blocked = false;
            HasReachedGoal = false;
        }

        public void SetBlocked(bool value)
        {
            blocked = value;
        }

        public void SetWaveSystem(WaveSystem ownerWaveSystem)
        {
            waveSystem = ownerWaveSystem;
        }

        private void Update()
        {
            if (owner == null || !owner.IsAlive || blocked || HasReachedGoal)
            {
                return;
            }

            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
            if (transform.position.x <= defeatX)
            {
                HasReachedGoal = true;
                if (waveSystem != null)
                {
                    waveSystem.NotifyEnemyReachedGoal(owner);
                }
            }
        }
    }
}
