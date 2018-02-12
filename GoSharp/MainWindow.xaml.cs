using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GoSharp.Data;
using GoSharp.Enums;
using GoSharp.Exceptions;
using GoSharp.Logic;
using Path = System.IO.Path;

namespace GoSharp
{
    //TODO: Error Exceptions - Denis - Check?
    //TODO: Dependency Injections - Anton - Check?
    //TODO: Folders and Namespaces - Anton - Check
    //TODO: JSON - Denis - Check
    //TODO: Screencast

    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     UI Logic for GoSharp
    /// </summary>
    public partial class MainWindow
    {
        private const int Size = 9; // grid size (9x9)
        private const int NumberOfTiles = Size - 1; // Amount of tiles is always one less than grid corners.

        private const int TileSize = 40; //Size of the grid rectangles. Lower this to get less space between the tiles.

        private readonly JsonSerialization _jsonSerialization; // For saving gamestates

        private static readonly string pathToLoadFrom = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private Game _game; // Game Logic

        public bool GameStarted;

        public MainWindow()
        {
            InitializeComponent();
            _jsonSerialization = new JsonSerialization();
        }

        private void ResizeGameCanvas()
        {
            //Calculate what size the game canvas should have.
            const int xSize = NumberOfTiles * TileSize;
            const int ySize = NumberOfTiles * TileSize;

            //Set the canvas size to the correct dimensions.
            //This is to futureproof - maybe we will let the player choose grid size?
            //Instead of 9x9 - 18x18 etc etc.
            GameCanvas.Height = ySize;
            GameCanvas.Width = xSize;
        }

        /// <summary>
        ///     Creates and draws the grid lines on the game canvas.
        /// </summary>
        private void InitializeGridLines()
        {
            //Create a visual brush to paint gridlines in tile mode.
            var visualBrush = new VisualBrush
            {
                //Make sure the rectangles keep repeating itself
                TileMode = TileMode.Tile,
                //Create viewport/viewbox which has a starting point of X:0 and Y:0 for each new box
                //and with the width and height of the curren tile size.
                Viewport = new Rect(0, 0, TileSize, TileSize),
                Viewbox = new Rect(0, 0, TileSize, TileSize),
                //Make sure the viewport/viewbox units are set locally inside the canvas
                //and not in the full window.
                ViewboxUnits = BrushMappingMode.Absolute,
                ViewportUnits = BrushMappingMode.Absolute
            };

            //Create a rectangle that will be used for the gridlines
            var rectangle = new Rectangle
            {
                Stroke = Brushes.DarkGray,
                StrokeThickness = 1,
                Height = TileSize,
                Width = TileSize
            };

            //Make sure the brush uses the rectangle as the template
            visualBrush.Visual = rectangle;

            //Set the canvas background to the visual brush.
            GameCanvas.Background = visualBrush;

            //Force UI refresh.
            InvalidateVisual();
        }

        /// <summary>
        ///     Calculates which row and column is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Get mouse pos
            Point mousePosition = e.GetPosition(GameCanvas);
            //Get which row in gamecanvas
            var row = (int) Math.Round((float) mousePosition.Y / TileSize);
            //Get which column in gamecanvas
            var column = (int) Math.Round((float) mousePosition.X / TileSize);


            //var coord = new Coord(row, column);
            var coord = new Coord(column, row);
            try
            {
                if (_game.MakeInput(coord)) //Make input to the game, returns true if something is updated
                {
                    DrawBoard(); //Draws the playing field
                }
            }catch(CoordinatesOutsideOfBoardException exe)
            {
                Console.WriteLine(exe.Message);
            }
        }

        /// <summary>
        ///     Draws the GameCanvas after something has changed.
        ///     Optimization can be made to only draw changed objects
        ///     This is determined to not be worthwhile for this application
        /// </summary>
        private void DrawBoard()
        {
            //Remove all children to make sure no stones are lingering.
            GameCanvas.Children.Clear();

            //Get the grid from GameBoard
            State[,] grid = _game.GetBoard().GetGridStones();

            //Iterate through the GameGrid and draw all stones.
            for (var row = 0; row < _game.GetBoard().GetSize(); row++)
            {
                for (var column = 0; column < _game.GetBoard().GetSize(); column++)
                {
                    //if there is no stone at this specific coordinate, don't draw anything.
                    if (grid[column, row] == State.None)
                    {
                        continue;
                    }

                    var coord = new Coord(column, row);

                    //We have a stone, check the owner of the stone and draw the stone with correct color.
                    switch (grid[column, row])
                    {
                        case State.Black:
                            DrawStone(coord, Colors.Black);
                            break;
                        case State.White:
                            DrawStone(coord, Colors.White);
                            break;
                    }
                }
            }

            BlackPoints.Text = "Black: " + _game.GetBoard().GetCaptures(State.Black);
            WhitePoints.Text = "White: " + _game.GetBoard().GetCaptures(State.White);
        }

