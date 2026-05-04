using LawnDefense.Data;
using LawnDefense.Plants;
using UnityEngine;

namespace LawnDefense.Grid
{
    public sealed class GridSystem : MonoBehaviour
    {
        [SerializeField] private Vector2 origin = new Vector2(-4f, -2f);
        [SerializeField] private Vector2 cellSize = new Vector2(1f, 1f);

        private GridCell[,] cells;

        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public Vector2 CellSize => cellSize;

        public void Initialize(LevelConfig levelConfig)
        {
            Rows = Mathf.Max(1, levelConfig.Rows);
            Columns = Mathf.Max(1, levelConfig.Columns);
            cells = new GridCell[Rows, Columns];

            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    cells[row, column] = new GridCell(new GridCoordinate(row, column));
                }
            }
        }

        public bool IsValid(GridCoordinate coordinate) =>
            coordinate.Row >= 0 && coordinate.Row < Rows &&
            coordinate.Column >= 0 && coordinate.Column < Columns;

        public bool TryGetCell(GridCoordinate coordinate, out GridCell cell)
        {
            if (!IsValid(coordinate) || cells == null)
            {
                cell = null;
                return false;
            }

            cell = cells[coordinate.Row, coordinate.Column];
            return true;
        }

        public bool TryOccupy(GridCoordinate coordinate, Plant plant)
        {
            return TryGetCell(coordinate, out GridCell cell) && cell.TryOccupy(plant);
        }

        public void ClearOccupant(GridCoordinate coordinate, Plant plant)
        {
            if (TryGetCell(coordinate, out GridCell cell))
            {
                cell.Clear(plant);
            }
        }

        public Vector3 GridToWorld(GridCoordinate coordinate)
        {
            return new Vector3(
                origin.x + coordinate.Column * cellSize.x,
                origin.y + coordinate.Row * cellSize.y,
                0f);
        }

        public bool TryWorldToGrid(Vector3 worldPosition, out GridCoordinate coordinate)
        {
            int column = Mathf.RoundToInt((worldPosition.x - origin.x) / cellSize.x);
            int row = Mathf.RoundToInt((worldPosition.y - origin.y) / cellSize.y);
            coordinate = new GridCoordinate(row, column);
            return IsValid(coordinate);
        }
    }
}
