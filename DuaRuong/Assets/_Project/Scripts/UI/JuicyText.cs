using UnityEngine;
using TMPro;

namespace DuaRuong.UI
{
    /// <summary>
    /// Utility to add "juice" to TMP_Text elements.
    /// Provides a scale pop effect when triggered.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class JuicyText : MonoBehaviour
    {
        [Header("Pop Settings")]
        [SerializeField] private float _popScale = 1.2f;
        [SerializeField] private float _duration = 0.2f;
        [SerializeField] private AnimationCurve _popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Vector3 _baseScale;
        private float _timer;
        private bool _isPopping;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            if (!_isPopping) return;

            _timer += Time.unscaledDeltaTime;
            float progress = _timer / _duration;

            if (progress >= 1f)
            {
                transform.localScale = _baseScale;
                _isPopping = false;
            }
            else
            {
                float scaleMultiplier = Mathf.Lerp(1f, _popScale, _popCurve.Evaluate(progress));
                // Simple up then down curve if evaluation is linear
                // Better to use a curve that goes 0->1->0
                transform.localScale = _baseScale * scaleMultiplier;
            }
        }

        public void PlayPop()
        {
            _timer = 0;
            _isPopping = true;
        }
    }
}
