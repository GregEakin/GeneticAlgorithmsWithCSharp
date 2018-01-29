using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GeneticAlgorithms.TicTacToe
{
    [TestClass]
    public class RuleTests
    {
        [TestMethod]
        public void CreateGeneTest()
        {
            var genes = TicTacToeTests.CreateGeneSet();
            Assert.AreEqual(34, genes.Length);
        }

        [TestMethod]
        public void RowContentFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            board[1] = new Square(board[1].Index, ContentType.Mine);
            board[5] = new Square(board[5].Index, ContentType.Opponent);
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new RowContentFilter(ContentType.Empty, 3);
            //Assert.AreEqual("its ROW has", rule.DescriptionPrefix);
            //Assert.AreEqual(ContentType.Empty, rule.ExpectedContent);
            //Assert.AreEqual(3, rule.Count);
            Assert.AreEqual("its ROW has 3 Empty ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {7, 8, 9}, matches.ToArray());
        }

        [TestMethod]
        public void TopRowFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new TopRowFilter();
            Assert.AreEqual("is in TOP ROW ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {1, 2, 3}, matches.ToArray());
        }

        [TestMethod]
        public void MiddleRowFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new MiddleRowFilter();
            Assert.AreEqual("is in MIDDLE ROW ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {4, 5, 6}, matches.ToArray());
        }

        [TestMethod]
        public void BottomRowFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new BottomRowFilter();
            Assert.AreEqual("is in BOTTOM ROW ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {7, 8, 9}, matches.ToArray());
        }

        [TestMethod]
        public void ColumnContentFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            board[1] = new Square(board[1].Index, ContentType.Mine);
            board[5] = new Square(board[5].Index, ContentType.Opponent);
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new ColumnContentFilter(ContentType.Empty, 3);
            Assert.AreEqual("its COLUMN has 3 Empty ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {3, 6, 9}, matches.ToArray());
        }

        [TestMethod]
        public void LeftColumnFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new LeftColumnFilter();
            Assert.AreEqual("is in LEFT COLUMN ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {1, 4, 7}, matches.ToArray());
        }

        [TestMethod]
        public void MiddleColumnFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new MiddleColumnFilter();
            Assert.AreEqual("is in MIDDLE COLUMN ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {2, 5, 8}, matches.ToArray());
        }

        [TestMethod]
        public void RightCoilumnFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new RightColumnFilter();
            Assert.AreEqual("is in RIGHT COLUMN ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {3, 6, 9}, matches.ToArray());
        }

        [TestMethod]
        public void DiagonalContentFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            board[1] = new Square(board[1].Index, ContentType.Mine);
            board[2] = new Square(board[2].Index, ContentType.Opponent);
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new DiagonalContentFilter(ContentType.Empty, 3);
            Assert.AreEqual("its DIAGONAL has 3 Empty ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {3, 5, 7}, matches.ToArray());
        }

        [TestMethod]
        public void DiagonalLocationFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            board[1] = new Square(board[1].Index, ContentType.Mine);
            board[2] = new Square(board[2].Index, ContentType.Opponent);
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new DiagonalLocationFilter();
            Assert.AreEqual("is in DIAGONAL  ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {3, 5, 7, 9}, matches.ToArray());
        }

        [TestMethod]
        public void CornerFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new CornerFilter();
            Assert.AreEqual("is a CORNER ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {1, 3, 7, 9}, matches.ToArray());
        }

        [TestMethod]
        public void SideFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new SideFilter();
            Assert.AreEqual("is SIDE ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {2, 4, 6, 8}, matches.ToArray());
        }

        [TestMethod]
        public void CenterFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new CenterFilter();
            Assert.AreEqual("is in CENTER ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] {5}, matches.ToArray());
        }

        [TestMethod]
        public void RowOppositeFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            board[1] = new Square(board[1].Index, ContentType.Mine);
            board[5] = new Square(board[5].Index, ContentType.Opponent);
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new RowOppositeFilter(ContentType.Mine);
            Assert.AreEqual("ROW-OPPOSITE is Mine ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] { 3 }, matches.ToArray());
        }

        [TestMethod]
        public void ColumnOppositeFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            board[1] = new Square(board[1].Index, ContentType.Mine);
            board[5] = new Square(board[5].Index, ContentType.Opponent);
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new ColumnOppositeFilter(ContentType.Mine);
            Assert.AreEqual("COLUMN-OPPOSITE is Mine ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] { 7 }, matches.ToArray());
        }

        [TestMethod]
        public void DiagonalOppositeFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            board[1] = new Square(board[1].Index, ContentType.Mine);
            board[5] = new Square(board[5].Index, ContentType.Opponent);
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new DiagonalOppositeFilter(ContentType.Mine);
            Assert.AreEqual("DIAGONAL-OPPOSITE is Mine ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] { 9 }, matches.ToArray());
        }

        [TestMethod]
        public void WinFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            board[1] = new Square(board[1].Index, ContentType.Mine);
            board[5] = new Square(board[5].Index, ContentType.Mine);
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new WinFilter(ContentType.Mine);
            Assert.AreEqual("WIN ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] { 9 }, matches.ToArray());
        }

        [TestMethod]
        public void BlockFilterTest()
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            board[1] = new Square(board[1].Index, ContentType.Opponent);
            board[5] = new Square(board[5].Index, ContentType.Opponent);
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var rule = new WinFilter(ContentType.Opponent);
            Assert.AreEqual("block OPPONENT WIN ", rule.ToString());

            var matches = rule.Matches(board, empties);
            CollectionAssert.AreEqual(new[] { 9 }, matches.ToArray());
        }
    }
}