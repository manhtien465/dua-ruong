using DuaRuong.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DuaRuong.UI.HUD
{
    /// <summary>
    /// Thin progress bar showing how close the player is to the next combo tier.
    /// Auto-hides when combo == 0. Changes color and pulses on tier-up.
    ///
    /// Setup in Inspector:
    ///   • _fillImage  — Image with Fill Method = Horizontal
    ///   • _group      — CanvasGroup on the bar's root (for show/hide alpha)
    ///   • _thresholds — must match ScoringConfig (default: 0, 10, 25, 50)
    ///   • _tierColors — one color per threshold (white, yellow, orange, red)
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ComboBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private CanvasGroup _group;

        [Header("Tiers — must match ScoringConfig")]
        [SerializeField] private int[]   _thresholds = { 0, 10, 25, 50 };
        [SerializeField] private Color[] _tierColors =
        {
            new Color(0.90f, 0.90f, 0.90f), // tier 0 — white
            new Color(1.00f, 0.90f, 0.20f), // tier 1 — yellow
            new Color(1.00f, 0.55f, 0.10f), // tier 2 — orange
            new Color(1.00f, 0.25f, 0.15f), // tier 3 — red-orange
        };

        [Header("Animation")]
        [SerializeField] private float _showHideSpeed = 6f;
        [SerializeField] private float _pulseDuration = 0.25f;
        [SerializeField] private float _pulseScale    = 1.15f;

        private int   _currentCombo;
        private int   _currentTier = -1;
        private float _targetAlpha;
        private float _pulseTimer;
        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
            if (_group != null) _group.alpha = 0f;
        }

        private void OnEnable()
        {
            GameEvents.ComboChanged        += HandleCombo;
            GameEvents.ComboMilestoneReached += HandleMilestone;
            GameEvents.GameStarted          += HandleGameStarted;
        }

        private void OnDisable()
        {
            GameEvents.ComboChanged          -= HandleCombo;
            GameEvents.ComboMilestoneReached -= HandleMilestone;
            GameEvents.GameStarted           -= HandleGameStarted;
        }

        private void HandleGameStarted()
        {
            _currentCombo = 0;
            _currentTier  = -1;
            _targetAlpha  = 0f;
            if (_fillImage != null) _fillImage.fillAmount = 0f;
        }

        private void HandleCombo(int combo)
        {
            _currentCombo = combo;
            _targetAlpha  = combo > 0 ? 1f : 0f;
            Refresh();
        }

        private void HandleMilestone(float _) => _pulseTimer = _pulseDuration;

        private void Refresh()
        {
            if (_fillImage == null || _thresholds.Length < 2) return;

            int tier     = GetTier(_currentCombo);
            bool atMax   = tier >= _thresholds.Length - 1;

            // Fill: progress within current tier to next tier
            float fill = 1f;
            if (!atMax)
            {
                int low  = _thresholds[tier];
                int high = _thresholds[tier + 1];
                fill = Mathf.Clamp01((float)(_currentCombo - low) / (high - low));
            }

            _fillImage.fillAmount = fill;

            // Color
            if (tier != _currentTier && tier < _tierColors.Length)
            {
                _currentTier         = tier;
                _fillImage.color     = _tierColors[tier];
            }
        }

        private void Update()
        {
            // Smooth show/hide
            if (_group != null)
                _group.alpha = Mathf.MoveTowards(_group.alpha, _targetAlpha,
                    _showHideSpeed * Time.deltaTime);

            // Tier-up pulse
            if (_pulseTimer > 0f)
            {
                _pulseTimer -= Time.deltaTime;
                float t    = 1f - Mathf.Clamp01(_pulseTimer / _pulseDuration);
                float bell = Mathf.Sin(t * Mathf.PI);
                transform.localScale = _baseScale * (1f + (_pulseScale - 1f) * bell);
            }
            else
            {
                transform.localScale = _baseScale;
            }
        }

        private int GetTier(int combo)
        {
            int tier = 0;
            for (int i = 0; i < _thresholds.Length; i++)
                if (combo >= _thresholds[i]) tier = i;
            return tier;
        }
    }
}
