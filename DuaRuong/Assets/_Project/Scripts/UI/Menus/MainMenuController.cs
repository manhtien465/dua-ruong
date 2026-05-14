using DuaRuong.Core;
using DuaRuong.Systems.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DuaRuong.UI.Menus
{
    /// <summary>
    /// Lives on a persistent parent GO (MainMenuSystem), never deactivated.
    /// Manages _root panel visibility via GameEvents.StateChanged.
    /// Pattern mirrors GameOverController: controller stays active, only _root toggles.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _muteButton;
        [SerializeField] private TMP_Text _highScoreText;

        private void Awake()
        {
            if (_playButton != null) _playButton.onClick.AddListener(OnPlayPressed);
            if (_shopButton != null) _shopButton.onClick.AddListener(OnShopPressed);
            if (_muteButton != null) _muteButton.onClick.AddListener(OnMutePressed);
        }

        private void OnDestroy()
        {
            if (_playButton != null) _playButton.onClick.RemoveListener(OnPlayPressed);
            if (_shopButton != null) _shopButton.onClick.RemoveListener(OnShopPressed);
            if (_muteButton != null) _muteButton.onClick.RemoveListener(OnMutePressed);
        }

        private void OnEnable()
        {
            GameEvents.StateChanged += HandleStateChanged;
            // Show on first enable (boot / main menu state).
            var state = GameManager.Instance != null ? GameManager.Instance.State : GameState.Boot;
            if (state == GameState.Boot || state == GameState.MainMenu)
                Show();
            else
                Hide();
        }

        private void OnDisable()
        {
            GameEvents.StateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.MainMenu) Show();
            else if (state == GameState.Playing) Hide();
        }

        private void Show()
        {
            if (_root != null) _root.SetActive(true);
            Refresh();
        }

        private void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        private void Refresh()
        {
            if (_highScoreText != null) _highScoreText.SetText("{0:N0}", SaveSystem.HighScore);
        }

        private void OnPlayPressed()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.StartGame();
        }

        private void OnShopPressed()
        {
            Debug.Log("Shop pressed - TODO: Implement Shop System");
            // Here you would typically call ShopSystem.Instance.Show();
        }

        private void OnMutePressed()
        {
            SaveSystem.AudioMuted = !SaveSystem.AudioMuted;
            AudioListener.volume = SaveSystem.AudioMuted ? 0f : 1f;
        }
    }
}
