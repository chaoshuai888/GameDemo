using LawnDefense.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LawnDefense.UI
{
    public sealed class GameHudView : MonoBehaviour
    {
        [SerializeField] private Text sunText;
        [SerializeField] private Slider waveProgressSlider;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text resultText;

        private void OnEnable()
        {
            GameEvents.SunChanged += HandleSunChanged;
            GameEvents.WaveProgressChanged += HandleWaveProgressChanged;
            GameEvents.GameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.SunChanged -= HandleSunChanged;
            GameEvents.WaveProgressChanged -= HandleWaveProgressChanged;
            GameEvents.GameStateChanged -= HandleGameStateChanged;
        }

        private void Awake()
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            if (waveProgressSlider != null)
            {
                waveProgressSlider.minValue = 0f;
                waveProgressSlider.maxValue = 1f;
                waveProgressSlider.value = 0f;
            }
        }

        private void HandleSunChanged(int value)
        {
            if (sunText != null)
            {
                sunText.text = value.ToString();
            }
        }

        private void HandleWaveProgressChanged(float progress)
        {
            if (waveProgressSlider != null)
            {
                waveProgressSlider.value = Mathf.Clamp01(progress);
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state != GameState.Victory && state != GameState.Defeat)
            {
                return;
            }

            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            if (resultText != null)
            {
                resultText.text = state == GameState.Victory ? "Victory" : "Defeat";
            }
        }
    }
}
