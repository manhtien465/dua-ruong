using DuaRuong.Core;
using UnityEngine;

namespace DuaRuong.Systems.Score
{
    /// <summary>
    /// Tracks the current combo streak. Increments on positive events
    /// (item pickup, perfect dodge), resets on player hit.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ComboSystem : MonoBehaviour
    {
        public int Combo { get; private set; }

        private void OnEnable()
        {
            GameEvents.RiceCollected += HandleRice;
            GameEvents.GoldBuffaloCollected += HandleGold;
            GameEvents.PerfectDodge += HandleDodge;
            GameEvents.PlayerHit += HandleHit;
            GameEvents.GameStarted += HandleGameStarted;
        }

        private void OnDisable()
        {
            GameEvents.RiceCollected -= HandleRice;
            GameEvents.GoldBuffaloCollected -= HandleGold;
            GameEvents.PerfectDodge -= HandleDodge;
            GameEvents.PlayerHit -= HandleHit;
            GameEvents.GameStarted -= HandleGameStarted;
        }

        private void HandleRice(int _) => Increment();
        private void HandleGold(int _) => Increment();
        private void HandleDodge() => Increment();

        private void HandleHit() => Reset();
        private void HandleGameStarted() => Reset();

        private void Increment()
        {
            Combo++;
            GameEvents.RaiseComboChanged(Combo);
        }

        private void Reset()
        {
            if (Combo == 0) return;
            Combo = 0;
            GameEvents.RaiseComboChanged(0);
        }
    }
}
