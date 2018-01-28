using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.TicTacToe
{
    [TestClass]
    public class RuleTests
    {
        [TestMethod]
        public void Test1()
        {
            var options = new[]
            {
                new Tuple<ContentType?, int[]>(ContentType.Opponent, new[] {0, 1, 2}),
                new Tuple<ContentType?, int[]>(ContentType.Mine, new[] {0, 1, 2}),
            };

            var geneSet = new[]
            {
                new RuleMetadata((expectedContent, count) => new RowContentFilter(expectedContent, count), options),
                new RuleMetadata((expectedContent, count) => new TopRowFilter(), options),
                new RuleMetadata((expectedContent, count) => new MiddleRowFilter(), options),
                new RuleMetadata((expectedContent, count) => new BottomRowFilter(), options),
                new RuleMetadata((expectedContent, count) => new ColumnContentFilter(expectedContent, count), options),
                new RuleMetadata((expectedContent, count) => new LeftColumnFilter(), options),
                new RuleMetadata((expectedContent, count) => new MiddleColumnFilter(), options),
                new RuleMetadata((expectedContent, count) => new RightColumnFilter(), options),
                new RuleMetadata((expectedContent, count) => new DiagonalContentFilter(expectedContent, count),
                    options),
                new RuleMetadata((expectedContent, count) => new DiagonalLocationFilter(), options),
                new RuleMetadata((expectedContent, count) => new CornerFilter()),
                new RuleMetadata((expectedContent, count) => new SideFilter()),
                new RuleMetadata((expectedContent, count) => new CenterFilter()),
                new RuleMetadata((expectedContent, count) => new RowOppositeFilter(expectedContent), options),
                new RuleMetadata((expectedContent, count) => new ColumnOppositeFilter(expectedContent), options),
                new RuleMetadata((expectedContent, count) => new DiagonalOppositeFilter(expectedContent), options),
            };

            var genes = geneSet.SelectMany(g => g.CreateRules());
            Assert.AreEqual(34, genes.Count());

        }
    }
}