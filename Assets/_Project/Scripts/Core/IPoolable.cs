namespace LawnDefense.Core
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
