using LawnDefense.Augments;
using LawnDefense.Core;
using UnityEngine;

namespace LawnDefense.UI
{
    public sealed class AugmentChoiceView : MonoBehaviour
    {
        [SerializeField] private AugmentSystem augmentSystem;
        [SerializeField] private GameObject panel;
        [SerializeField] private AugmentCardView[] cardViews;

        private void Awake()
        {
            if (augmentSystem == null)
            {
                augmentSystem = FindObjectOfType<AugmentSystem>();
            }

            if (panel != gameObject)
            {
                Hide();
            }
        }

        private void OnEnable()
        {
            GameEvents.AugmentChoicesOffered += HandleChoicesOffered;
            GameEvents.AugmentSelectionCompleted += Hide;
        }

        private void OnDisable()
        {
            GameEvents.AugmentChoicesOffered -= HandleChoicesOffered;
            GameEvents.AugmentSelectionCompleted -= Hide;
        }

        public void Select(AugmentConfig config)
        {
            if (augmentSystem != null)
            {
                augmentSystem.SelectAugment(config);
            }
        }

        private void HandleChoicesOffered(AugmentConfig[] choices)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            int count = cardViews != null ? cardViews.Length : 0;
            for (int i = 0; i < count; i++)
            {
                bool hasChoice = choices != null && i < choices.Length && choices[i] != null;
                cardViews[i].gameObject.SetActive(hasChoice);
                if (hasChoice)
                {
                    cardViews[i].Initialize(choices[i], this);
                }
            }
        }

        private void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
