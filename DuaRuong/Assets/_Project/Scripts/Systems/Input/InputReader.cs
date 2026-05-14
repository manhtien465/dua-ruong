using System;
using UnityEngine;

namespace DuaRuong.Systems.Input
{
    /// <summary>
    /// Touch / mouse input wrapped behind events. Player code subscribes; never reads
    /// <see cref="UnityEngine.Input"/> directly. Swap implementation later (e.g. Unity Input System)
    /// without touching gameplay code.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InputReader : MonoBehaviour
    {
        public event Action SwipeUp;
        public event Action SwipeDown;
        public event Action SwipeLeft;
        public event Action SwipeRight;
        public event Action Tap;

        [Header("Swipe Detection")]
        [SerializeField, Range(20f, 200f)] private float _minSwipePixels = 50f;
        [SerializeField, Range(0.05f, 1f)] private float _maxSwipeDuration = 0.5f;

        private Vector2 _swipeStartPos;
        private float _swipeStartTime;
        private bool _tracking;

        private void Update()
        {
            if (UnityEngine.Input.touchCount > 0)
            {
                ProcessTouch(UnityEngine.Input.GetTouch(0));
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            ProcessMouse();
#endif
        }

        private void ProcessTouch(Touch t)
        {
            switch (t.phase)
            {
                case TouchPhase.Began:
                    BeginSwipe(t.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndSwipe(t.position);
                    break;
            }
        }

        private void ProcessMouse()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                BeginSwipe(UnityEngine.Input.mousePosition);
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                EndSwipe(UnityEngine.Input.mousePosition);
            }
        }

        private void BeginSwipe(Vector2 startPos)
        {
            _swipeStartPos = startPos;
            _swipeStartTime = Time.unscaledTime;
            _tracking = true;
        }

        private void EndSwipe(Vector2 endPos)
        {
            if (!_tracking) return;
            _tracking = false;

            float duration = Time.unscaledTime - _swipeStartTime;
            if (duration > _maxSwipeDuration)
            {
                Tap?.Invoke();
                return;
            }

            Vector2 delta = endPos - _swipeStartPos;
            float absX = Mathf.Abs(delta.x);
            float absY = Mathf.Abs(delta.y);

            if (absX < _minSwipePixels && absY < _minSwipePixels)
            {
                Tap?.Invoke();
                return;
            }

            if (absX > absY)
            {
                if (delta.x > 0) SwipeRight?.Invoke();
                else SwipeLeft?.Invoke();
            }
            else
            {
                if (delta.y > 0) SwipeUp?.Invoke();
                else SwipeDown?.Invoke();
            }
        }
    }
}
