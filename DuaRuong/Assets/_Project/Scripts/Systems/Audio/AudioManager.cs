using DuaRuong.Core;
using DuaRuong.Systems.Save;
using UnityEngine;

namespace DuaRuong.Systems.Audio
{
    /// <summary>
    /// Plays BGM and one-shot SFX. Mounts a small ring of <see cref="AudioSource"/>s
    /// to avoid clipping when many SFX trigger in the same frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioDatabase _database;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource[] _sfxSources;

        private int _nextSfxIndex;

        private void Awake()
        {
            if (_database == null) Debug.LogError($"[{nameof(AudioManager)}] AudioDatabase missing.", this);
            ApplyMute(SaveSystem.AudioMuted);
        }

        private void OnEnable()
        {
            GameEvents.GameStarted            += HandleGameStarted;
            GameEvents.GameOver               += HandleGameOver;
            GameEvents.RiceCollected          += HandleRiceCollected;
            GameEvents.GoldBuffaloCollected   += HandleGoldBuffaloCollected;
            GameEvents.PlayerHit              += HandlePlayerHit;
            GameEvents.ComboMilestoneReached  += HandleComboMilestone;
            GameEvents.TierChanged            += HandleTierChanged;
        }

        private void OnDisable()
        {
            GameEvents.GameStarted            -= HandleGameStarted;
            GameEvents.GameOver               -= HandleGameOver;
            GameEvents.RiceCollected          -= HandleRiceCollected;
            GameEvents.GoldBuffaloCollected   -= HandleGoldBuffaloCollected;
            GameEvents.PlayerHit              -= HandlePlayerHit;
            GameEvents.ComboMilestoneReached  -= HandleComboMilestone;
            GameEvents.TierChanged            -= HandleTierChanged;
        }

        private void HandleRiceCollected(int _)       => Play(SfxId.RicePickup);
        private void HandleGoldBuffaloCollected(int _) => Play(SfxId.GoldBuffaloPickup);
        private void HandlePlayerHit()                => Play(SfxId.Hit);
        private void HandleComboMilestone(float _)    => Play(SfxId.ComboMilestone);
        private void HandleTierChanged(int _)         => Play(SfxId.TierChange);

        public void Play(SfxId id)
        {
            if (_database == null || _sfxSources == null || _sfxSources.Length == 0) return;
            if (!_database.TryGetSfx(id, out var clip, out var volume)) return;

            var source = _sfxSources[_nextSfxIndex];
            _nextSfxIndex = (_nextSfxIndex + 1) % _sfxSources.Length;
            source.PlayOneShot(clip, volume);
        }

        public void SetMuted(bool muted)
        {
            SaveSystem.AudioMuted = muted;
            ApplyMute(muted);
        }

        private void ApplyMute(bool muted)
        {
            AudioListener.volume = muted ? 0f : 1f;
        }

        private void HandleGameStarted()
        {
            if (_musicSource == null || _database == null) return;
            _musicSource.clip = _database.Bgm;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        private void HandleGameOver()
        {
            Play(SfxId.GameOver);
        }
    }
}
