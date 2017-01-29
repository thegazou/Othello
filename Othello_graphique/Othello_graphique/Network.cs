using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Othello_logique
{
    class Network
    {
        /// <summary>
        /// Get the move from the given endpoint.
        /// </summary>
        /// <param name="myPort"></param>
        /// <param name="myIp"></param>
        /// <returns>Return an array with the move in the form [x,y].</returns>
        public static int[] GetInput(int myPort = 8001, string myIp = "0")
        {
            IPAddress serverIp;
            if (myIp.Equals("0")== true)
                serverIp = System.Net.IPAddress.Parse(GetLocalIPAddress());
            else
                serverIp = System.Net.IPAddress.Parse(myIp);
            TcpListener server = new TcpListener(serverIp, 8001);
            server.Start();
            //Console.WriteLine("The local End point is  :" + server.LocalEndpoint);
            Console.WriteLine("Waiting for a connection.....");
            TcpClient client = server.AcceptTcpClient();
            NetworkStream strm = client.GetStream();
            IFormatter formatter = new BinaryFormatter();

            TcpObject input = (TcpObject)formatter.Deserialize(strm);
            strm.Close();
            client.Close();
            server.Stop();
            return input.Move;
        }

        /// <summary>
        /// Send the given move to the given endpoint.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="destIp"></param>
        /// <param name="destPort"></param>
        public static void SendInput(int x, int y, string destIp= "169.254.84.22", int destPort=8001)
        {
            bool isConnected = false;
            TcpObject input = new TcpObject(x, y);
            TcpClient client = new TcpClient();
            while (isConnected == false)
            {
                try
                {
                    client.Connect(destIp, destPort);
                    isConnected = true;
                }
                catch (System.Net.Sockets.SocketException){ }
            }
            IFormatter formatter = new BinaryFormatter();
            NetworkStream strm = client.GetStream();
            formatter.Serialize(strm, input);

            strm.Close();
            client.Close();
        }

        /// <summary>
        /// Get the local ip address of the host.
        /// </summary>
        /// <returns></returns>
        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
    }
}