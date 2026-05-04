using LawnDefense.Core;
using LawnDefense.Data;
using LawnDefense.Placement;
using UnityEngine;
using UnityEngine.UI;

namespace LawnDefense.UI
{
    public sealed class PlantCardView : MonoBehaviour
    {
        [SerializeField] private PlantConfig config;
        [SerializeField] private PlantPlacementSystem placementSystem;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text costText;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private Button button;

        public void Initialize(PlantConfig plantConfig, PlantPlacementSystem placement)
        {
            config = plantConfig;
            placementSystem = placement;
            RefreshStaticContent();
        }

        private void Awake()
        {
            RefreshStaticContent();
            if (button != null)
            {
                button.onClick.AddListener(SelectPlant);
            }
        }

        private void OnEnable()
        {
            GameEvents.PlantCardCooldownChanged += HandleCooldownChanged;
        }

        private void OnDisable()
        {
            GameEvents.PlantCardCooldownChanged -= HandleCooldownChanged;
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(SelectPlant);
            }
        }

        private void SelectPlant()
        {
            if (placementSystem != null && config != null)
            {
                placementSystem.SelectPlant(config);
            }
        }

        private void RefreshStaticContent()
        {
            if (config == null)
            {
                return;
            }

            if (iconImage != null)
            {
                iconImage.sprite = config.Icon;
            }

            if (costText != null)
            {
                costText.text = config.SunCost.ToString();
            }

            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = 0f;
            }
        }

        private void HandleCooldownChanged(string plantId, float normalizedRemaining)
        {
            if (config == null || config.Id != plantId)
            {
                return;
            }

            float cooldown = Mathf.Clamp01(normalizedRemaining);
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = cooldown;
            }

            if (button != null)
            {
                button.interactable = cooldown <= 0f;
            }
        }
    }
}
