// Author: Nicolas Gonin
using System;

namespace Othello_logique
{
    [Serializable]
    public class TcpObject
    {
        public int[] Move { get; set; }

        public TcpObject(int x, int y)
        {
            this.Move = new int[] { x, y };
        }
    }
}
