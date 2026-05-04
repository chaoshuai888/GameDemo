using LawnDefense.Data;

namespace LawnDefense.Combat
{
    public struct DamageInfo
    {
        public int Amount;
        public DamageType DamageType;
        public object Source;

        public DamageInfo(int amount, DamageType damageType, object source)
        {
            Amount = amount;
            DamageType = damageType;
            Source = source;
        }
    }
}
