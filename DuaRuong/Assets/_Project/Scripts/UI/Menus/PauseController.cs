using DuaRuong.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DuaRuong.UI.Menus
{
    /// <summary>
    /// Manages the pause overlay. Lives on a persistent parent GO (PauseSystem);
    /// only _root panel is shown/hidden. Mirrors the GameOverController pattern.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PauseController : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _menuButton;

        private void Awake()
        {
            if (_resumeButton != null) _resumeButton.onClick.AddListener(OnResume);
            if (_menuButton  != null) _menuButton.onClick.AddListener(OnMenu);
            Hide();
        }

        private void OnDestroy()
        {
            if (_resumeButton != null) _resumeButton.onClick.RemoveListener(OnResume);
            if (_menuButton  != null) _menuButton.onClick.RemoveListener(OnMenu);
        }

        private void OnEnable()  => GameEvents.StateChanged += HandleStateChanged;
        private void OnDisable() => GameEvents.StateChanged -= HandleStateChanged;

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.Paused) Show();
            else Hide();
        }

        private void Show() { if (_root != null) _root.SetActive(true); }
        private void Hide() { if (_root != null) _root.SetActive(false); }

        private void OnResume()
        {
            if (GameManager.Instance != null) GameManager.Instance.Resume();
        }

        private void OnMenu()
        {
            if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
        }
    }
}
