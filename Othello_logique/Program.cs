using System;
using System.Diagnostics;

namespace Othello_logique
{
    class Program
    {
        static void Main(string[] args)
        {
            bool debugFlag = false;
            Engine engine = new Engine();
            testTcp();

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
                engine.playMove(3, 2, Engine.BLACK);
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
        static void testTcp()
        {
            int isServer;
            int port = 8001;
            string ip = "169.254.84.22";
            Engine engine = new Engine();
            engine.NewGame();
            Console.Write("Are you a server? [y/*]");
            isServer = Console.Read();
            if (isServer == 121)
            {
                engine.NewOnlineGame(ip, port, Engine.BLACK);
                engine.playMove(3, 2, Engine.BLACK);
                Console.WriteLine("{0} turn", engine.Player);
                engine.board.Print();
            }
            else
            {
                engine.NewOnlineGame(ip, port, Engine.WHITE);
                Console.WriteLine("{0} turn", engine.Player);
                engine.board.Print();
                engine.playMove(2, 4, Engine.WHITE);
                Console.WriteLine("{0} turn", engine.Player);
                engine.board.Print();
            }
            Console.ReadKey();

        }
    }
}
