﻿// Author: Nicolas Gonin
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Windows;
using Othello_graphique;

namespace Othello_logique
{
    class Engine : IPlayable, INotifyPropertyChanged
    {

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method when a property is changed
        /// </summary>
        private void FirePropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        //public constants
        public const int BLACK = 1;
        public const int WHITE = -1;
        public const int EMPTY = 0;
        public const int BOARD_SIZE = 8;

        //public object field
        public Board board;

        //private fields
        private delegate Tuple<int, int> GetNextMove(int[,] game, int player);
        private GetNextMove[] strategies = { IA.Strategie1, IA.Strategie2 };
        private Stack<int[,]> boardHistory = new Stack<int[,]>();
        private Stack<int> playerHistory = new Stack<int>();
        private Decimal blackOffsetTime = 0;//Used if a game is loaded from a save.
        private Decimal whiteOffsetTime = 0;//Used if a game is loaded from a save.     
        private bool isOnline = false;
        private string opponentIp;
        private int opponengPort;
        private System.Windows.Threading.DispatcherTimer dt = new System.Windows.Threading.DispatcherTimer();
        private BackgroundWorker listeningWorker;

        private bool isOpponentTurn;
        /// <summary>
        /// Tells if it's the opponnent's turn.
        /// </summary>
        public bool IsOpponentTurn
        {
            get { return isOpponentTurn; }
            private set { isOpponentTurn = value; }
        }

        private bool isOnPause = false;
        /// <summary>
        /// Tells if the game is on pause.
        /// </summary>
        public bool IsOnPause
        {
            get { return isOnPause; }
            private set { isOnPause = value; FirePropertyChanged("IsOnPause"); }
        }

        private int startingPlayer;
        /// <summary>
        /// Get starting color of the player. Useful when playing online.
        /// </summary>
        public int Player
        {
            get { return startingPlayer; }
            private set { startingPlayer = value; }
        }

        private int currentPlayer;
        /// <summary>
        /// Get the color of the current player.
        /// </summary>
        public int CurrentPlayer
        {
            get { return currentPlayer; }
            private set { currentPlayer = value; FirePropertyChanged("CurrentPlayer"); }
        }

        private int whiteScore;
        /// <summary>
        /// Get the white score.
        /// </summary>
        public int WhiteScore
        {
            get { return whiteScore; }
            private set { whiteScore = value; FirePropertyChanged("WhiteScore"); }
        }

        private int blackScore;
        /// <summary>
        /// Get the black score.
        /// </summary>
        public int BlackScore
        {
            get { return blackScore; }
            private set { blackScore = value; FirePropertyChanged("BlackScore"); }
        }

        private Stopwatch blackTimer = new Stopwatch();
        /// <summary>
        /// Get the playing time of the black player in second rounded to one decimal.
        /// </summary>
        public Decimal BlackTimer
        {
            get { return decimal.Round((Convert.ToDecimal(blackTimer.ElapsedMilliseconds) / 1000 + blackOffsetTime), 1); }
            private set { FirePropertyChanged("BlackTimer"); }

        }

        private Stopwatch whiteTimer = new Stopwatch();
        /// <summary>
        /// Get the playing time of the white player in second rounded to one decimal.
        /// </summary>
        public Decimal WhiteTimer
        {
            get { return decimal.Round((Convert.ToDecimal(whiteTimer.ElapsedMilliseconds) / 1000 + whiteOffsetTime), 1); }
            private set { FirePropertyChanged("WhiteTimer"); }
        }

        private bool isSavingInProgress = false;
        /// <summary>
        /// Tells if the game is being saved in an XML file.
        /// </summary>
        public bool IsSavingInProgress
        {
            get { return isSavingInProgress; }
            private set { isSavingInProgress = value; }
        }

