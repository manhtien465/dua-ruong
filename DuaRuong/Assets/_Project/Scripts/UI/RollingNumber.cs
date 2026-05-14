using System.Collections;
using TMPro;
using UnityEngine;

namespace DuaRuong.UI
{
    /// <summary>
    /// Utility to animate a numeric text value from current to target.
    /// Great for score summaries and rewards.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class RollingNumber : MonoBehaviour
    {
        [SerializeField] private float _duration = 1.5f;
        [SerializeField] private string _format = "{0:N0}";
        [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private TMP_Text _text;
        private Coroutine _rollingRoutine;

        private void Awake() => _text = GetComponent<TMP_Text>();

        public void SetNumber(int target, int startValue = 0)
        {
            if (_rollingRoutine != null) StopCoroutine(_rollingRoutine);
            _rollingRoutine = StartCoroutine(RollingRoutine(startValue, target));
        }

        private IEnumerator RollingRoutine(int start, int end)
        {
            float elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                float value = Mathf.Lerp(start, end, _curve.Evaluate(t));
                _text.SetText(_format, Mathf.FloorToInt(value));
                yield return null;
            }
            _text.SetText(_format, end);
        }
    }
}
