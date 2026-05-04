using System;

namespace LawnDefense.Grid
{
    [Serializable]
    public struct GridCoordinate
    {
        public int Row;
        public int Column;

        public GridCoordinate(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}