        /// <summary>
        /// Start a new game.
        /// </summary>
        public void NewGame(int player = BLACK)
        {
            this.Player = player;
            this.CurrentPlayer = player;
            board = Board.StartingBoard();
            dt.Tick += new EventHandler(dt_Tick);
            dt.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dt.Start();
            blackTimer.Reset();
            whiteTimer.Reset();
            blackTimer.Start();
            WhiteScore = board.GetWhiteScore();
            BlackScore = board.GetBlackScore();

            // Happends if starting online game as second player

            if (Player == WHITE)
            {

                IsOpponentTurn = true;

                ((MainWindow)Application.Current.MainWindow).majBoard();
                listeningWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Start a new game with a player at the given IpAdress and port number.
        /// </summary>
        /// <param name="opponentIp"></param>
        /// <param name="opponentIp"></param>
        /// <param name="player"></param>
        public void NewOnlineGame(string opponentIp, int opponentPort, int player)
        {
            listeningWorker = new BackgroundWorker();
            listeningWorker.DoWork += new DoWorkEventHandler(bw_DoWork);
            listeningWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            isOnline = true;
            this.opponentIp = opponentIp;
            this.opponengPort = opponentPort;
            NewGame(player);
        }

        /// <summary>
        /// Stop the stopwatch of the current player.
        /// </summary>
        public void PauseGame()
        {
            IsOnPause = true;
            if (CurrentPlayer == WHITE)
                whiteTimer.Stop();
            else
                blackTimer.Stop();
            dt.Stop();
        }

        /// <summary>
        /// Start the stopwhatch of the current player.
        /// </summary>
        public void ResumeGame()
        {
            IsOnPause = false;
            dt.Start();
            if (CurrentPlayer == WHITE)
                whiteTimer.Start();
            else
                blackTimer.Start();
        }

        /// <summary>
        /// Set the current player and the board as it was before the last move.
        /// </summary>
        /// <returns>Returns true if the action is possible and false otherwise.</returns>
        public bool Undo()
        {
            if (boardHistory.Any())
            {
                board.SetBoard(boardHistory.Pop());
                CurrentPlayer = playerHistory.Pop();
                WhiteScore = board.GetWhiteScore();
                BlackScore = board.GetBlackScore();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Save the game to the file with the given path.
        /// This operation is done in a thread.
        /// </summary>
        /// <param name="fileName">File path default value is "Save.xml".</param>
        public void SaveGame(string fileName = "Save.xml")
        {
            while (IsSavingInProgress == true) ;
            IsSavingInProgress = true;
            new Thread(() =>
            {
                SavableEngine engineState = new SavableEngine();
                engineState.BlackTimer = this.BlackTimer;
                engineState.WhiteTimer = this.WhiteTimer;
                engineState.Player = this.CurrentPlayer;
                engineState.Board = Engine.Convert2DTo1DBoardArray(this.board.GetBoardCopy());

                int historySize = playerHistory.Count;
                engineState.PlayerHistory = new int[historySize];
                this.playerHistory.CopyTo(engineState.PlayerHistory, 0);

                int[][,] tempBoardHistory = this.boardHistory.ToArray();
                engineState.BoardHistory = new int[historySize][];
                for (int i = 0; i < historySize; i++)
                {
                    engineState.BoardHistory[i] = Engine.Convert2DTo1DBoardArray(tempBoardHistory[i]);
                }
                Serialize(fileName, engineState);
            }).Start();
        }

        /// <summary>
        /// Load the game from the file with the given path.
        /// </summary>
        /// <param name="fileName">File path default value is "Save.xml".</param>
        public void LoadGame(string fileName = "Save.xml")
        {
            while (IsSavingInProgress == true) ;
            PauseGame();
            SavableEngine sourceEngine;
            Deserialize(fileName, out sourceEngine);
            this.whiteTimer.Reset();
            this.blackTimer.Reset();
            this.whiteOffsetTime = sourceEngine.WhiteTimer;
            this.blackOffsetTime = sourceEngine.BlackTimer;
            this.BlackTimer = blackOffsetTime;
            this.WhiteTimer = whiteOffsetTime;
            this.CurrentPlayer = sourceEngine.Player;
            this.Player = sourceEngine.Player;
            this.board.SetBoard(Engine.Convert1DTo2DBoardArray(sourceEngine.Board));
            this.board.Print();
            this.playerHistory = new Stack<int>(sourceEngine.PlayerHistory.Reverse());
            foreach (int[] source in sourceEngine.BoardHistory.Reverse())
                this.boardHistory.Push(Engine.Convert1DTo2DBoardArray(source));
            WhiteScore = board.GetWhiteScore();

        }

        /// <summary>
        /// Will update the board status with the given move and switch players
        /// </summary>
        /// <param name="column">value between 0 and 7</param>
        /// <param name="line">value between 0 and 7</param>
        /// <param name="player"></param>
        public void playMove(int column, int line, int player)
        {
            Tuple<int, int> index = new Tuple<int, int>(column, line);
            SaveState();
            board.PlayMove(index, player);
            WhiteScore = getWhiteScore();
            BlackScore = getBlackScore();
            if (isOnline && IsOpponentTurn == false)
            {
                Network.SendInput(column, line, opponentIp, opponengPort);
                Thread.Sleep(3000);
            }
            nextTurn();
        }

        /// <summary>
        /// Returns true if the move is valid for specified player.
        /// </summary>
        /// <param name="column">value between 0 and 7</param>
        /// <param name="line">value between 0 and 7</param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool isPlayable(int column, int line, int player)
        {
            Tuple<int, int> index = new Tuple<int, int>(column, line);
            return board.CanMove(index, player);

        }
        /*############################################################################
        ##               Functions from IPlayble                                    ##
        ############################################################################*/

        /// <summary>
        /// Returns true if the move is valid for specified color
        /// </summary>
        /// <param name="column">value between 0 and 7</param>
        /// <param name="line">value between 0 and 7</param>
        /// <param name="isWhite"></param>
        /// <returns></returns>
        public bool isPlayable(int column, int line, bool isWhite)
        {
            Tuple<int, int> index = new Tuple<int, int>(column, line);
            if (isWhite)
                return board.CanMove(index, WHITE);
            else
                return board.CanMove(index, BLACK);
        }

        /// <summary>
        /// Will update the board status and switch players if the move is valid and legal and return true
        /// Will return false otherwise (board is unchanged)
        /// </summary>
        /// <param name="column">value between 0 and 7</param>
        /// <param name="line">value between 0 and 7</param>
        /// <param name="isWhite">true for white move, false for black move</param>
        /// <returns></returns>
        public bool playMove(int column, int line, bool isWhite)
        {
            Tuple<int, int> index = new Tuple<int, int>(column, line);
            if (isWhite && board.CanMove(index, WHITE))
            {
                SaveState();
                board.PlayMove(index, WHITE);
                nextTurn();
                return true;
            }
            else if (board.CanMove(index, BLACK))
            {
                SaveState();
                board.PlayMove(index, BLACK);
                nextTurn();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Asks the game engine next (valid) move given a game position
        /// </summary>
        /// <param name="game">a 2D board with 0 for white 1 for black and -1 for empty tiles. First index for the column, second index for the line</param>
        /// <param name="level">an integer value to set the level of the IA</param>
        /// <param name="whiteTurn">true if white players turn, false otherwise</param>
        /// <returns></returns>
        public Tuple<char, int> getNextMove(int[,] game, int level, bool whiteTurn)
        {
            if (whiteTurn)
                return ConvertCoordToCharInt(strategies[level](ConvertProfBoardToMyBoard(game), WHITE));
            else
                return ConvertCoordToCharInt(strategies[level](ConvertProfBoardToMyBoard(game), BLACK));
        }

        /// <summary>
        /// Returns the number of white tiles on the board
        /// </summary>
        /// <returns></returns>
        public int getWhiteScore()
        {
            return board.GetWhiteScore();
        }

        /// <summary>
        /// Returns the number of black tiles on the board
        /// </summary>
        /// <returns></returns>
        public int getBlackScore()
        {
            return board.GetBlackScore();
        }

        /*############################################################################
        ##               Functions tools                                            ##
        ############################################################################*/

        /// <summary>
        /// Switch current player and manage the stopwhatch of the players.
        /// This function will be called until one player can play.
        /// If no player is able to play, the current player will be set to EMPTY.
        /// </summary>
        private void nextTurn()
        {
            if (board.GameOver() == true)
            {
                CurrentPlayer = EMPTY;
                EndScreen();
            }
            else
            {
                if (CurrentPlayer == BLACK)
                {
                    blackTimer.Stop();
                    whiteTimer.Start();
                }
                else
                {
                    whiteTimer.Stop();
                    blackTimer.Start();
                }
                CurrentPlayer = -CurrentPlayer;
                IsOpponentTurn = !IsOpponentTurn;
                if (board.CanPlay(CurrentPlayer) == false)
                {
                    nextTurn();
                }

                if (isOnline && IsOpponentTurn)
                {
                    listeningWorker.RunWorkerAsync();
                }
            }

        }

        /// <summary>
        /// Save the current player and the current board.
        /// </summary>
        private void SaveState()
        {
            boardHistory.Push(board.GetBoardCopy());
            playerHistory.Push(CurrentPlayer);
        }

        /// <summary>
        /// Return the tuple char,int representation of the tuple int, int.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Tuple<char, int> ConvertCoordToCharInt(Tuple<int, int> index)
        {
            return new Tuple<char, int>((char)(index.Item1 + (int)'A'), index.Item2);
        }

        /// <summary>
        /// Convert a one dimensional array representation of the given board to a two dimensional array.
        /// Used by the deserialization protocole.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>Returns a new two dimensional array.</returns>
        private static int[,] Convert1DTo2DBoardArray(int[] source)
        {
            int[,] target = new int[BOARD_SIZE, BOARD_SIZE];
            int x = 0;
            int y = -1;
            for (int i = 0; i < BOARD_SIZE * BOARD_SIZE; i++)
            {
                x = i % BOARD_SIZE;
                if (x == 0)
                    y++;
                target[x, y] = source[i];
            }
            return target;
        }

        /// <summary>
        /// Convert a two dimensional array representation of the given board to a one dimensional array.
        /// Used by the serialization protocole.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>Returns a new one dimensional array.</returns>
        private static int[] Convert2DTo1DBoardArray(int[,] source)
        {
            int[] target = new int[BOARD_SIZE * BOARD_SIZE];
            int i = 0;
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                for (int x = 0; x < BOARD_SIZE; x++)
                {
                    target[i] = source[x, y];
                    i++;
                }
            }
            return target;
        }

        /// <summary>
        /// Deserialize the file with the given path to instantiate the given SavableEngine.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="engineState"></param>
        private void Deserialize(string fileName, out SavableEngine sourceEngine)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SavableEngine));
            serializer.UnknownNode += new XmlNodeEventHandler(Engine.Serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(Engine.Serializer_UnknownAttribute);
            FileStream fs = new FileStream(fileName, FileMode.Open);
            sourceEngine = (SavableEngine)serializer.Deserialize(fs);
            fs.Close();
        }

        /// <summary>
        /// Serialize the given SavableEngine in the file with the given path.
        /// This operation done in a thread.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="engineState"></param>
        private void Serialize(string fileName, SavableEngine engineState)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SavableEngine));
            StreamWriter writer = new StreamWriter(fileName);
            serializer.Serialize(writer, engineState);
            writer.Close();
            IsSavingInProgress = false;
        }

