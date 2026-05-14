using DuaRuong.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DuaRuong.UI
{
    /// <summary>
    /// Full-screen black overlay that fades from opaque to transparent when
    /// the game starts, giving a smooth "entering the field" transition.
    /// Also fades IN briefly when GoToMainMenu is called so the state swap
    /// isn't a jarring instant cut.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public sealed class ScreenFade : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float _fadeInDuration  = 0.40f;  // black→clear (game start)
        [SerializeField, Min(0.05f)] private float _fadeOutDuration = 0.25f;  // clear→black (go to menu)

        private Image  _image;
        private float  _alpha;
        private float  _targetAlpha;
        private float  _speed;

        private void Awake()
        {
            _image               = GetComponent<Image>();
            _image.color         = Color.black;
            _image.raycastTarget = false;
            _alpha               = 1f;
            _targetAlpha         = 1f;
        }

        private void OnEnable()
        {
            GameEvents.GameStarted += HandleGameStarted;
            GameEvents.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.GameStarted  -= HandleGameStarted;
            GameEvents.StateChanged -= HandleStateChanged;
        }

        private void HandleGameStarted()
        {
            _alpha       = 1f;
            _targetAlpha = 0f;
            _speed       = 1f / _fadeInDuration;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.MainMenu)
            {
                _alpha       = 0f;
                _targetAlpha = 0f;
            }
        }

        private void Update()
        {
            if (Mathf.Approximately(_alpha, _targetAlpha)) return;

            _alpha = Mathf.MoveTowards(_alpha, _targetAlpha, _speed * Time.unscaledDeltaTime);
            _image.color = new Color(0f, 0f, 0f, _alpha);
        }
    }
}
