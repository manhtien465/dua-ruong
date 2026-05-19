using System.Collections.Generic;
using DuaRuong.Core;
using UnityEngine;

namespace DuaRuong.UI.HUD
{
    /// <summary>
    /// Manages a pool of <see cref="FloatingScoreLabel"/> and fires them on item events.
    /// Attach to any persistent HUD GameObject that lives inside a Screen-Space Canvas.
    ///
    /// Setup in Inspector:
    ///   • _labelPrefab  — FloatingScoreLabel prefab (inside Canvas)
    ///   • _spawnAnchor  — RectTransform near the score counter or screen center
    ///   • _spawnRadius  — random jitter radius so labels don't stack exactly
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FloatingScoreLauncher : MonoBehaviour
    {
        [SerializeField] private FloatingScoreLabel _labelPrefab;
        [SerializeField] private RectTransform _spawnAnchor;
        [SerializeField] private float _spawnRadius = 40f;
        [SerializeField] private int _prewarm = 8;

        [Header("Colors")]
        [SerializeField] private Color _riceColor       = new Color(1.00f, 0.90f, 0.20f); // golden yellow
        [SerializeField] private Color _goldBuffaloColor = new Color(1.00f, 0.65f, 0.00f); // deep orange-gold

        private readonly Stack<FloatingScoreLabel> _pool = new();
        private Transform _poolParent;

        private void Awake()
        {
            _poolParent = new GameObject("FloatingLabelPool").transform;
            _poolParent.SetParent(transform, false);

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
        private void OnGoldBuffalo(int score) => Launch($"+{score} 🐃", _goldBuffaloColor);

        private void Launch(string label, Color color)
        {
            if (_labelPrefab == null || _spawnAnchor == null) return;

            var instance = _pool.Count > 0 ? _pool.Pop() : CreateLabel();

            Vector2 jitter = Random.insideUnitCircle * _spawnRadius;
            instance.transform.SetParent(_spawnAnchor.parent, false);
            instance.Spawn(label, color, _spawnAnchor.anchoredPosition + jitter);
        }

        private void ReturnToPool(FloatingScoreLabel label)
        {
            label.gameObject.SetActive(false);
            label.transform.SetParent(_poolParent, false);
            _pool.Push(label);
        }

        private FloatingScoreLabel CreateLabel()
        {
            var obj = Instantiate(_labelPrefab, _poolParent);
            obj.gameObject.SetActive(false);
            obj.Released += ReturnToPool;
            return obj;
        }
    }
}
