using LawnDefense.Augments;
using UnityEngine;
using UnityEngine.UI;

namespace LawnDefense.UI
{
    public sealed class AugmentCardView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text rarityText;
        [SerializeField] private Button button;

        private AugmentConfig config;
        private AugmentChoiceView owner;

        public void Initialize(AugmentConfig augment, AugmentChoiceView choiceView)
        {
            config = augment;
            owner = choiceView;
            Refresh();
        }

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(Select);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(Select);
            }
        }

        private void Select()
        {
            if (owner != null && config != null)
            {
                owner.Select(config);
            }
        }

        private void Refresh()
        {
            if (config == null)
            {
                return;
            }

            if (iconImage != null)
            {
                iconImage.sprite = config.Icon;
                iconImage.enabled = config.Icon != null;
            }

            if (nameText != null)
            {
                nameText.text = config.DisplayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = config.Description;
            }

            if (rarityText != null)
            {
                rarityText.text = config.Rarity.ToString();
            }
        }
    }
}
