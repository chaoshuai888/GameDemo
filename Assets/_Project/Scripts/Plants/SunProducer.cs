using LawnDefense.Data;
using LawnDefense.Sun;
using UnityEngine;

namespace LawnDefense.Plants
{
    public sealed class SunProducer : MonoBehaviour
    {
        [SerializeField] private SunSystem sunSystem;

        private Plant owner;
        private float timer;

        public void Initialize(Plant plant)
        {
            owner = plant;
            timer = 0f;
        }

        private void Update()
        {
            if (owner == null || owner.Config == null || owner.Config.Role != PlantRole.Producer)
            {
                return;
            }

            if (owner.Config.SunProduceInterval <= 0f)
            {
                return;
            }

            timer += Time.deltaTime;
            if (timer < owner.Config.SunProduceInterval)
            {
                return;
            }

            timer = 0f;
            if (sunSystem != null)
            {
                sunSystem.SpawnSun(owner.Config.SunProduceAmount, transform.position + Vector3.up * 0.3f);
            }
        }
    }
}