        /// <summary>
        ///     Draws a stone at the specified coordinates.
        /// </summary>
        /// <param name="coord">Coordinates to draw.</param>
        /// <param name="color">What color to draw the stone as.</param>
        private void DrawStone(Coord coord, Color color)
        {
            //Constant variable to make sure the ellipse get drawn on the intersecting gridlines.
            const double stoneRadius = 19;

            //Create an ellipse on the given coords.
            var ellipse = new Ellipse
            {
                Width = 2 * stoneRadius,
                Height = 2 * stoneRadius
            };

            //Make sure we draw on the actual grid.
            double rowCanvasCoord = coord.Row * TileSize - stoneRadius;
            double columnCanvasCoord = coord.Column * TileSize - stoneRadius;

            ellipse.SetValue(Canvas.LeftProperty, columnCanvasCoord);
            ellipse.SetValue(Canvas.TopProperty, rowCanvasCoord);

            //Add a new brush to paint the ellipse with.
            var solidColorBrush = new SolidColorBrush {Color = color};

            ellipse.Fill = solidColorBrush;
            ellipse.StrokeThickness = 2;
            ellipse.Stroke = Brushes.Black;

            if (_game.GetState() == GameState.Counting) // If in counting state, mark marked stones
            {
                if (_game.IsMarked(coord))
                {
                    ellipse.StrokeThickness = 5;
                    ellipse.Stroke = Brushes.Purple;
                }
            }

            //Add and draw the ellipse.
            GameCanvas.Children.Add(ellipse);

            //Force redraw UI
            InvalidateVisual();
        }

        // Saves the current gamestate
        private void MenuItemSaveGame_OnClick(object sender, RoutedEventArgs e)
        {
            if (GameStarted == false)
            {
                return;
            }

            _game.GetBoard().SaveGame(_game.GetPlayer(), _jsonSerialization);
        }

        // Load a saved gamestate
        private void MenuItemLoadGame_OnClick(object sender, RoutedEventArgs e)
        {

            LoadWindow loadWindow = new LoadWindow(_jsonSerialization);
            loadWindow.Show();

        }

        // Create new game
        private void MenuItemNewGame_OnClick(object sender, RoutedEventArgs e)
        {
            _game = new Game(Size, new Board(Size));

            MenuItemSaveGame.IsEnabled = true;
            ResizeGameCanvas();

            InitializeGridLines();

            DrawBoard();

            GameStarted = true;
        }

        private void MenuItemMarkDead_OnClick(object sender, RoutedEventArgs e)
        {
            if (_game.GetState() == GameState.Counting)
            {
                MenuItemMarkDead.Header = "Mark dead stones";
                _game.RemoveMarkedStones();
                _game.ResumePlay();
                DrawBoard();
            }
            else
            {
                _game.ScoreGame();
                MenuItemMarkDead.Header = "Marking done";
            }
        }

        public void LoadGame(BoardData save)
        {
            if (GameStarted == false)
            {
                _game = new Game(Size, new Board(Size));

                MenuItemSaveGame.IsEnabled = true;
                ResizeGameCanvas();
                
                InitializeGridLines();

                GameStarted = true;
            }


            //State activePlayer = _game.GetBoard().LoadGame(_jsonSerialization);
            try
            {
                State activePlayer = _game.GetBoard().LoadGame(save);
                _game.SetPlayer(activePlayer);
                DrawBoard();
            }catch(SaveDataCorruptedException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void MenuItemLoadLatestGame_OnClick(object sender, RoutedEventArgs e)
        {
            LoadGame(_jsonSerialization.GetLatestSaveFile(pathToLoadFrom));
        }
    }
}