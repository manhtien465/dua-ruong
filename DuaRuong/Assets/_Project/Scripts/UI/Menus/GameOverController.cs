using DuaRuong.Core;
using DuaRuong.Systems.Save;
using DuaRuong.Systems.Score;
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

        [Header("Buttons")]
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _menuButton;

        private void Awake()
        {
            if (_playAgainButton != null) _playAgainButton.onClick.AddListener(OnPlayAgain);
            if (_menuButton != null) _menuButton.onClick.AddListener(OnMenu);
            Hide();
        }

        private void OnEnable() => GameEvents.GameOver += HandleGameOver;
        private void OnDisable() => GameEvents.GameOver -= HandleGameOver;

        private void OnDestroy()
        {
            if (_playAgainButton != null) _playAgainButton.onClick.RemoveListener(OnPlayAgain);
            if (_menuButton != null) _menuButton.onClick.RemoveListener(OnMenu);
        }

        private void HandleGameOver()
        {
            if (_scoreSystem == null) return;

            int score = _scoreSystem.Score;
            bool isRecord = SaveSystem.TrySubmitHighScore(score);

            if (_rollingScore != null) _rollingScore.SetNumber(score);
            if (_bestText != null) _bestText.SetText("{0:N0}", SaveSystem.HighScore);
            if (_newRecordBadge != null) _newRecordBadge.SetActive(isRecord);

            Show();
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
