using DuaRuong.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DuaRuong.UI.HUD
{
    /// <summary>
    /// Wires the pause button to GameManager.Pause() at runtime.
    /// The button is only interactive when the game is Playing.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class PauseButtonHandler : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnPressed);
        }

        private void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(OnPressed);
        }

        private void OnEnable()  => GameEvents.StateChanged += HandleStateChanged;
        private void OnDisable() => GameEvents.StateChanged -= HandleStateChanged;

        private void HandleStateChanged(GameState state)
        {
            if (_button != null)
                _button.interactable = state == GameState.Playing;
        }

        private void OnPressed()
        {
            if (GameManager.Instance != null) GameManager.Instance.Pause();
        }
    }
}
