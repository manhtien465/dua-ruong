// DEV ONLY — chỉ dùng để test Gameplay scene trực tiếp từ Editor.
// Remove trước khi ship hoặc khi Bootstrap scene đã hoạt động.
using DuaRuong.Core;
using UnityEngine;

public sealed class DevBootstrap : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_EDITOR
        if (GameManager.Instance == null)
            new GameObject("GameManager").AddComponent<GameManager>();
#endif
    }

    private void Start()
    {
#if UNITY_EDITOR
        GameManager.Instance.StartGame();
#endif
    }
}
