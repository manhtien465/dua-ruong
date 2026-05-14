using UnityEngine;

namespace DuaRuong.Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "DuaRuong/Configs/Player")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Header("Run Speed")]
        [SerializeField, Min(0f)] private float _startSpeed = 8f;
        [SerializeField, Min(0f)] private float _maxSpeed = 18f;
        [SerializeField, Min(0f), Tooltip("Speed gained per meter travelled.")]
        private float _accelerationPerMeter = 0.0005f;

        [Header("Tiers")]
        [SerializeField] private float[] _tierY = { 0.5f, 2.5f, 4.5f };
        [SerializeField, Range(0.05f, 0.5f)] private float _tierChangeDuration = 0.2f;

        [Header("Dodge")]
        [SerializeField, Min(0.1f)] private float _dodgeDistance = 0.8f;
        [SerializeField, Range(0.05f, 0.5f)] private float _dodgeReturnDuration = 0.25f;

        public float StartSpeed => _startSpeed;
        public float MaxSpeed => _maxSpeed;
        public float AccelerationPerMeter => _accelerationPerMeter;
        public float[] TierY => _tierY;
        public float TierChangeDuration => _tierChangeDuration;
        public float DodgeDistance => _dodgeDistance;
        public float DodgeReturnDuration => _dodgeReturnDuration;

        public float GetTierY(int tier)
        {
            if (_tierY == null || _tierY.Length == 0) return 0.5f;
            return _tierY[Mathf.Clamp(tier, 0, _tierY.Length - 1)];
        }
    }
}
