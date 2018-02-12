using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using GoSharp.Data;
using GoSharp.Enums;
using GoSharp.Exceptions;

namespace GoSharp.Logic
{
    internal class Board : GameBoard
    {
        private readonly HashSet<int> _markedGroups;
        private Coord[,] _borCor; // Contains all coordinate objects on the playing field
        private int _capBlack; // The same for black (Points for black) 
        private int _capWhite; // How many BLACK stones WHITE has captured (Points for white)
        private int[,] _gridGroups; // Contains the ID of the group the stone in that point belongs to.
        private State[,] _gridStones; // Contains the game grid, 0 = Empty, 1 = Black, 2 = White
        private Dictionary<int, Group> _groups; // This set contains all the groups of stones on the board
        private int _nextId; // The id for the next group
        private int _size; // The size of the board

        public Board(int size)
        {
            _size = size;
            _capBlack = 0;
            _capWhite = 0;
            _nextId = 1;
            _gridStones = new State[_size, _size];
            _gridGroups = new int[_size, _size];
            _groups = new Dictionary<int, Group>();
            _borCor = new Coord[size, size];
            _markedGroups = new HashSet<int>();
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    _borCor[j, i] = new Coord(j, i);
                }
            }
        }

        // Returns the Captures of the given color
        public override int GetCaptures(State color)
        {
            switch (color)
            {
                case State.Black:
                    return GetBlackPoints();
                case State.White:
                    return GetWhitePoints();
                case State.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), color, null);
            }
            return -1;
        }

        public int GetBlackPoints()
        {
            return _capBlack;
        }

        public int GetWhitePoints()
        {
            return _capWhite;
        }

        public override void SaveGame(State activePlayer, JsonSerialization jsonSerialization)
        {
            string pathToSaveIn = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // Create a new object to save
            var boardData =
                new BoardData(_gridStones, _gridGroups, _groups, _borCor, _capWhite, _capBlack, _size, _nextId,
                              activePlayer, DateTime.Now);
            // save the object
            jsonSerialization.WriteToJsonFile(pathToSaveIn, boardData);
        }

        public override State LoadGame(JsonSerialization jsonSerialization)
        {
            const string fileName = "savedata.json";
            string pathToLoadFrom = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + fileName;

            // Recreate the object from the datafile
            try
            {
                var boardData = jsonSerialization.ReadFromJsonFile<BoardData>(pathToLoadFrom);
                if (boardData.GetType().GetProperties().Any(property => property == null))
                {
                    throw new SaveDataCorruptedException("Save data in " + pathToLoadFrom + " + is corrupted.");
                }

                if (boardData.GridStones.Length != _size * _size || boardData.GridGroups.Length != _size * _size ||
                    boardData.ActivePlayer != State.White && boardData.ActivePlayer != State.Black)
                {
                    throw new SaveDataCorruptedException("Save data in " + pathToLoadFrom + " + is corrupted.");
                }

                // Copy the values into this object
                _gridStones = boardData.GridStones;
                _gridGroups = boardData.GridGroups;
                _groups = boardData.Groups;
                _capWhite = boardData.CapWhite;
                _capBlack = boardData.CapBlack;
                _size = boardData.Size;
                _nextId = boardData.NextId;
                _borCor = boardData.BorCor;

                return boardData.ActivePlayer;
            }
            catch(SaveDataCorruptedException e)
            {
                throw e;
            }
            
        }

        public override State LoadGame(BoardData boardData)
        {
            const string fileName = "savedata.json";
            string pathToLoadFrom = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + fileName;

            if (boardData.GetType().GetProperties().Any(property => property == null))
            {
                throw new SaveDataCorruptedException("Save data in " + pathToLoadFrom + " + is corrupted.");
            }

            if (boardData.GridStones.Length != _size * _size || boardData.GridGroups.Length != _size * _size ||
                boardData.ActivePlayer != State.White && boardData.ActivePlayer != State.Black)
            {
                throw new SaveDataCorruptedException("Save data in " + pathToLoadFrom + " + is corrupted.");
            }

            // Copy the values into this object
            _gridStones = boardData.GridStones;
            _gridGroups = boardData.GridGroups;
            _groups = boardData.Groups;
            _capWhite = boardData.CapWhite;
            _capBlack = boardData.CapBlack;
            _size = boardData.Size;
            _nextId = boardData.NextId;
            _borCor = boardData.BorCor;

            return boardData.ActivePlayer;
        }

        // Returns true of the stone at the location is marked
        public override bool IsMarked(Coord coordToCheck)
        {
            return _markedGroups.Contains(_gridGroups[coordToCheck.Column, coordToCheck.Row]);
        }

        /**
         * This method merges group1 with group2
         * All stones and liberties from group2 will be added to group1
         * Group 2 will be deleted.
         */
        private void MergeGroup(AbGroup group1, AbGroup group2)
        {
            group1.MergeGroup(group2);
            DeleteGroup(group2);
        }

        /**
         * This method will remove a captured group
         * Each stone in the group is removed from the board and added as a liberty for all other groups
         */
        private void Capture(AbGroup capturedGroup, State color)
        {
            foreach (Coord coord in capturedGroup.GetStones())
            {
                // Update the board and add the capture
                _gridStones[coord.Column, coord.Row] = State.None;
                _gridGroups[coord.Column, coord.Row] = 0;
                switch (color)
                {
                    case State.Black:
                        _capBlack++;
                        break;
                    case State.White:
                        _capWhite++;
                        break;
                    case State.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(color), color, null);
                }
                // Update all groups liberties/adjacent data
                foreach (Group g in _groups.Values)
                {
                    g.RemoveStone(coord);
                }
            }
            // Remove the group from the board
            DeleteGroup(capturedGroup);
        }

        // Removes the group from the Board
        private void DeleteGroup(AbGroup group)
        {
            _groups.Remove(group.GetId());
        }

        /**
         * This method is called when a stone is placed
         * Returns true if successful
         */
        public override bool PlaceStone(Coord coordToPlace, State color, State opponent)
        {
            try
            {
                Coord coord = GetCoord(coordToPlace);

                if (LegalMove(coord, color, opponent)) // IF the move is legal
                {
                    UpdateBoard(coord, color, opponent); // Place the stone
                    return true;
                }
            }
            catch (CoordinatesOutsideOfBoardException e)
            {
                throw e;
            }
            
            return false;
        }

        // Returns true if placing a stone with color color on point coord is legal
        private bool LegalMove(Coord coord, State color, State opponent)
        {
            HashSet<Coord> neighbours = GetNeighbours(coord);
            if (_gridStones[coord.Column, coord.Row] != State.None) // there is a stone placed
            {
                return false;
            }

            if (GetColoredNeighbour(neighbours, State.None).Count != 0)
            {
                return true;
            }

            foreach (Coord n in GetColoredNeighbour(neighbours, color))
            {
                AbGroup g = _groups[_gridGroups[n.Column, n.Row]];
                // If the stone connects to a stone with 2 or more liberties It will not be captured.
                if (g.GetLiberties().Count > 1)
                {
                    return true;
                }
            }
            foreach (Coord n in GetColoredNeighbour(neighbours, opponent))
            {
                AbGroup g = _groups[_gridGroups[n.Column, n.Row]];
                // If any opponent group has one liberty that group is captured and the move is Legal, as it now has a liberty.
                if (g.GetLiberties().Count == 1)
                {
                    return true;
                }
            }
            return false;
        }

        /*
         * This Dummymethod for deciding if ko-rule applies
         */
        private bool Ko(int[] coord)
        {
            throw new NotImplementedException();
        }

        /**
         * This method Updates the state of the board with the addition of the new stone
         */
        private void UpdateBoard(Coord coord, State color, State opponent)
        {
            _gridStones[coord.Column, coord.Row] = color; //Place the stone
            HashSet<Coord> neigh = GetNeighbours(coord); // Get the neighboouring coords.

            var g = new Group(GetNextGroupId(), coord, color, GetColoredNeighbour(neigh, State.None),
                              GetColoredNeighbour(neigh, opponent)); //Create the group
            _groups.Add(g.GetId(), g); // Add the group to the board
            _gridGroups[coord.Column, coord.Row] = g.GetId(); //Add group to grid

            foreach (Coord pos in neigh) //Merge adjacent groups of same color into this group.
            {
                int groupId = _gridGroups[pos.Column, pos.Row];
                if (!_groups.ContainsKey(groupId) || groupId == g.GetId()) // If there is no group of the ID do nothing
                {
                    continue;
                }
                AbGroup ng = _groups[_gridGroups[pos.Column, pos.Row]];
                if (ng?.GetColor() == color) // If the group is of the same collor merge the groups
                {
                    MergeGroupGrid(g, ng);
                    MergeGroup(g, ng);
                    g.AddStone(coord);
                }
                else if (ng?.GetColor() == opponent) // If the gorup is of the opponent
                {
                    ng.AddStone(coord);
                    if (ng.GetLiberties().Count == 0) // If we removed the last liberty capture the group
                    {
                        Capture(ng, color);
                    }
                }
            }

            // These methods prints the board and some debugg information in the output.
            WriteGroupBoard();
            WriteGroupLiberties();
        }

        // This method updates the group grid, consuming all stones in the SlaveGroup to the MAsterGroup
        private void MergeGroupGrid(AbGroup masterGroup, AbGroup slaveGroup)
        {
            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < _size; j++)
                {
                    if (_gridGroups[i, j] == slaveGroup.GetId())
                    {
                        _gridGroups[i, j] = masterGroup.GetId();
                    }
                }
            }
        }

        private int GetNextGroupId()
        {
            int temp = _nextId;
            _nextId++;
            return temp;
        }

        public override State[,] GetGridStones()
        {
            return _gridStones;
        }

        public override int GetSize()
        {
            return _size;
        }

        private void WriteGroupBoard()
        {
            Debug.WriteLine("This is the current Group Grid:");
            Debug.WriteLine("");

            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < _size; j++)
                {
                    Debug.Write(_gridGroups[j, i] + " ");
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("");
            Debug.WriteLine("");
        }

        private void WriteGroupLiberties()
        {
            Debug.WriteLine("This is the current liberties for the groups:");
            Debug.WriteLine("");

            foreach (int id in _groups.Keys)
            {
                Debug.WriteLine(id + " : " + _groups[id].GetLiberties().Count + " : " +
                                _groups[id].GetAdjacent().Count);
            }
        }

        private Coord GetCoord(Coord coord)
        {
            if (coord.Column < 0 || coord.Column >= _size || coord.Row < 0 || coord.Row >= _size)
            {
                throw new CoordinatesOutsideOfBoardException();
            }
            return _borCor[coord.Column, coord.Row];
        }

        private bool CoordInBoard(Coord coord)
        {
            try
            {
                GetCoord(coord);
                return true;
            }
            catch (CoordinatesOutsideOfBoardException e)
            {
                return false;
                throw e;
            }
            
            
        }

        //
        private HashSet<Coord> GetNeighbours(Coord c)
        {
            HashSet<Coord> neighbours = new HashSet<Coord>();

            Coord coordToCheck;

            if (CoordInBoard(coordToCheck = new Coord(c.Column - 1, c.Row)))
            {
                neighbours.Add(coordToCheck);
            }

            if (CoordInBoard(coordToCheck = new Coord(c.Column + 1, c.Row)))
            {
                neighbours.Add(coordToCheck);
            }

            if (CoordInBoard(coordToCheck = new Coord(c.Column, c.Row - 1)))
            {
                neighbours.Add(coordToCheck);
            }

            if (CoordInBoard(coordToCheck = new Coord(c.Column, c.Row + 1)))
            {
                neighbours.Add(coordToCheck);
            }
            /*
            {
                GetCoord(new Coord(c.Column - 1, c.Row)),
                GetCoord(new Coord(c.Column + 1, c.Row)),
                GetCoord(new Coord(c.Column, c.Row - 1)),
                GetCoord(new Coord(c.Column, c.Row + 1))
 
            };
            */
            return neighbours;
        }

        // This just returns the coords in n of the color color.
        private HashSet<Coord> GetColoredNeighbour(HashSet<Coord> n, State color)
        {
            HashSet<Coord> colNeigh = new HashSet<Coord>();
            foreach (Coord c in n)
            {
                if (c != null && _gridStones[c.Column, c.Row] == color)
                {
                    colNeigh.Add(c);
                }
            }
            return colNeigh;
        }

        // This toggles the marking of the group at the location
        public override bool MarkDeadGroup(Coord coord)
        {
            int id = _gridGroups[coord.Column, coord.Row];
            if (_markedGroups.Contains(id))
            {
                _markedGroups.Remove(id);
            }
            else
            {
                _markedGroups.Add(id);
            }
            return true;
        }

        // Remove all groups that are marked dead
        public override void DeleteDead()
        {
            foreach (int groupId in _markedGroups)
            {
                AbGroup groupToDelete = _groups[groupId];
                State opponentColor = groupToDelete.GetColor();
                switch (opponentColor)
                {
                    case State.Black:
                        Capture(groupToDelete, State.White);
                        break;
                    case State.White:
                        Capture(groupToDelete, State.Black);
                        break;
                    case State.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _markedGroups.RemoveWhere(c => _markedGroups.Contains(c));
        }
    }
}