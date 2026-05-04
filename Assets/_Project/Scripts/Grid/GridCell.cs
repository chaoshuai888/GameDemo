using LawnDefense.Plants;

namespace LawnDefense.Grid
{
    public sealed class GridCell
    {
        public GridCoordinate Coordinate { get; }
        public Plant Occupant { get; private set; }
        public bool IsOccupied => Occupant != null;

        public GridCell(GridCoordinate coordinate)
        {
            Coordinate = coordinate;
        }

        public bool TryOccupy(Plant plant)
        {
            if (IsOccupied || plant == null)
            {
                return false;
            }

            Occupant = plant;
            return true;
        }

        public void Clear(Plant plant)
        {
            if (Occupant == plant)
            {
                Occupant = null;
            }
        }
    }
}
