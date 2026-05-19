using DuaRuong.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DuaRuong.UI.HUD
{
    /// <summary>
    /// Progress bar showing distance to the next combo tier.
    /// Zero Inspector setup: creates its own fill Image child at runtime if not assigned.
    /// Place inside a Canvas. Recommend sizing to ~600×12 (thin bar at screen bottom).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ComboBar : MonoBehaviour
    {
        [SerializeField] private Image       _fillImage;   // auto-created if null
        [SerializeField] private CanvasGroup _group;       // auto-added if null

        [Header("Tiers — must match ScoringConfig")]
        [SerializeField] private int[]   _thresholds = { 0, 10, 25, 50 };
        [SerializeField] private Color[] _tierColors =
        {
            new Color(0.90f, 0.90f, 0.90f), // tier 0 — white
            new Color(1.00f, 0.90f, 0.20f), // tier 1 — yellow
            new Color(1.00f, 0.55f, 0.10f), // tier 2 — orange
            new Color(1.00f, 0.25f, 0.15f), // tier 3 — red-orange
        };

        [Header("Animation")]
        [SerializeField] private float _showHideSpeed = 6f;
        [SerializeField] private float _pulseDuration = 0.25f;
        [SerializeField] private float _pulseScale    = 1.15f;

        private int   _currentCombo;
        private int   _currentTier = -1;
        private float _targetAlpha;
        private float _pulseTimer;
        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;

            if (_group == null) _group = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            if (_group != null) _group.alpha = 0f;

            if (_fillImage == null) EnsureBar();
        }

        private void OnEnable()
        {
            GameEvents.ComboChanged          += HandleCombo;
            GameEvents.ComboMilestoneReached += HandleMilestone;
            GameEvents.GameStarted           += HandleGameStarted;
        }

        private void OnDisable()
        {
            GameEvents.ComboChanged          -= HandleCombo;
            GameEvents.ComboMilestoneReached -= HandleMilestone;
            GameEvents.GameStarted           -= HandleGameStarted;
        }

        private void HandleGameStarted()
        {
            _currentCombo = 0;
            _currentTier  = -1;
            _targetAlpha  = 0f;
            if (_fillImage != null) _fillImage.fillAmount = 0f;
        }

        private void HandleCombo(int combo)
        {
            _currentCombo = combo;
            _targetAlpha  = combo > 0 ? 1f : 0f;
            Refresh();
        }

        private void HandleMilestone(float _) => _pulseTimer = _pulseDuration;

        private void Refresh()
        {
            if (_fillImage == null || _thresholds.Length < 2) return;

            int  tier  = GetTier(_currentCombo);
            bool atMax = tier >= _thresholds.Length - 1;

            float fill = 1f;
            if (!atMax)
            {
                int low  = _thresholds[tier];
                int high = _thresholds[tier + 1];
                fill = Mathf.Clamp01((float)(_currentCombo - low) / (high - low));
            }

            _fillImage.fillAmount = fill;

            if (tier != _currentTier && tier < _tierColors.Length)
            {
                _currentTier     = tier;
                _fillImage.color = _tierColors[tier];
            }
        }

        private void Update()
        {
            if (_group != null)
                _group.alpha = Mathf.MoveTowards(_group.alpha, _targetAlpha,
                    _showHideSpeed * Time.deltaTime);

            if (_pulseTimer > 0f)
            {
                _pulseTimer -= Time.deltaTime;
                float t    = 1f - Mathf.Clamp01(_pulseTimer / _pulseDuration);
                float bell = Mathf.Sin(t * Mathf.PI);
                transform.localScale = _baseScale * (1f + (_pulseScale - 1f) * bell);
            }
            else
            {
                transform.localScale = _baseScale;
            }
        }

        private int GetTier(int combo)
        {
            int tier = 0;
            for (int i = 0; i < _thresholds.Length; i++)
                if (combo >= _thresholds[i]) tier = i;
            return tier;
        }

        // Creates background + fill Image children automatically
        private void EnsureBar()
        {
            // Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(transform, false);
            var bgRt        = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin  = Vector2.zero;
            bgRt.anchorMax  = Vector2.one;
            bgRt.sizeDelta  = Vector2.zero;
            var bgImg       = bgGo.AddComponent<Image>();
            bgImg.color     = new Color(0f, 0f, 0f, 0.35f);
            bgImg.raycastTarget = false;

            // Fill
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(transform, false);
            var fillRt        = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin  = Vector2.zero;
            fillRt.anchorMax  = Vector2.one;
            fillRt.sizeDelta  = Vector2.zero;

            _fillImage              = fillGo.AddComponent<Image>();
            _fillImage.type         = Image.Type.Filled;
            _fillImage.fillMethod   = Image.FillMethod.Horizontal;
            _fillImage.fillOrigin   = (int)Image.OriginHorizontal.Left;
            _fillImage.fillAmount   = 0f;
            _fillImage.color        = _tierColors.Length > 0 ? _tierColors[0] : Color.white;
            _fillImage.raycastTarget = false;
        }
    }
}
