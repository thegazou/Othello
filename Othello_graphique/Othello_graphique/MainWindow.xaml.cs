using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            engine.NewGame();
            InitializeBinding();
            initializeBoard();
           

        }

        public void Tile_Click(object sender, RoutedEventArgs e)
        {
            Tile tile = (Tile)sender;

            if(engine.isPlayable(tile.Col,tile.Row,engine.CurrentPlayer))
            {
                engine.playMove(tile.Col, tile.Row, engine.CurrentPlayer);
                lblScoreBlack.Content = engine.getWhiteScore().ToString();
                lblScoreWhite.Content = engine.getBlackScore().ToString();
            }
            majBoard();
            
            
        }

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

        private void majBoard()
        {
            bool turn;
            if(engine.CurrentPlayer == 1)
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

        private void InitializeBinding()
        {
            //Binding Turn
            Binding BindingTurn = new Binding();
            BindingTurn.Path = new PropertyPath("CurrPlayBind");
            BindingTurn.Mode = BindingMode.OneWay;
            BindingTurn.Source = engine;
            tbTurn.DataContext = engine;
            tbTurn.SetBinding(TextBlock.TextProperty, BindingTurn);

            //Binding Black Timer
            Binding BindingBlackTimer = new Binding();
            BindingBlackTimer.Path = new PropertyPath("BlackTimer");
            BindingBlackTimer.Mode = BindingMode.OneWay;
            BindingBlackTimer.Source = engine;
            lblTimeBlack.DataContext = engine;
            lblTimeBlack.SetBinding(TextBlock.TextProperty, BindingBlackTimer);

            //Binding White Timer
            Binding BindingWhiteTimer = new Binding();
            BindingWhiteTimer.Path = new PropertyPath("WhiteTimer");
            BindingWhiteTimer.Mode = BindingMode.OneWay;
            BindingWhiteTimer.Source = engine;
            lblTimeWhite.DataContext = engine;
            lblTimeWhite.SetBinding(TextBlock.TextProperty, BindingWhiteTimer);
            
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            engine.NewGame();
            majBoard();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            engine.SaveGame();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            engine.LoadGame();
            majBoard();
            lblScoreBlack.Content = engine.getWhiteScore().ToString();
            lblScoreWhite.Content = engine.getBlackScore().ToString();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            engine.PauseGame();
        }
    }
}
