using DuaRuong.Core;
using DuaRuong.Systems.Pool;
using UnityEngine;

namespace DuaRuong.Gameplay.Items
{
    /// <summary>
    /// Base for collectible items. Sets <see cref="GameEvents"/> on collect; the game's
    /// Score / Combo systems decide what to do with the data.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class ItemBase : MonoBehaviour, IPoolable
    {
        [SerializeField] protected ItemType _type;
        [SerializeField, Min(1)] protected int _value = 1;

        public ItemType Type => _type;

        public virtual void OnSpawned() { }
        public virtual void OnDespawned() { }

        public void Collect()
        {
            switch (_type)
            {
                case ItemType.Rice:
                    GameEvents.RaiseRiceCollected(_value);
                    break;
                case ItemType.GoldBuffalo:
                    GameEvents.RaiseGoldBuffaloCollected(_value);
                    break;
            }
            OnCollected();
        }

        protected abstract void OnCollected();
    }
}
