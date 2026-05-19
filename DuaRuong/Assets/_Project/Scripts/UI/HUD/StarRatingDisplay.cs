using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DuaRuong.UI.HUD
{
    /// <summary>
    /// Reveals 1-3 stars sequentially with a scale-punch animation on the Game Over screen.
    ///
    /// Setup in Inspector:
    ///   • _starImages[0..2] — three Image components (filled sprite = bright, unfilled = dim)
    ///   • _filledSprite / _emptySprite — swap sprites per result
    ///   • _filledColor / _emptyColor   — gold vs grey
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StarRatingDisplay : MonoBehaviour
    {
        [SerializeField] private Image[] _starImages = new Image[3];

        [Header("Sprites & Colors")]
        [SerializeField] private Sprite _filledSprite;
        [SerializeField] private Sprite _emptySprite;
        [SerializeField] private Color  _filledColor = new Color(1.00f, 0.85f, 0.15f); // gold
        [SerializeField] private Color  _emptyColor  = new Color(0.45f, 0.45f, 0.45f); // grey

        [Header("Animation")]
        [SerializeField] private float _revealDelay  = 0.35f; // seconds between each star
        [SerializeField] private float _punchScale   = 1.6f;
        [SerializeField] private float _punchDuration = 0.30f;
        [SerializeField] private float _settleScale  = 1.05f; // slight overscale that holds
        [SerializeField] private float _settleDuration = 0.15f;

        private Coroutine _revealRoutine;

        private void OnEnable()
        {
            // Reset all stars to empty so they're invisible before Show() is called
            foreach (var img in _starImages)
                SetStarState(img, false);
        }

        public void Show(int starCount)
        {
            if (_revealRoutine != null) StopCoroutine(_revealRoutine);
            _revealRoutine = StartCoroutine(RevealSequence(starCount));
        }

        private IEnumerator RevealSequence(int count)
        {
            // First show all as empty (reset)
            foreach (var img in _starImages)
                SetStarState(img, false);

            yield return new WaitForSecondsRealtime(0.2f);

            for (int i = 0; i < _starImages.Length; i++)
            {
                bool earned = i < count;
                SetStarState(_starImages[i], earned);

                if (earned)
                    yield return StartCoroutine(PunchStar(_starImages[i].transform));
                else
                    yield return new WaitForSecondsRealtime(_revealDelay * 0.5f);

                yield return new WaitForSecondsRealtime(_revealDelay);
            }
        }

        private IEnumerator PunchStar(Transform t)
        {
            // Scale from 0 → punchScale → settleScale
            float elapsed = 0f;
            while (elapsed < _punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / _punchDuration);
                // bell: 0→punchScale→settleScale
                float bell  = Mathf.Sin(progress * Mathf.PI);
                float scale = _settleScale + (_punchScale - _settleScale) * bell;
                t.localScale = Vector3.one * scale;
                yield return null;
            }

            // Ease settle scale back to 1
            elapsed = 0f;
            while (elapsed < _settleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float s = Mathf.Lerp(_settleScale, 1f, elapsed / _settleDuration);
                t.localScale = Vector3.one * s;
                yield return null;
            }

            t.localScale = Vector3.one;
        }

        private void SetStarState(Image img, bool filled)
        {
            if (img == null) return;
            if (_filledSprite != null && _emptySprite != null)
                img.sprite = filled ? _filledSprite : _emptySprite;
            img.color = filled ? _filledColor : _emptyColor;
            img.transform.localScale = Vector3.one;
        }
    }
}
