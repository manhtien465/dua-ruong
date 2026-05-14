using UnityEngine;

namespace DuaRuong.Systems.Audio
{
    public enum SfxId
    {
        UiClick,
        TierChange,     // swipe up / down — player changes terrace level
        Dodge,          // swipe left / right — lateral dodge
        RicePickup,
        GoldBuffaloPickup,
        ComboMilestone,
        Hit,
        GameOver,
    }

    [System.Serializable]
    public struct SfxEntry
    {
        public SfxId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [CreateAssetMenu(fileName = "AudioDatabase", menuName = "DuaRuong/Audio/Database")]
    public sealed class AudioDatabase : ScriptableObject
    {
        [Header("Music")]
        [SerializeField] private AudioClip _bgm;

        [Header("SFX")]
        [SerializeField] private SfxEntry[] _sfxEntries;

        public AudioClip Bgm => _bgm;

        public bool TryGetSfx(SfxId id, out AudioClip clip, out float volume)
        {
            for (int i = 0; i < _sfxEntries.Length; i++)
            {
                if (_sfxEntries[i].id == id)
                {
                    clip = _sfxEntries[i].clip;
                    volume = _sfxEntries[i].volume;
                    return clip != null;
                }
            }
            clip = null;
            volume = 0f;
            return false;
        }
    }
}
