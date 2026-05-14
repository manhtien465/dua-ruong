using UnityEngine;
using UnityEngine.SceneManagement;

namespace DuaRuong.Core
{
    /// <summary>
    /// Single entry point. Place on a GameObject in the Bootstrap scene.
    /// Initializes persistent systems (GameManager, AudioManager, SaveSystem)
    /// before any gameplay scene loads.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class Bootstrap : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManagerPrefab;

        [Header("Application")]
        [SerializeField, Range(30, 120)] private int _targetFrameRate = 60;
        [SerializeField] private bool _disableSleep = true;

        private void Awake()
        {
            ConfigureApplication();
            EnsureGameManager();
        }

        private void Start()
        {
            SceneManager.LoadScene("Gameplay");
        }

        private void ConfigureApplication()
        {
            Application.targetFrameRate = _targetFrameRate;
            QualitySettings.vSyncCount = 0;
            if (_disableSleep) Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void EnsureGameManager()
        {
            if (GameManager.Instance != null) return;
            if (_gameManagerPrefab == null)
            {
                Debug.LogError($"[{nameof(Bootstrap)}] GameManager prefab not assigned.");
                return;
            }
            Instantiate(_gameManagerPrefab);
        }
    }
}
