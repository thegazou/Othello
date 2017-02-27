using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloIA7
{
    public class Board : IPlayable.IPlayable
    { 
        //public constants
        public const int BOARD_SIZE = 8;
        public const int BLACK = 1;
        public const int WHITE = -1;
        public const int EMPTY = 0;
        public const int INVALID = -2;
        public static Node treeNode;

        public int[,] board = new int[BOARD_SIZE, BOARD_SIZE];
        public int this[int idx1, int idx2]
        {
            get { return board[idx1, idx2]; }
            set { board[idx1, idx2] = value;}
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
                                this[xTemp, yTemp] = player;
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
            if (CanPlay(BLACK) == false && CanPlay(WHITE) == false)
                return true;
            else
                return false;
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
            startingBoard[3, 3] = WHITE;
            startingBoard[3, 4] = BLACK;
            startingBoard[4, 3] = BLACK;
            startingBoard[4, 4] = WHITE;

            return startingBoard;
        }

        /// <summary>
        /// Set the board with the given board.
        /// </summary>
        /// <param name="SourceBoard"></param>
        public void SetBoard(int[,] SourceBoard)
        {
            foreach (Tuple<int, int> index in GetSquareIndices())
                SetSquare(index, SourceBoard[index.Item1, index.Item2], false);
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
        /// Return false if the move is not valid (or not legal).
        /// </summary>
        /// <param name="index">index of the square</param>
        /// <param name="player">Value of the player</param>
        /// <param name="checkForLegality">Set to false if you don't want to check for legality of the move.</param>
        /// <returns></returns>
        public bool SetSquare(Tuple<int, int> index, int player, bool checkForLegality = true)
        {
            if (IsSquareValid(index) || checkForLegality && CanMove(index, player))
            {
                this[index.Item1, index.Item2] = player;
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
                    if (GetSquare(x, y) == value || value == 2)
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

        public string GetName()
        {
            return "7: Chaperon_Gonin";
        }

        public bool IsPlayable(int column, int line, bool isWhite)
        {
            Tuple<int, int> index = new Tuple<int, int>(column, line);
            return CanMove(index, player);
        }

        public bool PlayMove(int column, int line, bool isWhite)
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

        public Tuple<int, int> GetNextMove(int[,] game, int level, bool whiteTurn)
        {
            AlphaBeta(game);
        }

        public int[,] GetBoard()
        {
            return GetBoardCopy();
        }


        public void AlphaBeta(Board board)
        {
            treeNode = new Node();
            AlphaBeta(board, difficulty, playerColor, int.MinValue, int.MaxValue, treeNode);
        }

        public int AlphaBeta(Board board, int depth, int currentPlayer, int alpha, int beta, Node node)
        {
            Node thisNode = new Node();
            node.AddChild(thisNode);

            // If the game is over, terminate the search
            if (board.GameOver())
            {
                int value = board.StaticEvaluator(this.playerColor);
                thisNode.Text = value.ToString();
                return value;
            }

            // At the bottom of the search space
            if (depth == 0)
            {
                int value = board.StaticEvaluator(this.playerColor);
                thisNode.Text = value.ToString();
                return value;
            }

            // If this player can't play, skip turn
            if (!board.CanPlay(currentPlayer))
            {
                int value = AlphaBeta(board, depth, -currentPlayer, alpha, beta, thisNode);
                thisNode.Text = "Skip";
                return value;
            }

            // Get the list of plausable moves, sorted by most pieces flipped
            List<Move> moves = board.MoveGenerator(currentPlayer);
            moves.Sort();


            // Find the best currentMove
            Move bestMove = Move.Empty;
            foreach (Move currentMove in moves)
            {
                // Copy the board
                Board currentBoard = board.Copy();
                // Make the selected move
                currentBoard.Move(currentMove.x, currentMove.y, currentPlayer);
                // Evaluate the board
                int currentValue = AlphaBeta(currentBoard, depth - 1, -currentPlayer, alpha, beta, thisNode);

                // MAX
                if (currentPlayer == this.playerColor)
                {
                    if (currentValue > alpha)
                    {
                        alpha = currentValue;
                        bestMove = currentMove;
                    }
                    // Prune?
                    if (alpha >= beta)
                    {
                        thisNode.Text += String.Format("MAX >= {0} ({1}/{2})", alpha, moves.IndexOf(currentMove) + 1, moves.Count);
                        return alpha;
                    }
                }
                // MIN
                else
                {
                    if (currentValue < beta)
                    {
                        beta = currentValue;
                        bestMove = currentMove;
                    }
                    // Prune?
                    if (alpha >= beta)
                    {
                        thisNode.Text += String.Format("MIN <= {0} ({1}/{2})", beta, moves.IndexOf(currentMove) + 1, moves.Count);
                        return beta;
                    }
                }
            }

            // Make the best move
            if (!bestMove.Equals(Move.Empty))
                board.Move(bestMove.x, bestMove.y, currentPlayer);

            // Return the value of the best move
            if (currentPlayer == this.playerColor)
            {
                thisNode.Text += String.Format("MAX = {0} ({1}/{2}) [{3}, {4}]", alpha, moves.Count, moves.Count, bestMove.x + 1, bestMove.y + 1);
                return alpha;
            }
            else
            {
                thisNode.Text += String.Format("MIN = {0} ({1}/{2}) [{3}, {4}]", beta, moves.Count, moves.Count, bestMove.x + 1, bestMove.y + 1);
                return beta;
            }
        }

        #region Constructeur

        public Board()
        {
            board = new int[BOARD_SIZE, BOARD_SIZE];
        }

        #endregion
    }
}
