using UnityEngine;

namespace DuaRuong.Core
{
    /// <summary>
    /// Owns the top-level game state machine.
    /// Persistent across scenes via <see cref="DontDestroyOnLoad"/>, instantiated once by Bootstrap.
    /// State transitions go through <see cref="SetState"/> only — never mutate <see cref="State"/> directly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()  => GameEvents.PlayerHit += EndGame;
        private void OnDisable() => GameEvents.PlayerHit -= EndGame;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void StartGame()
        {
            if (State == GameState.Playing) return;
            SetState(GameState.Playing);
            GameEvents.RaiseGameStarted();
        }

        public void EndGame()
        {
            if (State == GameState.GameOver) return;
            SetState(GameState.GameOver);
            GameEvents.RaiseGameOver();
        }

        public void Pause()
        {
            if (State != GameState.Playing) return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
            GameEvents.RaiseGamePaused();
        }

        public void Resume()
        {
            if (State != GameState.Paused) return;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
            GameEvents.RaiseGameResumed();
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.MainMenu);
        }

        private void SetState(GameState next)
        {
            if (State == next) return;
            State = next;
            GameEvents.RaiseStateChanged(next);
        }
    }
}
