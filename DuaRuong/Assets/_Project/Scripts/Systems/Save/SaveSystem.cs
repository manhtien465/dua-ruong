using UnityEngine;

namespace DuaRuong.Systems.Save
{
    /// <summary>
    /// Local persistence for high score and settings.
    /// MVP uses <see cref="PlayerPrefs"/>; migrate to JSON file in <see cref="Application.persistentDataPath"/>
    /// when save schema gets non-trivial.
    /// </summary>
    public static class SaveSystem
    {
        private const string KEY_HIGH_SCORE = "dr.highscore";
        private const string KEY_TOTAL_RICE = "dr.totalrice";
        private const string KEY_AUDIO_MUTED = "dr.audiomuted";

        public static int HighScore
        {
            get => PlayerPrefs.GetInt(KEY_HIGH_SCORE, 0);
            private set
            {
                PlayerPrefs.SetInt(KEY_HIGH_SCORE, value);
                PlayerPrefs.Save();
            }
        }

        public static int TotalRice
        {
            get => PlayerPrefs.GetInt(KEY_TOTAL_RICE, 0);
            set
            {
                PlayerPrefs.SetInt(KEY_TOTAL_RICE, value);
                PlayerPrefs.Save();
            }
        }

        public static bool AudioMuted
        {
            get => PlayerPrefs.GetInt(KEY_AUDIO_MUTED, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(KEY_AUDIO_MUTED, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Updates <see cref="HighScore"/> if <paramref name="score"/> is higher.
        /// Returns true when a new record was set.
        /// </summary>
        public static bool TrySubmitHighScore(int score)
        {
            if (score <= HighScore) return false;
            HighScore = score;
            return true;
        }
    }
}
