// Author: Nicolas Gonin
using System;

namespace Othello_logique
{
    class IA
    {
        //public constants
        public const int BLACK = 1;
        public const int WHITE = -1;
        public const int EMPTY = 0;
        public const int BOARD_SIZE = 8;

        /// <summary>
        /// Return the best move with the Strategie1 at the given game state.
        /// This is a test function.
        /// </summary>
        /// <param name="game">a 2D board with -1 for white 1 for black and 0 for empty tiles. First index for the column, second index for the line</param>
        /// <param name="player">1 for black and -1 for white player</param>
        /// <returns>Return the tuple (2,2)</returns>
        public static Tuple<int, int> Strategie1(int[,] game, int player)
        {
            return new Tuple<int, int>(2, 2);
        }

        /// <summary>
        /// Return the best move with the Strategie2 at the given game state.
        /// This function is not implemented yet.
        /// </summary>
        /// <param name="game">a 2D board with -1 for white 1 for black and 0 for empty tiles. First index for the column, second index for the line</param>
        /// <param name="player">1 for black and -1 for white player</param>
        /// <returns></returns>
        public static Tuple<int, int> Strategie2(int[,] game, int player)
        {
            throw new NotImplementedException();
        }
    }
}
