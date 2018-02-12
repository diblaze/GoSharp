using System;
using System.Collections.Generic;
using GoSharp.Enums;
using GoSharp.Logic;

namespace GoSharp.Data
{
    /**
     * This class is used to saved and recreate the gameBoard state
     */
    [Serializable]
    public class BoardData
    {
        public BoardData(State[,] gridStones, int[,] gridGroups, Dictionary<int, Group> groups, Coord[,] borCor,
                         int capWhite, int capBlack, int size, int nextId, State activePlayer, DateTime date)
        {
            GridStones = gridStones;
            GridGroups = gridGroups;
            Groups = groups;
            CapWhite = capWhite;
            CapBlack = capBlack;
            Size = size;
            NextId = nextId;
            ActivePlayer = activePlayer;
            BorCor = borCor;
            Date = date;
        }

        public State[,] GridStones { get; }
        public int[,] GridGroups { get; }
        public Dictionary<int, Group> Groups { get; }
        public int CapWhite { get; }
        public int CapBlack { get; }
        public int Size { get; }
        public int NextId { get; }
        public State ActivePlayer { get; }
        public Coord[,] BorCor { get; }
        public DateTime Date { get; }

        public override string ToString()
        {
            return $"Last saved on {Date:yyyy-MM-dd HH.mm.ss} ";
        }
    }
}