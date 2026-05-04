using LawnDefense.Core;

namespace LawnDefense.Sun
{
    public sealed class SunWallet
    {
        public int Current { get; private set; }

        public void Set(int value)
        {
            Current = value < 0 ? 0 : value;
            GameEvents.RaiseSunChanged(Current);
        }

        public bool CanSpend(int amount) => Current >= amount;

        public bool TrySpend(int amount)
        {
            if (!CanSpend(amount))
            {
                return false;
            }

            Set(Current - amount);
            return true;
        }

        public void Add(int amount)
        {
            Set(Current + amount);
        }
    }
}
