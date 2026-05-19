using System.Collections.Generic;
using DuaRuong.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DuaRuong.UI.HUD
{
    /// <summary>
    /// Manages a pool of floating "+X" score labels and fires them on item-collect events.
    /// Zero Inspector setup: labels are created from code at runtime.
    /// Place this component anywhere inside a Screen-Space Canvas.
    /// Labels spawn at this GameObject's anchoredPosition (± random jitter).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FloatingScoreLauncher : MonoBehaviour
    {
        [Header("Pool")]
        [SerializeField] private int   _prewarm     = 8;
        [SerializeField] private float _spawnRadius = 50f;

        [Header("Label Style")]
        [SerializeField] private float _fontSize     = 52f;
        [SerializeField] private Color _riceColor       = new Color(1.00f, 0.90f, 0.20f);
        [SerializeField] private Color _goldBuffaloColor = new Color(1.00f, 0.65f, 0.00f);

        private readonly Stack<FloatingScoreLabel> _pool = new();
        private Transform _poolParent;
        private RectTransform _selfRt;

        private void Awake()
        {
            _selfRt = GetComponent<RectTransform>();
            if (_selfRt == null) _selfRt = gameObject.AddComponent<RectTransform>();

            var poolGo = new GameObject("_LabelPool");
            poolGo.transform.SetParent(transform, false);
            _poolParent = poolGo.transform;

            for (int i = 0; i < _prewarm; i++)
                ReturnToPool(CreateLabel());
        }

        private void OnEnable()
        {
            GameEvents.RiceCollected        += OnRice;
            GameEvents.GoldBuffaloCollected += OnGoldBuffalo;
        }

        private void OnDisable()
        {
            GameEvents.RiceCollected        -= OnRice;
            GameEvents.GoldBuffaloCollected -= OnGoldBuffalo;
        }

        private void OnRice(int score)        => Launch($"+{score}", _riceColor);
        private void OnGoldBuffalo(int score) => Launch($"+{score} ᬀ", _goldBuffaloColor);

        private void Launch(string label, Color color)
        {
            var instance = _pool.Count > 0 ? _pool.Pop() : CreateLabel();
            Vector2 jitter = Random.insideUnitCircle * _spawnRadius;
            instance.transform.SetParent(transform.parent, false);
            instance.Spawn(label, color, _selfRt.anchoredPosition + jitter);
        }

        private void ReturnToPool(FloatingScoreLabel lbl)
        {
            lbl.gameObject.SetActive(false);
            lbl.transform.SetParent(_poolParent, false);
            _pool.Push(lbl);
        }

        private FloatingScoreLabel CreateLabel()
        {
            var go = new GameObject("FloatingLabel");
            go.transform.SetParent(_poolParent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(240f, 70f);

            go.AddComponent<CanvasGroup>();

            // Text child — FloatingScoreLabel.Awake finds this via GetComponentInChildren
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRt       = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize  = _fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;

            go.SetActive(false);
            var lbl = go.AddComponent<FloatingScoreLabel>();
            lbl.Released += ReturnToPool;
            return lbl;
        }
    }
}
