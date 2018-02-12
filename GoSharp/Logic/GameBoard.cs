using GoSharp.Data;
using GoSharp.Enums;

namespace GoSharp.Logic
{
    internal abstract class GameBoard
    {
        // This method places a stone on the board at coordinate coord with the color of player
        // Returns true if successful and false if not
        public abstract bool PlaceStone(Coord coord, State color, State opponent);

        // Marks the group that the stone at the argument coord is in as dead
        public abstract bool MarkDeadGroup(Coord coord);

        // Returns if true if there is a marked stone at coord, false otherwise
        public abstract bool IsMarked(Coord coord);

        // Deletes all the dead groups from the board
        public abstract void DeleteDead();

        // Returns how many Captures the argument color has
        public abstract int GetCaptures(State color);

        // Returns the current state of each cell on the board
        public abstract State[,] GetGridStones();

        // Returns the size of the board. 9X9 Board returns 9 NOT 81
        public abstract int GetSize();

        // Saves the current gamestate, Argument tells board which is the active player
        public abstract void SaveGame(State currentPlayer, JsonSerialization jsonSerialization);

        // Loads the game and returns the current player
        public abstract State LoadGame(JsonSerialization jsonSerialization);

        // Loads the game and returns the current player
        public abstract State LoadGame(BoardData boardData);
    }
}