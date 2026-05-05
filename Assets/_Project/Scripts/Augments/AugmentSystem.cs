using System;
using System.Collections.Generic;
using LawnDefense.Core;
using LawnDefense.Data;
using UnityEngine;

namespace LawnDefense.Augments
{
    public sealed class AugmentSystem : MonoBehaviour
    {
        public static AugmentModifierService Modifiers { get; } = new AugmentModifierService();

        private readonly AugmentRuntimeState runtimeState = new AugmentRuntimeState();
        private readonly List<AugmentConfig> currentChoices = new List<AugmentConfig>();
        private Action selectionCompleted;

        public IReadOnlyList<AugmentConfig> CurrentChoices => currentChoices;

        public bool BeginSelection(LevelConfig levelConfig, Action onSelectionCompleted)
        {
            runtimeState.Clear();
            currentChoices.Clear();
            Modifiers.SetActiveAugments(null);
            selectionCompleted = onSelectionCompleted;

            AugmentConfig[] pool = levelConfig != null ? levelConfig.AvailableAugments : null;
            if (pool == null || pool.Length == 0)
            {
                selectionCompleted = null;
                GameEvents.RaiseAugmentSelectionCompleted();
                return false;
            }

            FillChoices(pool, 3);
            if (currentChoices.Count == 0)
            {
                selectionCompleted = null;
                GameEvents.RaiseAugmentSelectionCompleted();
                return false;
            }

            GameEvents.RaiseAugmentChoicesOffered(currentChoices.ToArray());
            return true;
        }

        public void SelectAugment(AugmentConfig augment)
        {
            if (augment == null || !currentChoices.Contains(augment))
            {
                return;
            }

            runtimeState.Add(augment);
            Modifiers.SetActiveAugments(runtimeState.SelectedAugments);
            GameEvents.RaiseAugmentSelected(augment);
            CompleteSelection();
        }

        private void FillChoices(AugmentConfig[] pool, int maxChoices)
        {
            List<AugmentConfig> candidates = new List<AugmentConfig>();
            for (int i = 0; i < pool.Length; i++)
            {
                if (pool[i] != null && !candidates.Contains(pool[i]))
                {
                    candidates.Add(pool[i]);
                }
            }

            while (currentChoices.Count < maxChoices && candidates.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, candidates.Count);
                currentChoices.Add(candidates[index]);
                candidates.RemoveAt(index);
            }
        }

        private void CompleteSelection()
        {
            Action completed = selectionCompleted;
            selectionCompleted = null;
            GameEvents.RaiseAugmentSelectionCompleted();
            if (completed != null)
            {
                completed.Invoke();
            }
        }
    }
}
