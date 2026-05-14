using DuaRuong.Systems.Pool;
using UnityEngine;

namespace DuaRuong.Gameplay.Obstacles
{
    /// <summary>
    /// Base for static obstacles. Subclasses can override <see cref="OnPlayerHit"/>
    /// for type-specific reactions (knockback, screen shake, etc.).
    /// Trigger collider on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class ObstacleBase : MonoBehaviour, IPoolable
    {
        [SerializeField] protected ObstacleType _type;

        public ObstacleType Type => _type;

        public virtual void OnSpawned() { }
        public virtual void OnDespawned() { }
        public virtual void OnPlayerHit() { }
    }
}
