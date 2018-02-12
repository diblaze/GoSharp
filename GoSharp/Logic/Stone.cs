using System;
using GoSharp.Enums;

namespace GoSharp.Logic
{
    [Serializable]
    internal class Stone
    {
        //TODO: Use Stone in a future refactor?

        /// <summary>
        ///     Creates a new stone set at a specifed column and row.
        ///     The stone is also given a owner. And how many spots are free around the stone.
        /// </summary>
        /// <param name="col">Column</param>
        /// <param name="row">Row</param>
        /// <param name="owner">Player that owns this stone</param>
        public Stone(int row, int col, State owner)
        {
            Liberties = 4; // All stones will have an inital liberties of 4.
            Column = col;
            Row = row;
            Owner = owner;
            StoneGroup = null; //No group is associated to the stone from the beginning.
        }

        /// <summary>
        ///     What group is this stone associated with.
        /// </summary>
        public Group StoneGroup { get; private set; }

        /// <summary>
        ///     How many liberties does this stone have?
        /// </summary>
        public int Liberties { get; set; } // How many spots around current stone is free?

        /// <summary>
        ///     What column is this stone at?
        /// </summary>
        public int Column { get; } // What column

        /// <summary>
        ///     What row is this stone at?
        /// </summary>
        public int Row { get; } // What row

        /// <summary>
        ///     Who is the owner of this stone?
        /// </summary>
        public State Owner { get; } // Who is the owner of this stone?

        /// <summary>
        ///     Leaves and joins a new group.
        /// </summary>
        /// <param name="groupToJoin"></param>
        public void JoinGroup(Group groupToJoin)
        {
            StoneGroup = groupToJoin;
        }

        /// <summary>
        ///     Leaves the group and removes itself from the stone list.
        /// </summary>
        public void LeaveGroup()
        {
            //StoneGroup.RemoveStoneFromGroup(this);
            StoneGroup = null;
        }
    }
}