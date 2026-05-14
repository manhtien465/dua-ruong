using DuaRuong.Core;
using DuaRuong.Systems.Score;
using UnityEngine;

namespace DuaRuong.Gameplay.Player
{
    /// <summary>
    /// Bridges <see cref="PlayerMovement"/> with <see cref="ScoreSystem"/>.
    /// Lives separately so neither system depends on the other directly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DistanceReporter : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private ScoreSystem _scoreSystem;

        private float _lastDistance;
        private bool _running;

        private void OnEnable()
        {
            GameEvents.GameStarted += HandleStarted;
            GameEvents.GameOver += HandleEnded;
        }

        private void OnDisable()
        {
            GameEvents.GameStarted -= HandleStarted;
            GameEvents.GameOver -= HandleEnded;
        }

        private void Update()
        {
            if (!_running || _movement == null || _scoreSystem == null) return;

            float current = _movement.DistanceTravelled;
            float delta = current - _lastDistance;
            if (delta > 0f)
            {
                _scoreSystem.AddDistance(delta);
                _lastDistance = current;
            }
        }

        private void HandleStarted()
        {
            _lastDistance = 0f;
            _running = true;
        }

        private void HandleEnded() => _running = false;
    }
}
