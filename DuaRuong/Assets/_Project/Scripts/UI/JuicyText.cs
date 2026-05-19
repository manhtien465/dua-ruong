using UnityEngine;
using TMPro;

namespace DuaRuong.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class JuicyText : MonoBehaviour
    {
        [Header("Pop Settings")]
        [SerializeField] private float _popScale = 1.2f;
        [SerializeField] private float _duration = 0.22f;

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
            float t = _timer / _duration;

            if (t >= 1f)
            {
                transform.localScale = _baseScale;
                _isPopping = false;
                return;
            }

            // Bell curve: 0 → peak → 0, so scale goes base → popScale → base
            float bell = Mathf.Sin(t * Mathf.PI);
            transform.localScale = _baseScale * (1f + (_popScale - 1f) * bell);
        }

        public void PlayPop()
        {
            _timer = 0f;
            _isPopping = true;
        }
    }
}
