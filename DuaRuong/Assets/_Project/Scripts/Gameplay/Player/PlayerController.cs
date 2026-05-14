using DuaRuong.Core;
using DuaRuong.Systems.Input;
using UnityEngine;

namespace DuaRuong.Gameplay.Player
{
    /// <summary>
    /// Wires <see cref="InputReader"/> to <see cref="PlayerMovement"/>.
    /// SwipeUp/Down → climb/drop tier. SwipeLeft/Right → lateral dodge.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputReader _input;

        private PlayerMovement _movement;
        private bool _canControl;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            if (_input == null) Debug.LogError($"[{nameof(PlayerController)}] InputReader missing.", this);
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.SwipeLeft  += HandleSwipeLeft;
                _input.SwipeRight += HandleSwipeRight;
                _input.SwipeUp    += HandleSwipeUp;
                _input.SwipeDown  += HandleSwipeDown;
            }

            GameEvents.GameStarted += HandleGameStarted;
            GameEvents.GameOver    += HandleGameOver;
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.SwipeLeft  -= HandleSwipeLeft;
                _input.SwipeRight -= HandleSwipeRight;
                _input.SwipeUp    -= HandleSwipeUp;
                _input.SwipeDown  -= HandleSwipeDown;
            }

            GameEvents.GameStarted -= HandleGameStarted;
            GameEvents.GameOver    -= HandleGameOver;
        }

        private void Update()
        {
            if (!_canControl) return;
            _movement.Tick(Time.deltaTime);
        }

        private void HandleGameStarted()
        {
            _movement.ResetState();
            _canControl = true;
        }

        private void HandleGameOver() => _canControl = false;

        private void HandleSwipeLeft()  { if (_canControl) _movement.TryDodge(-1); }
        private void HandleSwipeRight() { if (_canControl) _movement.TryDodge(+1); }
        private void HandleSwipeUp()    { if (_canControl) _movement.TryClimbTier(); }
        private void HandleSwipeDown()  { if (_canControl) _movement.TryDropTier(); }
    }
}
