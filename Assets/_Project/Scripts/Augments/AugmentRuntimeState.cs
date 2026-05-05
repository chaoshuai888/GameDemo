using System.Collections.Generic;

namespace LawnDefense.Augments
{
    public sealed class AugmentRuntimeState
    {
        private readonly List<AugmentConfig> selectedAugments = new List<AugmentConfig>();

        public IReadOnlyList<AugmentConfig> SelectedAugments => selectedAugments;

        public void Clear()
        {
            selectedAugments.Clear();
        }

        public void Add(AugmentConfig augment)
        {
            if (augment != null && !selectedAugments.Contains(augment))
            {
                selectedAugments.Add(augment);
            }
        }
    }
}
