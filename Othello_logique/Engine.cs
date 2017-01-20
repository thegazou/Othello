// Author: Nicolas Gonin
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;
using System.Threading;

namespace Othello_logique
{
    class Engine : IPlayable
    {
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
        private int player = BLACK;
        private Stopwatch blackTimer = new Stopwatch();
        private Stopwatch whiteTimer = new Stopwatch();
        private Stack<int[,]> boardHistory = new Stack<int[,]>();
        private Stack<int> playerHistory = new Stack<int>();
        private Decimal blackOffsetTime = 0;//Used if a game is loaded from a save.
        private Decimal whiteOffsetTime = 0;//Used if a game is loaded from a save.
        private bool isSavingInProgress = false;

        //properties
        public int Player
        {
            get { return player; }
            private set { }
        }
        /// <summary>
        /// Get the playing time of the black player rounded to one decimal.
        /// </summary>
        public Decimal BlackTimer
        {
            get { return decimal.Round(Convert.ToDecimal(blackTimer.ElapsedMilliseconds) + blackOffsetTime, 1); }
            private set { }
        }
        /// <summary>
        /// Get the playing time of the white player rounded to one decimal.
        /// </summary>
        public Decimal WhiteTimer
        {
            get { return decimal.Round(Convert.ToDecimal(whiteTimer.ElapsedMilliseconds) + whiteOffsetTime, 1); }
            private set { }
        }

        /// <summary>
        /// Tells if the game is being saved in an XML file.
        /// </summary>
        public bool IsSavingInProgress
        {
            get { return isSavingInProgress; }
            private set { }
        }

        /// <summary>
        /// Start a new game.
        /// </summary>
        public void NewGame()
        {
            player = BLACK;
            board = Board.StartingBoard();
            blackTimer.Reset();
            whiteTimer.Reset();
            blackTimer.Start();
        }

        /// <summary>
        /// Stop the stopwatch of the current player.
        /// </summary>
        public void PauseGame()
        {
            if (player == WHITE)
                whiteTimer.Stop();
            else
                blackTimer.Stop();
        }

        /// <summary>
        /// Start the stopwhatch of the current player.
        /// </summary>
        public void ResumeGame()
        {
            if (player == WHITE)
                whiteTimer.Start();
            else
                blackTimer.Start();
        }

        /// <summary>
        /// Set the player and the board as it was before the last move.
        /// </summary>
        /// <returns>Returns true if the action is possible and false otherwise.</returns>
        public bool Undo()
        {
            if (boardHistory.Any())
            {
                board.SetBoard(boardHistory.Pop());
                player = playerHistory.Pop();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Save the game to the file with the given path.
        /// This operation is done in a thread.
        /// </summary>
        /// <param name="fileName">File path default value is "Save.xml".</param>
        public void SaveGame(string fileName = "Save.xml")
        {
            while (isSavingInProgress == true) ;
            isSavingInProgress = true;
            new Thread(() =>
            {
                SavableEngine engineState = new SavableEngine();
                engineState.BlackTimer = this.BlackTimer;
                engineState.WhiteTimer = this.WhiteTimer;
                engineState.Player = this.Player;
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
            while (isSavingInProgress == true) ;
            SavableEngine sourceEngine;
            Deserialize(fileName, out sourceEngine);
            this.whiteOffsetTime = sourceEngine.WhiteTimer;
            this.blackOffsetTime = sourceEngine.BlackTimer;
            this.player = sourceEngine.Player;

            this.board.SetBoard(Engine.Convert1DTo2DBoardArray(sourceEngine.Board));
            this.playerHistory = new Stack<int>(sourceEngine.PlayerHistory);

            foreach (int[] source in sourceEngine.BoardHistory)
                this.boardHistory.Push(Engine.Convert1DTo2DBoardArray(source));
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
            if (board.GameOver() == false)
                player = EMPTY;
            else
            {
                if (player == BLACK)
                {
                    blackTimer.Stop();
                    whiteTimer.Start();
                }
                else
                {
                    whiteTimer.Stop();
                    blackTimer.Start();
                }
                player = -player;
                if (board.CanPlay(player) == false)
                    nextTurn();
            }
        }

        /// <summary>
        /// Save the current player and the current board.
        /// </summary>
        private void SaveState()
        {
            boardHistory.Push(board.GetBoardCopy());
            playerHistory.Push(player);
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
            isSavingInProgress = false;
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
    }
}
