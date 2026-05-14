using DuaRuong.Core;
using TMPro;
using UnityEngine;

namespace DuaRuong.UI.HUD
{
    /// <summary>
    /// Shows a brief "COMBO ×2!" popup when the score multiplier tier increases.
    /// Fades out automatically. No coroutines — driven by Update timer.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ComboPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private CanvasGroup _group;

        [SerializeField, Min(0.05f)] private float _holdDuration = 0.5f;
        [SerializeField, Min(0.05f)] private float _fadeDuration = 0.7f;

        private float _timer;

        private void OnEnable()  => GameEvents.ComboMilestoneReached += HandleMilestone;
        private void OnDisable() => GameEvents.ComboMilestoneReached -= HandleMilestone;

        private void HandleMilestone(float multiplier)
        {
            if (_text != null)
                _text.text = $"COMBO ×{multiplier:g}!";

            _timer = _holdDuration + _fadeDuration;
            if (_group != null) _group.alpha = 1f;
        }

        private void Update()
        {
            if (_timer <= 0f) return;

            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                _timer = 0f;
                if (_group != null) _group.alpha = 0f;
                return;
            }

            if (_timer < _fadeDuration && _group != null)
                _group.alpha = _timer / _fadeDuration;
        }
    }
}
