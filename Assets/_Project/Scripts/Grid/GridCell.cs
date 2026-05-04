using UnityEngine;

namespace LawnDefense.Grid
{
    public sealed class GridCell
    {
        public GridCoordinate Coordinate { get; }
        public MonoBehaviour Occupant { get; private set; }
        public bool IsOccupied => Occupant != null;

        public GridCell(GridCoordinate coordinate)
        {
            Coordinate = coordinate;
        }

        public bool TryOccupy(MonoBehaviour occupant)
        {
            if (IsOccupied || occupant == null)
            {
                return false;
            }

            Occupant = occupant;
            return true;
        }

        public void Clear(MonoBehaviour occupant)
        {
            if (Occupant == occupant)
            {
                Occupant = null;
            }
        }
    }
}
