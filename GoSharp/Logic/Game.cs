using GoSharp.Enums;
using GoSharp.Exceptions;

namespace GoSharp.Logic
{
    internal class Game
    {
        //The board logic.
        private readonly GameBoard _gameBoard;

        //The player who is currently placing a stone
        private State _activePlayer;

        //The player who is not placing a stone.
        private State _passivePlayer;

        private GameState _state;

        /// <summary>
        ///     Creates an instance of Game. Handles the simple tasks as keeping track of who is the current player.
        ///     Holds the connection to GameBoard.
        /// </summary>
        /// <param name="size">Size of the grid to be created.</param>
        /// <param name="board">Board to use.</param>
        public Game(int size, GameBoard board)
        {
            _state = GameState.Playing;
            _activePlayer = State.Black;
            _passivePlayer = State.White;

            _gameBoard = board;
        }

        public GameState GetState()
        {
            return _state;
        }

        /// <summary>
        ///     Checks with GameBoard logic if it is possible placing a stone at the given coordinates.
        ///     If possible, the stone is placed and the next player is allowed to play.
        /// </summary>
        /// <param name="coord">Coordinates to place stone at.</param>
        /// <returns><c>True</c> if allowed to place stone at coordinates.</returns>
        public bool MakeMove(Coord coord)
        {
            try
            {
                if (_gameBoard.PlaceStone(coord, _activePlayer, _passivePlayer))
                {
                    ChangeActive();
                    return true;
                }
                return false;
            }
            catch (CoordinatesOutsideOfBoardException e)
            {
                throw e;
            }
        }

        /// <summary>
        ///     Switches to the next player.
        /// </summary>
        private void ChangeActive()
        {
            State temp = _activePlayer;
            _activePlayer = _passivePlayer;
            _passivePlayer = temp;
        }

        /// <summary>
        ///     Gets the currently playing player.
        /// </summary>
        /// <returns>Active player</returns>
        public State GetPlayer()
        {
            return _activePlayer;
        }

        /// <summary>
        ///     Returns the GameGrid.
        /// </summary>
        /// <returns>GameGrid</returns>
        public GameBoard GetBoard()
        {
            return _gameBoard;
        }

        public bool MarkDead(Coord coord)
        {
            _gameBoard.MarkDeadGroup(coord);

            return true;
        }

        public bool IsMarked(Coord coord)
        {
            return _gameBoard.IsMarked(coord);
        }

        public void ScoreGame()
        {
            _state = GameState.Counting;
        }

        public void ResumePlay()
        {
            _state = GameState.Playing;
        }

        public void RemoveMarkedStones()
        {
            _gameBoard.DeleteDead();
        }

        /// <summary>
        ///     Sets the current player. Useful if a game has been loaded from save file.
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayer(State player)
        {
            switch (player)
            {
                case State.Black:
                    _activePlayer = State.Black;
                    _passivePlayer = State.White;
                    break;
                case State.White:
                    _activePlayer = State.White;
                    _passivePlayer = State.Black;
                    break;
                default:
                    return;
            }
        }

        public bool MakeInput(Coord coord)
        {
            if (_state == GameState.Playing)
            {
                try
                {
                    return MakeMove(coord);
                }catch(CoordinatesOutsideOfBoardException e)
                {
                    throw e;
                }
            }
            return MarkDead(coord);
        }
    }
}