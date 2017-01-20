using System;
using System.Diagnostics;

namespace Othello_logique
{
    class Program
    {
        static void Main(string[] args)
        {
            bool debugFlag = true;
            Engine engine = new Engine();

            if (debugFlag)
            {
                engine.NewGame();
                engine.board.Print();
                Debug.Assert(engine.isPlayable(2, 2, false) == false, "IsPlayable failed 2,2");
                Debug.Assert(engine.isPlayable(3, 3, false) == false, "IsPlayable failed 3,3");
                Debug.Assert(engine.isPlayable(2, 3, false) == true, "IsPlayable failed 2,3");
                Debug.Assert(engine.isPlayable(3, 2, false) == true, "IsPlayable failed 3,2");
                engine.SaveGame();
                engine.LoadGame();
                engine.board.Print();
                engine.playMove(3, 2, false);
                Console.WriteLine("{0} turn", engine.Player);
                engine.board.Print();
                engine.SaveGame();
                engine.LoadGame();
                Console.WriteLine("{0} turn", engine.Player);
                engine.board.Print();
                engine.Undo();
                Console.WriteLine("{0} turn", engine.Player);
                engine.board.Print();
                Console.ReadKey();
            }
        }
    }
}
