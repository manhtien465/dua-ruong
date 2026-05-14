using System;

namespace DuaRuong.Core
{
    /// <summary>
    /// Static event bus. Decouples gameplay systems from UI / audio / analytics.
    /// All events are <see cref="Action"/>-based (alloc-free at invocation).
    /// Subscribers MUST unsubscribe in OnDisable to avoid leaks across scene reloads.
    /// </summary>
    public static class GameEvents
    {
        public static event Action<GameState> StateChanged;
        public static event Action GameStarted;
        public static event Action GameOver;
        public static event Action GamePaused;
        public static event Action GameResumed;

        public static event Action<int> ScoreChanged;
        public static event Action<int> ComboChanged;
        public static event Action<float> DistanceChanged;

        public static event Action<int> RiceCollected;
        public static event Action<int> GoldBuffaloCollected;
        public static event Action PlayerHit;
        public static event Action PerfectDodge;
        // Fired when combo multiplier tier increases (x1.0→x1.5, x1.5→x2, x2→x3).
        public static event Action<float> ComboMilestoneReached;
        // Fired when player changes terrace tier. direction: +1 = up, -1 = down.
        public static event Action<int> TierChanged;

        public static void RaiseStateChanged(GameState s) => StateChanged?.Invoke(s);
        public static void RaiseGameStarted() => GameStarted?.Invoke();
        public static void RaiseGameOver() => GameOver?.Invoke();
        public static void RaiseGamePaused() => GamePaused?.Invoke();
        public static void RaiseGameResumed() => GameResumed?.Invoke();

        public static void RaiseScoreChanged(int v) => ScoreChanged?.Invoke(v);
        public static void RaiseComboChanged(int v) => ComboChanged?.Invoke(v);
        public static void RaiseDistanceChanged(float v) => DistanceChanged?.Invoke(v);

        public static void RaiseRiceCollected(int v) => RiceCollected?.Invoke(v);
        public static void RaiseGoldBuffaloCollected(int v) => GoldBuffaloCollected?.Invoke(v);
        public static void RaisePlayerHit() => PlayerHit?.Invoke();
        public static void RaisePerfectDodge() => PerfectDodge?.Invoke();
        public static void RaiseComboMilestoneReached(float multiplier) => ComboMilestoneReached?.Invoke(multiplier);
        public static void RaiseTierChanged(int direction) => TierChanged?.Invoke(direction);

        /// <summary>
        /// Clear all subscriptions. Call this when leaving the gameplay scene
        /// to prevent stale subscribers from receiving events after destruction.
        /// </summary>
        public static void ClearAll()
        {
            StateChanged = null;
            GameStarted = null;
            GameOver = null;
            GamePaused = null;
            GameResumed = null;

            ScoreChanged = null;
            ComboChanged = null;
            DistanceChanged = null;

            RiceCollected = null;
            GoldBuffaloCollected = null;
            PlayerHit = null;
            PerfectDodge = null;
            ComboMilestoneReached = null;
            TierChanged = null;
        }
    }
}
