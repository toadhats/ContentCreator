using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentCreator
{
    internal class MarkovTextModel
    {
        public const char StartChar = (char)0xFFFE;
        public const char EndChar = (char)0xFFFF;

        internal class MarkovNode
        {
            public char Ch;
            public int Count;
            public int FollowCount;
            public Dictionary<Char, MarkovNode> Children;

            public MarkovNode(char c)
            {
                Ch = c;
                Count = 1;
                FollowCount = 0;
                Children = new Dictionary<char, MarkovNode>();
            }

            public MarkovNode AddChild(char c)
            {
                if (Children == null)
                {
                    Children = new Dictionary<char, MarkovNode>();
                }
                ++FollowCount;
                MarkovNode child;
                if (Children.TryGetValue(c, out child))
                {
                    ++child.Count;
                }
                else
                {
                    child = new MarkovNode(c);
                    Children.Add(c, child);
                }
                return child;
            }
        }

        private MarkovNode Root;
        private int ModelOrder;

        public MarkovTextModel(int order)
        {
            ModelOrder = order;
            Root = new MarkovNode(StartChar);
        }

        public MarkovTextModel(string text, int order)
        {
            Root = new MarkovNode(StartChar);

            // Currently building only an order 0 model
            foreach (char c in text)
            {
                ++Root.Count;
                Root.AddChild(c);
            }

            // Add the stop character
            Root.AddChild(EndChar);
        }

        public void AddString(string s)
        {
            // Construct the string that will be added.
            StringBuilder sb = new StringBuilder(s.Length + 2 * (ModelOrder));
            // Order+1 Start characters. The string to add. Order+1 Stop characters.
            sb.Append(StartChar, ModelOrder);
            sb.Append(s);
            sb.Append(EndChar, ModelOrder);

            // Naive method
            for (int iStart = 0; iStart < sb.Length; ++iStart)
            {
                // Get the order 0 node
                MarkovNode parent = Root.AddChild(sb[iStart]);

                // Now add N-grams starting with this node
                for (int i = 1; i <= ModelOrder && i + iStart < sb.Length; ++i)
                {
                    MarkovNode child = parent.AddChild(sb[iStart + i]);
                    parent = child;
                }
            }
        }

        public string OutputModel()
        {
            return OutputNode(Root, 1, string.Empty);
        }

        private string OutputNode(MarkovNode node, int parentCount, string pad)
        {
            //Console.WriteLine("{0}{1} - {2:N0} ({3:P2})", pad, node.Ch, node.Count, (double)node.Count / parentCount);
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("{0}{1} - {2:N0} ({3:P2})",
                pad, node.Ch, node.Count, (double)node.Count / parentCount));
            if (node.Children != null)
            {
                int cnt = 0;
                foreach (var kvp in node.Children)
                {
                    sb.AppendLine(OutputNode(kvp.Value, node.Count, pad + "  "));
                    cnt += kvp.Value.Count;
                }
                if (cnt != node.Count)
                {
                    Console.WriteLine("ERROR: child count doesn't match.");
                }
            }
            return sb.ToString();
        }

        private Random RandomSelector = new Random();

        public string Generate(int order)
        {
            if (order > ModelOrder)
            {
                throw new ApplicationException("Cannot generate higher order than was built.");
            }
            StringBuilder rslt = new StringBuilder();
            rslt.Append(StartChar, order);
            int iStart = 0;
            char ch = StartChar;
            do
            {
                MarkovNode node = Root.Children[rslt[iStart]];
                for (int i = 1; i < order; ++i)
                {
                    node = node.Children[rslt[i + iStart]];
                }
                ch = SelectChildChar(node);
                if (ch != EndChar)
                    rslt.Append(ch);
                ++iStart;
            } while (ch != EndChar);

            // remove start characters from the string
            return rslt.ToString().Substring(order);
        }

        private char SelectChildChar(MarkovNode node)
        {
            // Generate a random number in the range 0..(node.Count-1)
            int rnd = RandomSelector.Next(node.Count);

            // Go through the children to select the node
            int cnt = 0;
            foreach (var kvp in node.Children)
            {
                cnt += kvp.Value.Count;
                if (cnt > rnd)
                {
                    return kvp.Key;
                }
            }
            throw new ApplicationException("This can't happen!");
        }
    }
}
