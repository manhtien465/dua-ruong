using DuaRuong.Core;
using DuaRuong.Gameplay.Items;
using DuaRuong.Gameplay.Obstacles;
using UnityEngine;

namespace DuaRuong.Gameplay.Player
{
    /// <summary>
    /// Detects collisions with obstacles and items via trigger volumes.
    /// Collision is suppressed during tier transitions to prevent false hits
    /// from the adjacent tier's obstacles while the player is lerping between levels.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class PlayerCollision : MonoBehaviour
    {
        private PlayerMovement _movement;

        private void Awake() => _movement = GetComponent<PlayerMovement>();

        private void OnTriggerEnter(Collider other)
        {
            // Skip during tier transition — capsule overlaps adjacent tier's volume briefly.
            if (_movement != null && _movement.IsTierChanging) return;

            if (other.TryGetComponent(out ItemBase item))
            {
                item.Collect();
                return;
            }

            if (other.TryGetComponent(out ObstacleBase obstacle))
            {
                obstacle.OnPlayerHit();
                GameEvents.RaisePlayerHit();
            }
        }
    }
}
