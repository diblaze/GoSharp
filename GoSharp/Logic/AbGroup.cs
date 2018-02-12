using System;
using System.Collections.Generic;
using GoSharp.Enums;
using Newtonsoft.Json;

namespace GoSharp.Logic
{
    [Serializable]
    public abstract class AbGroup
    {
        [JsonProperty] protected HashSet<Coord> _adjacent;

        [JsonProperty] protected State _color;

        [JsonProperty] protected int _id;

        [JsonProperty] protected HashSet<Coord> _liberties;

        [JsonProperty] protected HashSet<Coord> _stones;

        // Adds the stone at argument to the group
        public abstract void AddStone(Coord coord);

        // Removes the stone at argument from the group
        public abstract void RemoveStone(Coord coord);

        // Consumes the argument group into this group
        public abstract void MergeGroup(AbGroup group);

        // Returns the set of coordinates which contains opponent stones adjacent to the group
        public abstract HashSet<Coord> GetAdjacent();

        // Returns the set of coordinates which are liberties adjacent to the group
        public abstract HashSet<Coord> GetLiberties();

        // Returns the set of coordinates which contain the stones of the group
        public abstract HashSet<Coord> GetStones();

        // Returns the color of the group
        public abstract State GetColor();

        // Returns the ID of the group.
        public abstract int GetId();
    }
}