using UnityEngine;

namespace DuaRuong.UI
{
    /// <summary>
    /// Fades a panel in via CanvasGroup and adds a scale-up effect whenever the GameObject is activated.
    /// Works automatically with SetActive(true) — no controller changes needed.
    /// Uses unscaledDeltaTime so the Pause panel animates even when timeScale = 0.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [DisallowMultipleComponent]
    public sealed class PanelFadeIn : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float _duration = 0.25f;
        [SerializeField] private float _startScale = 0.9f;

        private CanvasGroup _group;
        private float _elapsed;
        private bool  _animating;
        private Vector3 _baseScale;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            _group.alpha          = 0f;
            _group.blocksRaycasts = false;
            _elapsed              = 0f;
            _animating            = true;
            transform.localScale = _baseScale * _startScale;
        }

        private void Update()
        {
            if (!_animating) return;

            _elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            float easedT = EaseOutCubic(t);

            _group.alpha = easedT;
            transform.localScale = Vector3.Lerp(_baseScale * _startScale, _baseScale, easedT);

            if (t >= 1f)
            {
                _animating            = false;
                _group.blocksRaycasts = true;
                transform.localScale = _baseScale;
            }
        }

        private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    }
}
