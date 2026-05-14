using DuaRuong.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DuaRuong.UI
{
    /// <summary>
    /// Full-screen red flash when the player hits an obstacle. Pure visual feedback.
    /// Attach to a stretched Image GO inside Canvas (raycastTarget = false).
    /// </summary>
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public sealed class HitFlash : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _peakAlpha    = 0.45f;
        [SerializeField, Min(0.05f)]    private float _fadeDuration  = 0.35f;

        private static readonly Color FlashColor = new(1f, 0.08f, 0.08f);

        private Image _image;
        private float _alpha;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.raycastTarget = false;
            _image.color = new Color(FlashColor.r, FlashColor.g, FlashColor.b, 0f);
        }

        private void OnEnable()  => GameEvents.PlayerHit += Flash;
        private void OnDisable() => GameEvents.PlayerHit -= Flash;

        private void Flash() => _alpha = _peakAlpha;

        private void Update()
        {
            if (_alpha <= 0f) return;
            _alpha = Mathf.MoveTowards(_alpha, 0f, Time.deltaTime / _fadeDuration);
            _image.color = new Color(FlashColor.r, FlashColor.g, FlashColor.b, _alpha);
        }
    }
}
