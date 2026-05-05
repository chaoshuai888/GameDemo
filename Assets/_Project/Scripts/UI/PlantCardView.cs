using LawnDefense.Augments;
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
        [SerializeField] private Image selectedFrame;
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
            GameEvents.AugmentSelected += HandleAugmentSelected;
            GameEvents.PlantSelectionChanged += HandlePlantSelectionChanged;
        }

        private void OnDisable()
        {
            GameEvents.PlantCardCooldownChanged -= HandleCooldownChanged;
            GameEvents.AugmentSelected -= HandleAugmentSelected;
            GameEvents.PlantSelectionChanged -= HandlePlantSelectionChanged;
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
                costText.text = AugmentSystem.Modifiers.GetPlantCost(config).ToString();
            }

            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = 0f;
            }

            SetSelected(false);
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

        private void HandleAugmentSelected(AugmentConfig augment)
        {
            RefreshStaticContent();
        }

        private void HandlePlantSelectionChanged(PlantConfig selectedConfig)
        {
            bool selected = config != null && selectedConfig != null && selectedConfig.Id == config.Id;
            SetSelected(selected);
        }

        private void SetSelected(bool selected)
        {
            if (selectedFrame != null)
            {
                selectedFrame.enabled = selected;
            }
        }
    }
}
