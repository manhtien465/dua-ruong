using DuaRuong.Core;
using UnityEngine;

namespace DuaRuong.Gameplay.Player
{
    /// <summary>
    /// Drives forward run + tier climbing (Y-axis) + lateral dodge (X-axis).
    /// Three tiers represent terraced rice field levels. SwipeUp/Down climbs/drops a tier.
    /// SwipeLeft/Right snaps a short dodge that auto-returns to center.
    /// Position is fully script-controlled — no Rigidbody physics involvement.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _config;

        private const int TierCount = 3;
        private const int StartTier = 1;   // middle tier

        // Tier (Y axis)
        private int   _currentTier;
        private float _tierStartY;
        private float _tierTargetY;
        private float _tierChangeT = 1f;   // 1 = not changing

        // Dodge (X axis, always returns to 0)
        private float _dodgeX;
        private float _dodgeXVelocity;

        public float CurrentSpeed      { get; private set; }
        public float DistanceTravelled { get; private set; }
        public int   CurrentTier       => _currentTier;

        /// <summary>True while the tier Y-lerp is in flight. Collision is suppressed during this window.</summary>
        public bool IsTierChanging => _tierChangeT < 1f;

        private void Awake()
        {
            if (_config == null) Debug.LogError($"[{nameof(PlayerMovement)}] PlayerConfig missing.", this);
            CurrentSpeed = _config != null ? _config.StartSpeed : 8f;
        }

        public void ResetState()
        {
            _currentTier     = StartTier;
            _tierStartY      = _config.GetTierY(StartTier);
            _tierTargetY     = _tierStartY;
            _tierChangeT     = 1f;
            _dodgeX          = 0f;
            _dodgeXVelocity  = 0f;
            DistanceTravelled = 0f;
            CurrentSpeed     = _config.StartSpeed;

            var p = transform.position;
            transform.position = new Vector3(0f, _tierTargetY, p.z);
        }

        public void Tick(float dt)
        {
            if (_config == null) return;

            // Forward
            float deltaZ = CurrentSpeed * dt;
            DistanceTravelled += deltaZ;
            CurrentSpeed = Mathf.Min(
                _config.MaxSpeed,
                _config.StartSpeed + DistanceTravelled * _config.AccelerationPerMeter);

            // Tier Y lerp
            if (_tierChangeT < 1f)
            {
                _tierChangeT += dt / _config.TierChangeDuration;
                if (_tierChangeT > 1f) _tierChangeT = 1f;
            }
            float currentY = Mathf.Lerp(_tierStartY, _tierTargetY, EaseOutQuad(_tierChangeT));

            // Dodge X auto-return to 0
            _dodgeX = Mathf.SmoothDamp(_dodgeX, 0f, ref _dodgeXVelocity, _config.DodgeReturnDuration);

            var p = transform.position;
            transform.position = new Vector3(_dodgeX, currentY, p.z + deltaZ);
        }

        /// <summary>Climb one tier level (swipe up). No-op at top tier.</summary>
        public void TryClimbTier()
        {
            int next = _currentTier + 1;
            if (next >= TierCount) return;
            BeginTierChange(next);
        }

        /// <summary>Drop one tier level (swipe down). No-op at bottom tier.</summary>
        public void TryDropTier()
        {
            int next = _currentTier - 1;
            if (next < 0) return;
            BeginTierChange(next);
        }

        /// <summary>Quick lateral dodge. direction: -1 (left) or +1 (right). Auto-returns to center.</summary>
        public void TryDodge(int direction)
        {
            if (_config == null) return;
            _dodgeX = Mathf.Clamp(
                direction * _config.DodgeDistance,
                -_config.DodgeDistance,
                _config.DodgeDistance);
            _dodgeXVelocity = 0f;
        }

        private void BeginTierChange(int nextTier)
        {
            int direction = nextTier > _currentTier ? 1 : -1;
            _tierStartY  = Mathf.Lerp(_tierStartY, _tierTargetY, EaseOutQuad(_tierChangeT));
            _tierTargetY = _config.GetTierY(nextTier);
            _tierChangeT = 0f;
            _currentTier = nextTier;
            GameEvents.RaiseTierChanged(direction);
        }

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    }
}
