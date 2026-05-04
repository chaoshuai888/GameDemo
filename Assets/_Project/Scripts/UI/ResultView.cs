using LawnDefense.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LawnDefense.UI
{
    public sealed class ResultView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text resultText;

        private void Awake()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void OnEnable()
        {
            GameEvents.GameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.GameStateChanged -= HandleGameStateChanged;
        }

        public void Restart()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state != GameState.Victory && state != GameState.Defeat)
            {
                return;
            }

            if (root != null)
            {
                root.SetActive(true);
            }

            if (resultText != null)
            {
                resultText.text = state == GameState.Victory ? "Victory" : "Defeat";
            }
        }
    }
}
