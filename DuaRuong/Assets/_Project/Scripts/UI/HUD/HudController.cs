using DuaRuong.Core;
using TMPro;
using UnityEngine;

namespace DuaRuong.UI.HUD
{
    /// <summary>
    /// In-game HUD: score, combo, distance. Pure subscriber to <see cref="GameEvents"/>;
    /// never mutates gameplay state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _comboText;
        [SerializeField] private TMP_Text _distanceText;
        [SerializeField] private CanvasGroup _comboGroup;

        [Header("Juice")]
        [SerializeField] private JuicyText _scoreJuice;
        [SerializeField] private JuicyText _comboJuice;

        [Header("Format")]
        [SerializeField] private string _scoreFormat = "{0:N0}";
        [SerializeField] private string _comboFormat = "x{0}";
        [SerializeField] private string _distanceFormat = "{0:N0}m";

        private void OnEnable()
        {
            GameEvents.ScoreChanged += HandleScore;
            GameEvents.ComboChanged += HandleCombo;
            GameEvents.DistanceChanged += HandleDistance;
        }

        private void OnDisable()
        {
            GameEvents.ScoreChanged -= HandleScore;
            GameEvents.ComboChanged -= HandleCombo;
            GameEvents.DistanceChanged -= HandleDistance;
        }

        private void HandleScore(int score)
        {
            if (_scoreText != null) 
            {
                _scoreText.SetText(_scoreFormat, score);
                if (_scoreJuice != null) _scoreJuice.PlayPop();
            }
        }

        private void HandleCombo(int combo)
        {
            if (_comboText != null)
            {
                _comboText.SetText(_comboFormat, combo);
                if (combo > 0 && _comboJuice != null) _comboJuice.PlayPop();
            }
            if (_comboGroup != null) _comboGroup.alpha = combo > 0 ? 1f : 0f;
        }

        private void HandleDistance(float distance)
        {
            if (_distanceText != null) _distanceText.SetText(_distanceFormat, distance);
        }
        private void Reset()
        {
            // Auto-find references if they follow naming conventions
            if (_scoreText == null) _scoreText = transform.Find("ScoreText")?.GetComponent<TMP_Text>();
            if (_comboText == null) _comboText = transform.Find("ComboText")?.GetComponent<TMP_Text>();
            if (_distanceText == null) _distanceText = transform.Find("DistanceText")?.GetComponent<TMP_Text>();
            
            if (_scoreJuice == null && _scoreText != null) _scoreJuice = _scoreText.GetComponent<JuicyText>();
            if (_comboJuice == null && _comboText != null) _comboJuice = _comboText.GetComponent<JuicyText>();
        }
    }
}
