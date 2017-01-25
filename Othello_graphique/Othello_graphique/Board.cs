// Author: Nicolas Gonin
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Othello_logique
{
    class Board
    {

        //public constants
        public const int BOARD_SIZE = 8;
        public const int BLACK = 1;
        public const int WHITE = -1;
        public const int EMPTY = 0;
        public const int INVALID = -2;

        //private field
        private int[,] board = new int[BOARD_SIZE, BOARD_SIZE];

        //public field
        public int[,] BoardState
        {
            get { return board; }
            private set { }
        }

        /// <summary>
        /// Returns true if the given player can make any move.
        /// returns false otherwise.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool CanPlay(int player)
        {
            foreach (Tuple<int, int> indices in GetSquareIndices(EMPTY))
            {
                if (CanMove(indices, player) == true)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the move is valid and legal.
        /// Returns false otherwise.
        /// </summary>
        /// <param name="index">Ind</param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool CanMove(Tuple<int, int> index, int player)
        {
            // Place the piece at the selected location
            if (IsSquareValid(index) == false || GetSquare(index) != EMPTY)
                return false;
            int value = 0;
            int xDirection; //Horizontal direction
            int yDirection; //Vertical direction
            int distance; //Distance
            int x = index.Item1;
            int y = index.Item2;
            int xTemp;
            int yTemp;

            for (xDirection = -1; xDirection <= 1; xDirection++)
            {
                for (yDirection = -1; yDirection <= 1; yDirection++)
                {
                    if (!(xDirection == 0 && yDirection == 0))
                    {
                        distance = 1;
                        xTemp = x + xDirection;
                        yTemp = y + yDirection;
                        while (GetSquare(xTemp, yTemp) == -player)
                        {
                            distance++;
                            xTemp += xDirection;
                            yTemp += yDirection;
                        }
                        if (distance > 1 && GetSquare(xTemp, yTemp) == player)
                            value += distance - 1;
                    }
                }
            }
            if (value > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Updates the board status with the given move.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="player"></param>
        public void PlayMove(Tuple<int, int> index, int player)
        {
            // Place the piece at the selected location
            board[index.Item1, index.Item2] = player;
            int xDirection; //Horizontal direction
            int yDirection; //Vertical direction
            int distance; //Distance
            int x = index.Item1;
            int y = index.Item2;
            int xTemp;
            int yTemp;

            for (xDirection = -1; xDirection <= 1; xDirection++)
            {
                for (yDirection = -1; yDirection <= 1; yDirection++)
                {
                    if (!(xDirection == 0 && yDirection == 0))
                    {
                        distance = 1;
                        xTemp = x + xDirection;
                        yTemp = y + yDirection;
                        while (IsSquareValid(new Tuple<int, int>(xTemp, yTemp)) && board[xTemp, yTemp] == -player)
                        {
                            distance++;
                            xTemp += xDirection;
                            yTemp += yDirection;
                        }
                        if (IsSquareValid(new Tuple<int, int>(xTemp, yTemp)) && distance > 1 && board[xTemp, yTemp] == player)
                        {
                            do
                            {
                                board[xTemp, yTemp] = player;
                                distance--;
                                xTemp -= xDirection;
                                yTemp -= yDirection;
                            } while (distance > 0);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if neither player can play.
        /// Returns false otherwise.
        /// </summary>
        /// <returns></returns>
        public bool GameOver()
        {
            if (CanPlay(BLACK) == false || CanPlay(WHITE) == false)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Computes and returns white score.
        /// </summary>
        /// <returns></returns>
        public int GetWhiteScore()
        {
            return GetSquareIndices(WHITE).Count;
        }

        /// <summary>
        /// Computes and returns black score.
        /// </summary>
        /// <returns></returns>
        public int GetBlackScore()
        {
            return GetSquareIndices(BLACK).Count;
        }

        /// <summary>
        /// Return a new Board object with a board set to the starting state.
        /// </summary>
        /// <returns></returns>
        public static Board StartingBoard()
        {
            Board startingBoard = new Board();
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    //the funtion SetSquare doesn't work here because it checks for legal move.
                    startingBoard.board[x, y] = EMPTY;
                }
            }
            startingBoard.board[3, 3] = WHITE;
            startingBoard.board[3, 4] = BLACK;
            startingBoard.board[4, 3] = BLACK;
            startingBoard.board[4, 4] = WHITE;
            return startingBoard;
        }

        /// <summary>
        /// Set the board with the given board.
        /// </summary>
        /// <param name="SourceBoard"></param>
        public void SetBoard(int[,] SourceBoard)
        {
            board = SourceBoard;
        }

        /*############################################################################
        ##               Getter and Setter                                          ##
        ############################################################################*/

        /// <summary>
        /// Returns the value of the square at the given index.
        /// Returns INVALID if the square is outside the board.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetSquare(Tuple<int, int> index)
        {
            if (IsSquareValid(index) == true)
                return board[index.Item1, index.Item2];
            else
                return INVALID;
        }
        /// <summary>
        /// Redefine funtion of the function GetSquare(Tuple<int, int> index)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetSquare(int x, int y)
        {
            return GetSquare(new Tuple<int, int>(x, y));
        }

        /// <summary>
        /// Set a specifique square with the given player value.
        /// Return false if the move is not valid or not legal.
        /// </summary>
        /// <param name="index">index of the square</param>
        /// <param name="player">Value of the player</param>
        /// <returns></returns>
        public bool SetSquare(Tuple<int, int> index, int player)
        {
            if (IsSquareValid(index) && CanMove(index, player))
            {
                board[index.Item1, index.Item2] = player;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Redefine funtion of SetSquare(Tuple<int, int> index, int player).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool SetSquare(int x, int y, int player)
        {
            return SetSquare(new Tuple<int, int>(x, y), player);
        }

        /*############################################################################
        ##               Debug functions                                            ##
        ############################################################################*/

        /// <summary>
        /// Print the current board to the console.
        /// </summary>
        public void Print()
        {
            Console.Write(" ");
            for (int i = 1; i <= 8; i++)
                Console.Write(i);
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                Console.WriteLine();
                Console.Write(i + 1);
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    switch (board[j, i])
                    {
                        case BLACK:
                            Console.Write('b');
                            break;
                        case WHITE:
                            Console.Write('w');
                            break;
                        case EMPTY:
                            Console.Write('.');
                            break;
                        default:
                            Console.WriteLine("Error in printBoard");
                            break;
                    }
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Return a deepCopy of the current board.
        /// </summary>
        /// <returns></returns>
        public int[,] GetBoardCopy()
        {
            return board.Clone() as int[,];
        }

        /// <summary>
        /// Return a list of tuple that represent the index of the squares of the corresponding value.
        /// Default value returne all the indices of the board.
        /// </summary>
        /// <param name="value">Value of the corresponding square: EMPTY, WHITE, BLACK or 2 for all the squares.</param>
        /// <returns></returns>
        private List<Tuple<int, int>> GetSquareIndices(int value = 2)
        {
            if (!new[] { EMPTY, WHITE, BLACK, 2 }.Contains(value))
                throw new ArgumentException();
            List<Tuple<int, int>> indices = new List<Tuple<int, int>>();
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    if (GetSquare(x, y) == value || GetSquare(x, y) == 2)
                    {
                        indices.Add(new Tuple<int, int>(x, y));
                    }
                }
            }
            return indices;
        }

        /// <summary>
        /// Returns true if the square at the given index is within the board.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool IsSquareValid(Tuple<int, int> index)
        {
            if (index.Item1 >= 0 && index.Item1 < 8 && index.Item2 >= 0 && index.Item2 < 8)
                return true;
            else
                return false;
        }
    }
}
