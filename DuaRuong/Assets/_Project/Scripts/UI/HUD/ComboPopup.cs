using DuaRuong.Core;
using TMPro;
using UnityEngine;

namespace DuaRuong.UI.HUD
{
    /// <summary>
    /// Shows a "COMBO ×2!" popup when the multiplier tier increases.
    /// Scale-punches in, then fades out. No coroutines — driven by Update timer.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ComboPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private CanvasGroup _group;

        [SerializeField, Min(0.05f)] private float _holdDuration  = 0.5f;
        [SerializeField, Min(0.05f)] private float _fadeDuration  = 0.7f;
        [SerializeField, Min(0.05f)] private float _punchDuration = 0.25f;
        [SerializeField, Range(1f, 2f)] private float _punchScale = 1.35f;

        [Header("Tier Colors")]
        [SerializeField] private Color _color15 = new Color(1.0f, 0.95f, 0.30f); // ×1.5 — yellow
        [SerializeField] private Color _color20 = new Color(1.0f, 0.60f, 0.10f); // ×2   — orange
        [SerializeField] private Color _color30 = new Color(1.0f, 0.25f, 0.15f); // ×3   — red-orange

        private float _totalTimer;
        private float _punchTimer;
        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void OnEnable()  => GameEvents.ComboMilestoneReached += HandleMilestone;
        private void OnDisable() => GameEvents.ComboMilestoneReached -= HandleMilestone;

        private void HandleMilestone(float multiplier)
        {
            if (_text != null)
            {
                _text.text  = $"COMBO ×{multiplier:g}!";
                _text.color = GetTierColor(multiplier);
            }

            _totalTimer = _holdDuration + _fadeDuration;
            _punchTimer = _punchDuration;
            if (_group != null) _group.alpha = 1f;
        }

        private Color GetTierColor(float m) =>
            m >= 3f ? _color30 :
            m >= 2f ? _color20 :
            _color15;

        private void Update()
        {
            // Scale punch — bell curve so it returns cleanly to base
            if (_punchTimer > 0f)
            {
                _punchTimer -= Time.deltaTime;
                float t    = 1f - Mathf.Clamp01(_punchTimer / _punchDuration);
                float bell = Mathf.Sin(t * Mathf.PI);
                transform.localScale = _baseScale * (1f + (_punchScale - 1f) * bell);
            }
            else
            {
                transform.localScale = _baseScale;
            }

            // Fade out
            if (_totalTimer <= 0f) return;
            _totalTimer -= Time.deltaTime;

            if (_totalTimer <= 0f)
            {
                _totalTimer = 0f;
                if (_group != null) _group.alpha = 0f;
                return;
            }

            if (_totalTimer < _fadeDuration && _group != null)
                _group.alpha = _totalTimer / _fadeDuration;
        }
    }
}
