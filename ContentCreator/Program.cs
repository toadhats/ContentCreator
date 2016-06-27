using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentCreator
{
    internal class Program
    {
        private const string SourceFile = "source.txt";

        private static void Main(string[] args)
        {
            MarkovTextModel Model = new MarkovTextModel(5);
            Console.WriteLine("Building model...");
            var stopwatch = Stopwatch.StartNew();
            ReadHeadlines(Model, SourceFile);
            stopwatch.Stop();
            Console.WriteLine("Model built in {0}ms.", stopwatch.ElapsedMilliseconds);
            int iterations = 1;
            int modelOrder = 3;
            if (args.Length == 2) { modelOrder = Int32.Parse(args[1]); }
            if (args.Length > 0 && Int32.TryParse(args[0], out iterations))
            {
                Console.WriteLine("Generating {0} items", iterations);
                for (var i = 0; i < iterations; ++i)
                {
                    Console.WriteLine(Model.Generate(3));
                }
            }
            else
            {
                Console.WriteLine(Model.Generate(3));
            }

            Console.ReadLine(); // Press any key to exit lmao
        }

        private static void ReadHeadlines(MarkovTextModel model, string filename)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                string headline;
                while ((headline = sr.ReadLine()) != null)
                {
                    headline = headline.Trim();
                    if (headline.Length > 0)
                    {
                        model.AddString(headline);
                    }
                }
            }
        }
    }
}
