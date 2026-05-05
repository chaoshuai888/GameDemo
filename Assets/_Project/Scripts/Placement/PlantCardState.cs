using LawnDefense.Augments;
using LawnDefense.Core;
using LawnDefense.Data;
using UnityEngine;

namespace LawnDefense.Placement
{
    public sealed class PlantCardState
    {
        public PlantConfig Config { get; }
        public float RemainingCooldown { get; private set; }
        public bool IsReady => RemainingCooldown <= 0f;

        public PlantCardState(PlantConfig config)
        {
            Config = config;
        }

        public void StartCooldown()
        {
            if (Config == null)
            {
                return;
            }

            RemainingCooldown = Mathf.Max(0f, GetCooldown());
            GameEvents.RaisePlantCardCooldownChanged(Config.Id, RemainingCooldown > 0f ? 1f : 0f);
        }

        public void Tick(float deltaTime)
        {
            if (Config == null || RemainingCooldown <= 0f || deltaTime <= 0f)
            {
                return;
            }

            RemainingCooldown = Mathf.Max(0f, RemainingCooldown - deltaTime);
            float cooldown = GetCooldown();
            float normalizedRemaining = cooldown > 0f ? RemainingCooldown / cooldown : 0f;
            GameEvents.RaisePlantCardCooldownChanged(Config.Id, normalizedRemaining);
        }

        private float GetCooldown()
        {
            return AugmentSystem.Modifiers.GetPlantCooldown(Config);
        }
    }
}
