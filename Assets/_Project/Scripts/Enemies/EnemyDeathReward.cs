using LawnDefense.Augments;
using LawnDefense.Data;
using LawnDefense.Sun;
using UnityEngine;

namespace LawnDefense.Enemies
{
    public sealed class EnemyDeathReward : MonoBehaviour
    {
        [SerializeField] private int bonusRewardSun;

        public void Initialize(EnemyConfig config)
        {
            bonusRewardSun = config != null ? Mathf.Max(0, config.BonusRewardSun) : 0;
        }

        public void Grant(EnemyConfig config, SunSystem sunSystem)
        {
            if (sunSystem == null)
            {
                return;
            }

            int baseReward = config != null ? Mathf.Max(0, config.RewardSun) : 0;
            int reward = AugmentSystem.Modifiers.GetEnemyRewardSun(baseReward + bonusRewardSun);
            if (reward > 0)
            {
                sunSystem.Wallet.Add(reward);
            }
        }
    }
}
