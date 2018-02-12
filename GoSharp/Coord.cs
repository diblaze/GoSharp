using System;

namespace GoSharp
{
    [Serializable]
    public class Coord
    {
        public Coord(int column, int row)
        {
            Row = row;
            Column = column;
        }

        public int Column { get; }
        public int Row { get; }
    }
}