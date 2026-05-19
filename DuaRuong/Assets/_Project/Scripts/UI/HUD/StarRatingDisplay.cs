using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DuaRuong.UI.HUD
{
    /// <summary>
    /// Reveals 1-3 stars on the Game Over screen with a scale-punch animation.
    /// Zero Inspector setup: if _starImages is empty, creates 3 Image children at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StarRatingDisplay : MonoBehaviour
    {
        [SerializeField] private Image[] _starImages = new Image[0]; // populated at runtime if empty

        [Header("Colors")]
        [SerializeField] private Color _filledColor = new Color(1.00f, 0.85f, 0.15f);
        [SerializeField] private Color _emptyColor  = new Color(0.40f, 0.40f, 0.40f);

        [Header("Animation")]
        [SerializeField] private float _revealDelay   = 0.35f;
        [SerializeField] private float _punchScale    = 1.6f;
        [SerializeField] private float _punchDuration = 0.30f;
        [SerializeField] private float _settleScale   = 1.05f;
        [SerializeField] private float _settleDuration = 0.15f;

        private Coroutine _revealRoutine;

        private void Awake()
        {
            if (_starImages == null || _starImages.Length < 3)
                EnsureStars();
        }

        private void OnEnable()
        {
            foreach (var img in _starImages)
                SetState(img, false);
        }

        public void Show(int starCount)
        {
            if (_revealRoutine != null) StopCoroutine(_revealRoutine);
            _revealRoutine = StartCoroutine(RevealSequence(Mathf.Clamp(starCount, 0, 3)));
        }

        private IEnumerator RevealSequence(int count)
        {
            foreach (var img in _starImages)
                SetState(img, false);

            yield return new WaitForSecondsRealtime(0.2f);

            for (int i = 0; i < _starImages.Length; i++)
            {
                bool earned = i < count;
                SetState(_starImages[i], earned);
                if (earned)
                    yield return StartCoroutine(PunchStar(_starImages[i].transform));
                else
                    yield return new WaitForSecondsRealtime(_revealDelay * 0.5f);

                yield return new WaitForSecondsRealtime(_revealDelay);
            }
        }

        private IEnumerator PunchStar(Transform t)
        {
            float elapsed = 0f;
            while (elapsed < _punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float bell  = Mathf.Sin(Mathf.Clamp01(elapsed / _punchDuration) * Mathf.PI);
                float scale = _settleScale + (_punchScale - _settleScale) * bell;
                t.localScale = Vector3.one * scale;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < _settleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                t.localScale = Vector3.one * Mathf.Lerp(_settleScale, 1f, elapsed / _settleDuration);
                yield return null;
            }

            t.localScale = Vector3.one;
        }

        private void SetState(Image img, bool filled)
        {
            if (img == null) return;
            img.color = filled ? _filledColor : _emptyColor;
            img.transform.localScale = Vector3.one;
        }

        // Creates 3 star Image children if not already assigned in Inspector
        private void EnsureStars()
        {
            _starImages = new Image[3];
            float spacing = 90f;

            for (int i = 0; i < 3; i++)
            {
                var existing = transform.Find($"Star_{i}");
                if (existing != null)
                {
                    _starImages[i] = existing.GetComponent<Image>();
                    continue;
                }

                var go = new GameObject($"Star_{i}");
                go.transform.SetParent(transform, false);

                var rt              = go.AddComponent<RectTransform>();
                rt.anchorMin        = new Vector2(0.5f, 0.5f);
                rt.anchorMax        = new Vector2(0.5f, 0.5f);
                rt.pivot            = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2((i - 1) * spacing, 0f);
                rt.sizeDelta        = new Vector2(72f, 72f);

                var img   = go.AddComponent<Image>();
                img.color = _emptyColor;
                _starImages[i] = img;
            }
        }
    }
}
