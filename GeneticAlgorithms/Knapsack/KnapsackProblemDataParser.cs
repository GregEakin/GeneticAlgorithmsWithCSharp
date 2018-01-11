using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeneticAlgorithms.Knapsack
{
    public class KnapsackProblemDataParser
    {
        public List<Resource> Resources { get;  } = new List<Resource>();

        public int MaxWeight { get; set; }

        public List<ItemQuantity> Solution { get;  }=new List<ItemQuantity>();

        public delegate FindMethod FindMethod(string line, KnapsackProblemDataParser data);

        public static KnapsackProblemData LoadData(string filename)
        {
            var lines = File.ReadAllLines(filename);
            var data = new KnapsackProblemDataParser();
            FindMethod f = FindConstraint;
            foreach (var line in lines)
            {
                f = f(line.Trim(), data);
                if (f == null)
                    break;
            }

            return new KnapsackProblemData(data.Resources.ToArray(), data.MaxWeight, data.Solution.ToArray());
        }

        public static FindMethod FindConstraint(string line, KnapsackProblemDataParser data)
        {
            var parts = line.Split(' ');
            if (parts[0] != "c:")
                return FindConstraint;

            data.MaxWeight = int.Parse(parts[1]);
            return FindDataStart;
        }

        public static FindMethod FindDataStart(string line, KnapsackProblemDataParser data)
        {
            if (line != "begin data")
                return FindDataStart;

            return ReadResourceOrFindDataEnd;
        }

        public static FindMethod ReadResourceOrFindDataEnd(string line, KnapsackProblemDataParser data)
        {
            if (line == "end data")
                return FindSolutionStart;
            var parts = line.Split('\t');
            var resouce = new Resource("R" + (data.Resources.Count + 1), int.Parse(parts[1]), int.Parse(parts[0]), 0);
            data.Resources.Add(resouce);
            return ReadResourceOrFindDataEnd;
        }

        public static FindMethod FindSolutionStart(string line, KnapsackProblemDataParser data)
        {
            if (line == "sol:")
                return ReadSolutionResourceOrFindSolutionEnd;
            return FindSolutionStart;
        }

        public static FindMethod ReadSolutionResourceOrFindSolutionEnd(string line, KnapsackProblemDataParser data)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            var parts = line.Split('\t').Where(s => !string.IsNullOrWhiteSpace(s)).Select(p => p).ToArray();
            var resourceIndex = int.Parse(parts[0]) - 1;
            var resourceQuantity = int.Parse(parts[1]);
            var item = new ItemQuantity(data.Resources[resourceIndex], resourceQuantity);
            data.Solution.Add(item);
            return ReadSolutionResourceOrFindSolutionEnd;
        }
    }
}