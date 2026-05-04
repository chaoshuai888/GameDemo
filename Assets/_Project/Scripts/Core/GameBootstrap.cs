using LawnDefense.CameraTools;
using LawnDefense.Data;
using LawnDefense.Grid;
using LawnDefense.Placement;
using LawnDefense.Sun;
using LawnDefense.Waves;
using UnityEngine;

namespace LawnDefense.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private GridSystem gridSystem;
        [SerializeField] private SunSystem sunSystem;
        [SerializeField] private PlantPlacementSystem plantPlacementSystem;
        [SerializeField] private WaveSystem waveSystem;
        [SerializeField] private CameraFitController cameraFitController;
        [SerializeField] private GameStateController gameStateController;

        private void Start()
        {
            if (levelConfig == null)
            {
                Debug.LogError("GameBootstrap requires a LevelConfig.", this);
            }

            if (gridSystem != null)
            {
                gridSystem.Initialize(levelConfig);
            }

            if (sunSystem != null)
            {
                sunSystem.Initialize(levelConfig);
            }

            if (plantPlacementSystem != null)
            {
                plantPlacementSystem.Initialize(levelConfig != null ? levelConfig.AvailablePlants : null);
            }

            if (cameraFitController != null && gridSystem != null)
            {
                cameraFitController.Fit(gridSystem);
            }

            if (waveSystem != null)
            {
                waveSystem.Initialize(levelConfig);
            }

            if (gameStateController != null)
            {
                gameStateController.SetState(GameState.Playing);
            }
            else
            {
                GameEvents.RaiseGameStateChanged(GameState.Playing);
            }
        }
    }
}
