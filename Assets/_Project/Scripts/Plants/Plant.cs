using LawnDefense.Augments;
using LawnDefense.Data;
using LawnDefense.Grid;
using LawnDefense.Combat;
using LawnDefense.Core;
using LawnDefense.Sun;
using UnityEngine;

namespace LawnDefense.Plants
{
    public sealed class Plant : MonoBehaviour
    {
        private GridSystem gridSystem;

        public PlantConfig Config { get; private set; }
        public GridCoordinate Coordinate { get; private set; }

        public void ConfigureRuntimeServices(
            PoolManager poolManager,
            LaneTargetService targetService,
            SunSystem sunSystem)
        {
            PlantAttackController attack = GetComponentInChildren<PlantAttackController>(true);
            if (attack != null)
            {
                attack.ConfigureRuntimeServices(poolManager, targetService);
            }

            SunProducer producer = GetComponentInChildren<SunProducer>(true);
            if (producer != null)
            {
                producer.ConfigureRuntimeServices(sunSystem);
            }

            SporeMine mine = GetComponentInChildren<SporeMine>(true);
            if (mine != null)
            {
                mine.ConfigureRuntimeServices(poolManager, targetService);
            }
        }

        public void Initialize(PlantConfig config, GridCoordinate coordinate, GridSystem ownerGrid)
        {
            Config = config;
            Coordinate = coordinate;
            gridSystem = ownerGrid;

            PlantHealth health = GetComponentInChildren<PlantHealth>(true);
            if (health != null)
            {
                health.Initialize(this, AugmentSystem.Modifiers.GetPlantMaxHealth(config));
            }

            PlantAttackController attack = GetComponentInChildren<PlantAttackController>(true);
            if (attack != null)
            {
                attack.Initialize(this);
            }

            SunProducer producer = GetComponentInChildren<SunProducer>(true);
            if (producer != null)
            {
                producer.Initialize(this);
            }

            SporeMine mine = GetComponentInChildren<SporeMine>(true);
            if (mine != null)
            {
                mine.Initialize(this);
            }
        }

        public void Die()
        {
            if (gridSystem != null)
            {
                gridSystem.ClearOccupant(Coordinate, this);
            }

            gameObject.SetActive(false);
        }
    }
}
