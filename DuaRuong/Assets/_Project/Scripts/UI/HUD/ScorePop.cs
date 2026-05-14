using DuaRuong.Core;
using UnityEngine;

namespace DuaRuong.UI.HUD
{
    /// <summary>
    /// Brief scale-punch on the score text when the player collects an item.
    /// Subscribes only to item events (not distance) so it doesn't fire every frame.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public sealed class ScorePop : MonoBehaviour
    {
        [SerializeField, Range(1.05f, 1.5f)] private float _peakScale  = 1.25f;
        [SerializeField, Min(0.05f)]          private float _duration   = 0.22f;

        private RectTransform _rt;
        private Vector3       _baseScale;
        private float         _timer;

        private void Awake()
        {
            _rt        = GetComponent<RectTransform>();
            _baseScale = _rt.localScale;
        }

        private void OnEnable()
        {
            GameEvents.RiceCollected        += OnItem;
            GameEvents.GoldBuffaloCollected += OnItem;
        }

        private void OnDisable()
        {
            GameEvents.RiceCollected        -= OnItem;
            GameEvents.GoldBuffaloCollected -= OnItem;
        }

        private void OnItem(int _) => _timer = _duration;

        private void Update()
        {
            if (_timer <= 0f)
            {
                _rt.localScale = _baseScale;
                return;
            }

            _timer -= Time.deltaTime;
            float t     = _timer / _duration;                       // 1→0
            float scale = Mathf.Lerp(1f, _peakScale, EaseOutQuad(t));
            _rt.localScale = _baseScale * scale;
        }

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    }
}
