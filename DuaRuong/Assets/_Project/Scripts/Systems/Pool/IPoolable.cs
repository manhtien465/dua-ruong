namespace DuaRuong.Systems.Pool
{
    /// <summary>
    /// Implement on pooled prefabs to receive lifecycle hooks from <see cref="ObjectPool{T}"/>.
    /// Use these hooks to reset state instead of relying on Awake/OnEnable side effects.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
