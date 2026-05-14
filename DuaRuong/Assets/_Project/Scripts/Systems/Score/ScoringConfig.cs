using UnityEngine;

namespace DuaRuong.Systems.Score
{
    [CreateAssetMenu(fileName = "ScoringConfig", menuName = "DuaRuong/Configs/Scoring")]
    public sealed class ScoringConfig : ScriptableObject
    {
        [Header("Distance")]
        [SerializeField, Min(0f)] private float _distanceScorePerMeter = 1f;

        [Header("Items")]
        [SerializeField, Min(0)] private int _riceScore = 10;
        [SerializeField, Min(0)] private int _goldBuffaloScore = 200;

        [Header("Combo Multiplier")]
        [Tooltip("Combo thresholds (ascending). Each entry pairs with the matching multiplier index.")]
        [SerializeField] private int[] _comboThresholds = { 0, 10, 25, 50 };
        [SerializeField] private float[] _comboMultipliers = { 1f, 1.5f, 2f, 3f };

        public float DistanceScorePerMeter => _distanceScorePerMeter;
        public int RiceScore => _riceScore;
        public int GoldBuffaloScore => _goldBuffaloScore;

        /// <summary>Returns the multiplier for a given combo count.</summary>
        public float GetMultiplier(int combo)
        {
            float multiplier = 1f;
            for (int i = 0; i < _comboThresholds.Length; i++)
            {
                if (combo >= _comboThresholds[i] && i < _comboMultipliers.Length)
                {
                    multiplier = _comboMultipliers[i];
                }
            }
            return multiplier;
        }

        private void OnValidate()
        {
            if (_comboThresholds.Length != _comboMultipliers.Length)
            {
                Debug.LogWarning(
                    $"[{nameof(ScoringConfig)}] Threshold and multiplier arrays must be the same length.",
                    this);
            }
        }
    }
}
