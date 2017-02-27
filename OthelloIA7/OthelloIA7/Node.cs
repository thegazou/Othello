using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloIA7
{
    class Node
    {
        List<Node> Children;

        Tuple<int,int> Item { get; set; }

        public Node()
        {
            Item = null;
        }

        public Node(Tuple<int, int> item)
        {
            Item = item;
        }

        public Node AddChild(Tuple<int, int> item)
        {
            Node nodeItem = new Node(item);
            Children.Add(nodeItem);
            return nodeItem;
        }
    }
}
