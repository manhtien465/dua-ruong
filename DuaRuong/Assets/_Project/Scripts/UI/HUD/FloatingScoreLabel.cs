using TMPro;
using UnityEngine;

namespace DuaRuong.UI.HUD
{
    /// <summary>
    /// Pooled floating "+X" label that rises and fades after an item is collected.
    /// Managed by <see cref="FloatingScoreLauncher"/>. Place inside a Screen-Space Canvas.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [DisallowMultipleComponent]
    public sealed class FloatingScoreLabel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _lifetime     = 0.95f;
        [SerializeField] private float _riseSpeed    = 110f;   // UI units per second
        [SerializeField] private float _fadeStart    = 0.50f;  // fraction of lifetime when fade begins
        [SerializeField] private float _punchScale   = 1.40f;
        [SerializeField] private float _punchDuration = 0.18f;

        private CanvasGroup _group;
        private RectTransform _rt;
        private float _timer;
        private float _punchTimer;

        public System.Action<FloatingScoreLabel> Released;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _rt    = GetComponent<RectTransform>();
        }

        public void Spawn(string label, Color color, Vector2 anchoredPos)
        {
            if (_text != null) { _text.text = label; _text.color = color; }
            _rt.anchoredPosition = anchoredPos;
            _timer      = _lifetime;
            _punchTimer = _punchDuration;
            _group.alpha = 1f;
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (_timer <= 0f) return;

            _timer -= Time.deltaTime;

            _rt.anchoredPosition += Vector2.up * (_riseSpeed * Time.deltaTime);

            // Fade
            float fadeThreshold = _lifetime * _fadeStart;
            if (_timer < fadeThreshold)
                _group.alpha = Mathf.Clamp01(_timer / fadeThreshold);

            // Scale punch — bell so it snaps back cleanly
            if (_punchTimer > 0f)
            {
                _punchTimer -= Time.deltaTime;
                float t    = 1f - Mathf.Clamp01(_punchTimer / _punchDuration);
                float bell = Mathf.Sin(t * Mathf.PI);
                transform.localScale = Vector3.one * (1f + (_punchScale - 1f) * bell);
            }
            else
            {
                transform.localScale = Vector3.one;
            }

            if (_timer <= 0f)
            {
                _timer = 0f;
                gameObject.SetActive(false);
                Released?.Invoke(this);
            }
        }
    }
}
