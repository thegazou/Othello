using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Othello_logique;

namespace Othello_graphique
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Tile[,] listTiles = new Tile[8, 8];
        private Engine engine = new Engine();

        public MainWindow()
        {
            InitializeComponent();
            initializeBoard();
            InitializeBinding();
        }
         
        /// <summary>
        /// Append when the player click on the Tile.
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public void Tile_Click(object sender, RoutedEventArgs e)
        {
            Tile tile = (Tile)sender;
            try
            {
                if (engine.isPlayable(tile.Col, tile.Row, engine.CurrentPlayer) && !engine.IsOnPause)
                {
                    engine.playMove(tile.Col, tile.Row, engine.CurrentPlayer);
                    majBoard();
                }
                
            }
            catch
            {
                Console.WriteLine("Aucune partie en cours");
            }
        }

        /// <summary>
        /// Will update the tile value
        /// </summary>
        /// <param name="x">value between 0 and 7</param>
        /// <param name="y">value between 0 and 7</param>
        /// <param name="value">1 for white, -1 for black</param>
        /// <returns></returns>
        public void UpdateTile(int x, int y, int value)
        {
            try
            {
                listTiles[y, x].Pion = value;
                bool turn;
                if (engine.CurrentPlayer == 1)
                {
                    turn = false;
                }
                else
                {
                    turn = true;
                }
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        //Met à jour les cases voisines
                        listTiles[i, j].changeBackground(engine.isPlayable(j, i, turn));
                    }
                }
            }
            catch { }
            
        }

        /// <summary>
        /// Initialize a new list of Tile
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private void initializeBoard()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    listTiles[i, j] = new Tile(this, i, j);
                    Grid.SetRow(listTiles[i, j], i);
                    Grid.SetColumn(listTiles[i, j], j);
                    Board.Children.Add(listTiles[i, j]);
                   
                }
            }
        }

        /// <summary>
        /// Will Update the board
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public void majBoard()
        {
            try
            {
                bool turn;
                if (engine.CurrentPlayer == 1)
                {
                    turn = false;
                }
                else
                {
                    turn = true;
                }
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        //Met à jour les cases
                        listTiles[i, j].Pion = engine.board.GetSquare(j, i);
                        //Change le background des cases jouables
                        listTiles[i, j].changeBackground(engine.isPlayable(j, i, turn));
                    }
                }
            }
            catch
            {
                Console.WriteLine("Update Fail");
            }
        }

        /// <summary>
        /// Initialize all the bindings
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private void InitializeBinding()
        {

           //Binding Turn
            Binding BindingTurn = new Binding();
            BindingTurn.Path = new PropertyPath("CurrentPlayer");
            BindingTurn.Mode = BindingMode.OneWay;
            BindingTurn.Converter = new StringConverter();
            BindingTurn.Source = engine;
            tbTurn.DataContext = engine;
            tbTurn.SetBinding(TextBlock.TextProperty, BindingTurn);
            

            //Binding Black Timer
            Binding BindingBlackTimer = new Binding();
            BindingBlackTimer.Path = new PropertyPath("BlackTimer");
            BindingBlackTimer.Mode = BindingMode.OneWay;
            BindingBlackTimer.Source = engine;
            lblTimerBlack.DataContext = engine;
            lblTimerBlack.SetBinding(TextBlock.TextProperty, BindingBlackTimer);

            //Binding White Timer
            Binding BindingWhiteTimer = new Binding();
            BindingWhiteTimer.Path = new PropertyPath("WhiteTimer");
            BindingWhiteTimer.Mode = BindingMode.OneWay;
            BindingWhiteTimer.Source = engine;
            lblTimerWhite.DataContext = engine;
            lblTimerWhite.SetBinding(TextBlock.TextProperty, BindingWhiteTimer);

            //Binding White Score
            Binding BindingWhiteScore = new Binding();
            BindingWhiteScore.Path = new PropertyPath("WhiteScore");
            BindingWhiteScore.Mode = BindingMode.OneWay;
            BindingWhiteScore.Source = engine;
            lblScoreWhite.DataContext = engine;
            lblScoreWhite.SetBinding(TextBlock.TextProperty, BindingWhiteScore);

            //Binding Black Score
            Binding BindingBlackScore = new Binding();
            BindingBlackScore.Path = new PropertyPath("BlackScore");
            BindingBlackScore.Mode = BindingMode.OneWay;
            BindingBlackScore.Source = engine;
            lblScoreBlack.DataContext = engine;
            lblScoreBlack.SetBinding(TextBlock.TextProperty, BindingBlackScore);

            Binding BindingPause = new Binding();
            BindingPause.Path = new PropertyPath("IsOnPause");
            BindingPause.Mode = BindingMode.OneWay;
            BindingPause.Converter = new StringConverter();
            BindingPause.Source = engine;
            btnPause.DataContext = engine;
            btnPause.SetBinding(Button.ContentProperty, BindingPause);

        }

        /// <summary>
        /// Will launch a new game
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if(engine.IsOnPause)
            {
                engine.ResumeGame();
            }
            engine.NewGame();
            majBoard();
        }

        /// <summary>
        /// Will save the game
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            engine.SaveGame();
        }

        /// <summary>
        /// Will load a game
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {

            //If there is no game initialized we initialized one before. (usefull when the first thing the user do is to load a game).
            engine.NewGame();
            engine.LoadGame();
            majBoard();
                
        }

        /// <summary>
        /// Will pause a new game
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if(engine.board != null)
            { 
                if (!engine.IsOnPause)
                {
                    engine.PauseGame();
                }
                else
                {
                    engine.ResumeGame();
                }
            }
        }

        /// <summary>
        /// Will go back
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            engine.Undo();
            majBoard();
        }

        /// <summary>
        /// Will launch a new online game
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private void btnOnline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string opponentIp = tbxIp.Text;
                int opponentPort = Convert.ToInt32(tbxPort.Text);
                int player;
                if(rbtnBlack.IsChecked == true)
                {
                    player = 1;
                }
                else
                {
                    player = -1;
                }
                engine.NewOnlineGame(opponentIp, opponentPort, player);
            }
            catch
            {
                Console.WriteLine("Fail to start Online Mode");
            }
            majBoard();     
        }
    }
}
