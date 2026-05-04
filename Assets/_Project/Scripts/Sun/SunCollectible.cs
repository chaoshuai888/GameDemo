using LawnDefense.Core;
using UnityEngine;

namespace LawnDefense.Sun
{
    public sealed class SunCollectible : MonoBehaviour, IPoolable
    {
        [SerializeField] private float fallSpeed = 1.2f;
        private SunSystem sunSystem;
        private int amount;
        private float targetY;
        private bool falling;

        public void Initialize(SunSystem owner, int value, float stopY)
        {
            sunSystem = owner;
            amount = value;
            targetY = stopY;
            falling = true;
        }

        private void Update()
        {
            if (!falling)
            {
                return;
            }

            Vector3 position = transform.position;
            position.y = Mathf.Max(targetY, position.y - fallSpeed * Time.deltaTime);
            transform.position = position;
            falling = position.y > targetY;
        }

        private void OnMouseDown()
        {
            if (sunSystem != null)
            {
                sunSystem.Collect(this, amount);
            }
        }

        public void OnSpawned() { }
        public void OnDespawned() => sunSystem = null;
    }
}
