using UnityEngine;
using UnityEngine.EventSystems;

namespace DuaRuong.UI
{
    /// <summary>
    /// Adds a juicy spring-scale animation to any Button.
    /// Smoothly shrinks on press, bounces back on release.
    /// Drop on the same GO as the Button component.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ButtonFeedback : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("Settings")]
        [SerializeField, Range(0.7f, 1.0f)] private float _pressedScale = 0.9f;
        [SerializeField, Range(5f, 50f)]   private float _springFrequency = 20f;
        [SerializeField, Range(0f, 1f)]    private float _damping = 0.5f;

        private Vector3 _baseScale;
        private Vector3 _targetScale;
        private Vector3 _currentVelocity;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _targetScale = _baseScale;
        }

        private void Update()
        {
            // Spring animation logic
            transform.localScale = Vector3.SmoothDamp(
                transform.localScale, 
                _targetScale, 
                ref _currentVelocity, 
                1f / _springFrequency, 
                Mathf.Infinity, 
                Time.unscaledDeltaTime
            );
        }

        public void OnPointerDown(PointerEventData _) => _targetScale = _baseScale * _pressedScale;
        public void OnPointerUp(PointerEventData _)   => _targetScale = _baseScale;
        public void OnPointerExit(PointerEventData _) => _targetScale = _baseScale;
    }
}
