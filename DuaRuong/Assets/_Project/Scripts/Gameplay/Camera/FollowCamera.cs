using DuaRuong.Core;
using UnityEngine;

namespace DuaRuong.Gameplay.Cam
{
    /// <summary>
    /// Smooth third-person follow camera. Snaps instantly to target on game start
    /// so play-again doesn't drift from old position.
    /// Adds a brief positional shake on PlayerHit for game-feel feedback.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _localOffset = new(0f, 4f, -7f);
        [SerializeField, Range(0f, 30f)] private float _lookDownAngle = 12f;
        [SerializeField, Range(0.05f, 1f)] private float _smoothTime = 0.15f;

        [Header("Hit Shake")]
        [SerializeField, Min(0f)] private float _shakeDuration  = 0.30f;
        [SerializeField, Min(0f)] private float _shakeMagnitude = 0.20f;

        private Vector3 _velocity;
        private float   _shakeTimer;

        private void OnEnable()
        {
            GameEvents.GameStarted += SnapToTarget;
            GameEvents.PlayerHit   += TriggerShake;
        }

        private void OnDisable()
        {
            GameEvents.GameStarted -= SnapToTarget;
            GameEvents.PlayerHit   -= TriggerShake;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired = _target.position + _localOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, _smoothTime);
            transform.rotation = Quaternion.Euler(_lookDownAngle, 0f, 0f);

            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                float strength = (_shakeTimer / _shakeDuration) * _shakeMagnitude;
                transform.position += Random.insideUnitSphere * strength;
            }
        }

        public void SetTarget(Transform t) => _target = t;

        private void TriggerShake() => _shakeTimer = _shakeDuration;

        private void SnapToTarget()
        {
            if (_target == null) return;
            _velocity = Vector3.zero;
            _shakeTimer = 0f;
            transform.position = _target.position + _localOffset;
        }
    }
}
