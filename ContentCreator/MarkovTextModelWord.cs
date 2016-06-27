using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentCreator
{
    /*
    This class doesn't work, because the 'StartChar' implementation doesn't translate from 
    working with chars to working with strings/words. It's not strictly necessary to achieve
    the desired effect, but when generating paragraphs it does help to ensure that nonsense
    words aren't generated - which may or may not be desired.
    */
    internal class MarkovTextModelWord
    {
        public const char StartChar = '^';
        public const char EndChar = '$';

        internal class MarkovNode
        {
            public string word;
            public int Count;
            public int FollowCount;
            public Dictionary<string, MarkovNode> Children;

            public MarkovNode(string w)
            {
                word = w;
                Count = 1;
                FollowCount = 0;
                Children = new Dictionary<string, MarkovNode>();
            }

            public MarkovNode AddChild(string w)
            {
                if (Children == null)
                {
                    Children = new Dictionary<string, MarkovNode>();
                }
                ++FollowCount;
                MarkovNode child;
                if (Children.TryGetValue(w, out child))
                {
                    ++child.Count;
                }
                else
                {
                    child = new MarkovNode(w);
                    Children.Add(w, child);
                }
                return child;
            }
        }

        private MarkovNode Root;
        private int ModelOrder;

        public MarkovTextModelWord(int order)
        {
            ModelOrder = order;
            Root = new MarkovNode(StartChar.ToString());
        }

        public void AddString(string s)
        {
            var input = s.Split(' ');

            // Naive method
            for (int iStart = 0; iStart < input.Length; ++iStart)
            {
                // Get the order 0 node
                MarkovNode parent = Root.AddChild(input[iStart]);

                // Now add N-grams starting with this node
                for (int i = 1; i <= ModelOrder && i + iStart < input.Length; ++i)
                {
                    MarkovNode child = parent.AddChild(input[iStart + i]);
                    parent = child;
                    if (parent.word.Last() == '.')
                    {
                        parent.AddChild(EndChar.ToString());
                    }
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
                pad, node.word, node.Count, (double)node.Count / parentCount));
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
            List<String> rslt = new List<string>();
            // Trying to add one start char per order 'level', have to do it in a loop because we
            // don't have a method that adds a given number like we do with StringBuilder.Append()
            for (var i = 0; i < order; ++i)
            {
                rslt.Add(StartChar.ToString());
            }

            int iStart = 0;
            string w = StartChar.ToString();
            do
            { //TODO: Fix this garbage
                MarkovNode node = Root.Children[rslt[iStart]];
                for (int i = 1; i < order; ++i)
                {
                    node = node.Children[rslt[i + iStart]];
                }
                w = SelectChildWord(node);
                if (w.Equals(EndChar.ToString()))
                    rslt.Add(" " + w);
                ++iStart;
            } while (!w.Equals(EndChar.ToString()));

            // remove start characters from the string
            return rslt.ToString().Substring(order);
        }

        private string SelectChildWord(MarkovNode node)
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
