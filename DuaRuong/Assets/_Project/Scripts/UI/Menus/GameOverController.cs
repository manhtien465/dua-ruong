using DuaRuong.Core;
using DuaRuong.Systems.Save;
using DuaRuong.Systems.Score;
using DuaRuong.UI.HUD;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DuaRuong.UI.Menus
{
    [DisallowMultipleComponent]
    public sealed class GameOverController : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private ScoreSystem _scoreSystem;

        [Header("Juice")]
        [SerializeField] private RollingNumber _rollingScore;
        [SerializeField] private TMP_Text _bestText;
        [SerializeField] private GameObject _newRecordBadge;
        [SerializeField] private StarRatingDisplay _starRating;

        [Header("Star Thresholds")]
        [SerializeField, Min(1)] private int _twoStarScore   = 1000;
        [SerializeField, Min(1)] private int _threeStarScore = 3000;

        [Header("Buttons")]
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _menuButton;

        private int _hitCount;

        private void Awake()
        {
            if (_playAgainButton != null) _playAgainButton.onClick.AddListener(OnPlayAgain);
            if (_menuButton != null) _menuButton.onClick.AddListener(OnMenu);
            Hide();
        }

        private void OnEnable()
        {
            GameEvents.GameOver    += HandleGameOver;
            GameEvents.GameStarted += HandleGameStarted;
            GameEvents.PlayerHit   += HandlePlayerHit;
        }

        private void OnDisable()
        {
            GameEvents.GameOver    -= HandleGameOver;
            GameEvents.GameStarted -= HandleGameStarted;
            GameEvents.PlayerHit   -= HandlePlayerHit;
        }

        private void OnDestroy()
        {
            if (_playAgainButton != null) _playAgainButton.onClick.RemoveListener(OnPlayAgain);
            if (_menuButton != null) _menuButton.onClick.RemoveListener(OnMenu);
        }

        private void HandleGameStarted() => _hitCount = 0;
        private void HandlePlayerHit()   => _hitCount++;

        private void HandleGameOver()
        {
            if (_scoreSystem == null) return;

            int score     = _scoreSystem.Score;
            bool isRecord = SaveSystem.TrySubmitHighScore(score);

            if (_rollingScore != null) _rollingScore.SetNumber(score);
            if (_bestText != null) _bestText.SetText("{0:N0}", SaveSystem.HighScore);
            if (_newRecordBadge != null) _newRecordBadge.SetActive(isRecord);
            if (_starRating != null) _starRating.Show(CalcStars(score));

            Show();
        }

        private int CalcStars(int score)
        {
            if (score >= _threeStarScore && _hitCount == 0) return 3;
            if (score >= _twoStarScore) return 2;
            return 1;
        }

        private void Show() { if (_root != null) _root.SetActive(true); }
        private void Hide() { if (_root != null) _root.SetActive(false); }

        private void OnPlayAgain()
        {
            Hide();
            if (GameManager.Instance != null) GameManager.Instance.StartGame();
        }

        private void OnMenu()
        {
            Hide();
            if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
        }
    }
}
