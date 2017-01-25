// Author: Nicolas Gonin
using System;

namespace Othello_logique
{
    [Serializable]
    public class SavableEngine
    {
        //properties
        public int Player { get; set; }
        public Decimal BlackTimer { get; set; }
        public Decimal WhiteTimer { get; set; }
        public int[] Board { get; set; }
        public int[][] BoardHistory { get; set; }
        public int[] PlayerHistory { get; set; }
    }
}