        /// <summary>
        /// Print an error in the console if the XML document has been altered with unknown nodes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
        }

        /// <summary>
        /// Print an error in the console if the XML document has been altered with unknown nodes or attributes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("Unknown attribute " + attr.Name + "='" + attr.Value + "'");
        }

        /// <summary>
        /// Convert the player value notation of Olivier Hüsser to mines.
        /// </summary>
        /// <param name="profBoard"></param>
        /// <returns></returns>
        private static int[,] ConvertProfBoardToMyBoard(int[,] profBoard)
        {
            int[,] myBoard = new int[BOARD_SIZE, BOARD_SIZE];
            for (int x = 0; x < BOARD_SIZE; x++)
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    switch (profBoard[y, x])
                    {
                        case -1:
                            myBoard[y, x] = EMPTY;
                            break;
                        case 0:
                            myBoard[y, x] = WHITE;
                            break;
                        case 1:
                            myBoard[y, x] = BLACK;
                            break;
                        default:
                            Console.WriteLine("ERROR: givenBoard has an invalid value!");
                            break;
                    }
                }
            return myBoard;
        }

        /// <summary>
        /// Update the timers in the UI 10 time per seconde.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dt_Tick(object sender, EventArgs e)
        {
            BlackTimer = 0;
            WhiteTimer = 0;
        }

        /// <summary>
        /// Listen for the opponnent move.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            int[] move = Network.GetInput();
            e.Result = new Tuple<int, int>(move[0], move[1]);
        }

        /// <summary>
        /// play the opponnent's move.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Tuple<int, int> result = (Tuple<int, int>)e.Result;
            playMove(result.Item1, result.Item2, CurrentPlayer);
        }


        /// <summary>
        /// Display the winner and his score on an information box.
        /// Ask if the user if he wants to continue playing.
        /// </summary>
        private void EndScreen()
        {
            string message;
            if (BlackScore == WhiteScore)
            {
                message = "It's a draw! \n Do you want to play again?";
            }
            else
            {
                string winner;
                int winnerScore;
                if (BlackScore > WhiteScore)
                {
                    winner = "black";
                    winnerScore = BlackScore;
                }
                else
                {
                    winner = "white";
                    winnerScore = WhiteScore;
                }
                message = String.Format("The {0} player wins with {1} points!\n Do you want to play again?", winner, winnerScore);
            }

            if (MessageBox.Show(message, "My Application", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
