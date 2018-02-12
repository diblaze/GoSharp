using System;
using System.Collections.Generic;
using GoSharp.Enums;

namespace GoSharp.Logic
{
    /**
     * This class contains the logic used for a group of stones
     * A group of stones consists of one or more stones on the board
     * Once a stone is placed on the game board a group is created
     * and merged with any adjacent groups of the same color using the
     * mergeGroup method in the Board class. A group can be removed 
     * using the capture method in the board class
     * 
     * The logic in this group contains which stones are a part of the group
     * What stones are adjacent to this group and which liberties 
     * the group has. 
     */
    [Serializable]
    public class Group : AbGroup
    {
        //private readonly HashSet<Coord> _adjacent
        //; //This set contains the coordinates of adjacent stones to any stone in the group

        //private readonly State _color;
        //private readonly int _id;
        //private readonly HashSet<Coord> _liberties; //This set containt the coordinates of the liberties of the group
        //private readonly HashSet<Coord> _stones; //This set contains the stones wich make up the group

        // The constructor is called when a stone is placed on the board
        public Group(int id, Coord mCoord, State mColor, HashSet<Coord> mLiberties, HashSet<Coord> mAdjacent)
        {
            _stones = new HashSet<Coord> {mCoord};
            _color = mColor;
            _liberties = mLiberties;
            _adjacent = mAdjacent;
            _id = id;
        }

        // Returns the liberties of the group, used to decide if a group dies
        public override HashSet<Coord> GetLiberties()
        {
            return _liberties;
        }

        public override HashSet<Coord> GetAdjacent()
        {
            return _adjacent;
        }

        public override HashSet<Coord> GetStones()
        {
            return _stones;
        }

        public override int GetId()
        {
            return _id;
        }

        // This adds a liberty to the group, it is called when a group gets captured to grant liberties where there was before a stone
        public override void RemoveStone(Coord coord)
        {
            if (_adjacent.RemoveWhere(c => c.Row == coord.Row && c.Column == coord.Column) > 0)
            {
                _liberties.Add(coord);
            }
        }

        public override void AddStone(Coord coord)
        {
            if (_liberties.RemoveWhere(c => c.Row == coord.Row && c.Column == coord.Column) > 0)
            {
                _adjacent.Add(coord);
            }
        }

        public override void MergeGroup(AbGroup slaveGroup)
        {
            _stones.UnionWith(slaveGroup.GetStones());
            _liberties.UnionWith(slaveGroup.GetLiberties());
            _adjacent.UnionWith(slaveGroup.GetAdjacent());

            foreach (Coord coord in _stones)
            {
                _liberties.RemoveWhere(c => c.Row == coord.Row && c.Column == coord.Column);
            }
        }

        public override State GetColor()
        {
            return _color;
        }
    }
}