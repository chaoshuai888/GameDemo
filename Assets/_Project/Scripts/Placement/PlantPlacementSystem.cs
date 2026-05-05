using System.Collections.Generic;
using LawnDefense.Augments;
using LawnDefense.Combat;
using LawnDefense.Core;
using LawnDefense.Data;
using LawnDefense.Grid;
using LawnDefense.Plants;
using LawnDefense.Sun;
using UnityEngine;

namespace LawnDefense.Placement
{
    public sealed class PlantPlacementSystem : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private GridSystem gridSystem;
        [SerializeField] private SunSystem sunSystem;
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private LaneTargetService targetService;

        private readonly Dictionary<string, PlantCardState> cardStates = new Dictionary<string, PlantCardState>();
        private PlantConfig selectedPlant;

        public void Initialize(PlantConfig[] availablePlants)
        {
            cardStates.Clear();
            selectedPlant = null;
            GameEvents.RaisePlantSelectionChanged(null);

            if (availablePlants == null)
            {
                return;
            }

            for (int i = 0; i < availablePlants.Length; i++)
            {
                PlantConfig config = availablePlants[i];
                if (config == null || string.IsNullOrEmpty(config.Id))
                {
                    continue;
                }

                cardStates[config.Id] = new PlantCardState(config);
            }
        }

        public void SelectPlant(PlantConfig config)
        {
            if (config == null || string.IsNullOrEmpty(config.Id))
            {
                return;
            }

            PlantCardState cardState;
            if (cardStates.TryGetValue(config.Id, out cardState) && cardState.IsReady)
            {
                selectedPlant = config;
                GameEvents.RaisePlantSelectionChanged(selectedPlant);
            }
            else
            {
                GameEvents.RaisePlantSelectionChanged(selectedPlant);
            }
        }

        private void Update()
        {
            TickCardCooldowns(Time.deltaTime);

            if (selectedPlant != null && Input.GetMouseButtonDown(0))
            {
                TryPlaceSelectedPlantAtScreen(Input.mousePosition);
            }
        }

        private void TickCardCooldowns(float deltaTime)
        {
            foreach (PlantCardState cardState in cardStates.Values)
            {
                cardState.Tick(deltaTime);
            }
        }

        public bool TryPlaceSelectedPlantAtScreen(Vector3 screenPosition)
        {
            if (selectedPlant == null || selectedPlant.Prefab == null || gridSystem == null || sunSystem == null)
            {
                return false;
            }

            Camera cameraToUse = worldCamera != null ? worldCamera : Camera.main;
            if (cameraToUse == null)
            {
                return false;
            }

            screenPosition.z = -cameraToUse.transform.position.z;
            Vector3 worldPosition = cameraToUse.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;
            return TryPlaceSelectedPlantAtWorld(worldPosition);
        }

        public bool TryPlaceSelectedPlantAtWorld(Vector3 worldPosition)
        {
            if (selectedPlant == null || selectedPlant.Prefab == null || gridSystem == null || sunSystem == null)
            {
                return false;
            }

            GridCoordinate coordinate;
            if (!gridSystem.TryWorldToGrid(worldPosition, out coordinate))
            {
                return false;
            }

            GridCell cell;
            if (!gridSystem.TryGetCell(coordinate, out cell) || cell.IsOccupied)
            {
                return false;
            }

            int plantCost = AugmentSystem.Modifiers.GetPlantCost(selectedPlant);
            if (!sunSystem.Wallet.TrySpend(plantCost))
            {
                return false;
            }

            Vector3 plantPosition = gridSystem.GridToWorld(coordinate);
            GameObject instance = SpawnPlant(selectedPlant.Prefab, plantPosition);
            if (instance == null)
            {
                RefundSelectedPlant(plantCost);
                return false;
            }

            Plant plant = instance.GetComponent<Plant>();
            if (plant == null)
            {
                CleanupPlacedObject(instance);
                RefundSelectedPlant(plantCost);
                return false;
            }

            if (!gridSystem.TryOccupy(coordinate, plant))
            {
                CleanupPlacedObject(instance);
                RefundSelectedPlant(plantCost);
                return false;
            }

            instance.transform.position = plantPosition;
            plant.ConfigureRuntimeServices(poolManager, targetService, sunSystem);
            plant.Initialize(selectedPlant, coordinate, gridSystem);
            StartSelectedPlantCooldown();
            selectedPlant = null;
            GameEvents.RaisePlantSelectionChanged(null);
            return true;
        }

        private GameObject SpawnPlant(GameObject prefab, Vector3 position)
        {
            if (poolManager != null)
            {
                return poolManager.Spawn(prefab, position, Quaternion.identity);
            }

            return Instantiate(prefab, position, Quaternion.identity);
        }

        private void CleanupPlacedObject(GameObject instance)
        {
            if (poolManager != null)
            {
                poolManager.Despawn(instance);
            }
            else
            {
                Destroy(instance);
            }
        }

        private void RefundSelectedPlant(int amount)
        {
            if (sunSystem != null && selectedPlant != null)
            {
                sunSystem.Wallet.Add(amount);
            }
        }

        private void StartSelectedPlantCooldown()
        {
            if (selectedPlant == null || string.IsNullOrEmpty(selectedPlant.Id))
            {
                return;
            }

            PlantCardState cardState;
            if (cardStates.TryGetValue(selectedPlant.Id, out cardState))
            {
                cardState.StartCooldown();
            }
        }
    }
}
