using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
