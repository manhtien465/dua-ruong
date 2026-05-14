using DuaRuong.Core;
using UnityEngine;

namespace DuaRuong.Systems.Score
{
    /// <summary>
    /// Aggregates score from distance + item events. Combo multiplier sourced from <see cref="ComboSystem"/>.
    /// Distance score is integer-truncated per <see cref="ScoringConfig.DistanceScorePerMeter"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ComboSystem))]
    public sealed class ScoreSystem : MonoBehaviour
    {
        [SerializeField] private ScoringConfig _config;

        private ComboSystem _combo;

        public int Score { get; private set; }
        public float Distance { get; private set; }

        private float _distanceAccumulator;
        private float _prevMultiplier = 1f;

        private void Awake()
        {
            _combo = GetComponent<ComboSystem>();
            if (_config == null)
            {
                Debug.LogError($"[{nameof(ScoreSystem)}] ScoringConfig is not assigned.", this);
            }
        }

        private void OnEnable()
        {
            GameEvents.RiceCollected += HandleRice;
            GameEvents.GoldBuffaloCollected += HandleGold;
            GameEvents.GameStarted += HandleGameStarted;
        }

        private void OnDisable()
        {
            GameEvents.RiceCollected -= HandleRice;
            GameEvents.GoldBuffaloCollected -= HandleGold;
            GameEvents.GameStarted -= HandleGameStarted;
        }

        public void AddDistance(float meters)
        {
            if (meters <= 0f || _config == null) return;

            Distance += meters;
            GameEvents.RaiseDistanceChanged(Distance);

            _distanceAccumulator += meters * _config.DistanceScorePerMeter;
            int whole = Mathf.FloorToInt(_distanceAccumulator);
            if (whole > 0)
            {
                _distanceAccumulator -= whole;
                AddScore(whole);
            }
        }

        private void HandleRice(int amount)
        {
            if (_config == null) return;
            float mult = _config.GetMultiplier(_combo.Combo);
            CheckMilestone(mult);
            AddScore(Mathf.RoundToInt(amount * _config.RiceScore * mult));
        }

        private void HandleGold(int amount)
        {
            if (_config == null) return;
            float mult = _config.GetMultiplier(_combo.Combo);
            CheckMilestone(mult);
            AddScore(Mathf.RoundToInt(amount * _config.GoldBuffaloScore * mult));
        }

        private void CheckMilestone(float currentMultiplier)
        {
            if (currentMultiplier > _prevMultiplier)
            {
                _prevMultiplier = currentMultiplier;
                GameEvents.RaiseComboMilestoneReached(currentMultiplier);
            }
        }

        private void HandleGameStarted()
        {
            Score = 0;
            Distance = 0f;
            _distanceAccumulator = 0f;
            _prevMultiplier = 1f;
            GameEvents.RaiseScoreChanged(0);
            GameEvents.RaiseDistanceChanged(0f);
        }

        private void AddScore(int amount)
        {
            Score += amount;
            GameEvents.RaiseScoreChanged(Score);
        }
    }
}
