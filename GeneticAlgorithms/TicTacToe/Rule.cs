using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.TicTacToe
{
    public abstract class Rule
    {
        public string DescriptionPrefix { get; }
        public ContentType? ExpectedContent { get; }
        public int? Count { get; }

        public Rule(string descriptionPrefix, ContentType? expectedContent = null, int? count = null)
        {
            DescriptionPrefix = descriptionPrefix;
            ExpectedContent = expectedContent;
            Count = count;
        }

        public override string ToString()
        {
            var a1 = Count != null ? Count + " " : string.Empty;
            var b1 = ExpectedContent != null ? ExpectedContent + " " : string.Empty;
            return $"{DescriptionPrefix} {a1}{b1}";
        }

        public abstract ISet<int> Matches(IDictionary<int, Square> board, Square[] squares);
    }

    public class RuleMetadata
    {
        public Func<ContentType?, int?, Rule> Create { get; }
        public Tuple<ContentType?, int[]>[] Options { get; }
        public bool NeedsSpecificContent { get; }
        public bool NeedsSpecificCount { get; }

        public RuleMetadata(Func<ContentType?, int?, Rule> create,
            Tuple<ContentType?, int[]>[] options = null,
            bool needsSpecificContent = true, bool needsSpecificCount = true)
        {
            if (needsSpecificCount && !needsSpecificContent)
                throw new ArgumentException("needsSpecificCount is only valid if needsSpecificContent is true");

            Create = create;
            Options = options;
            NeedsSpecificContent = options != null && needsSpecificContent;
            NeedsSpecificCount = options != null && needsSpecificCount;
        }

        public List<Rule> CreateRules()
        {
            if (!NeedsSpecificContent)
                return new List<Rule> {Create(null, null)};

            var seen = new HashSet<string>();
            var rules = new List<Rule>();

            foreach (var optionInfo in Options)
            {
                var option = optionInfo.Item1;
                if (NeedsSpecificCount)
                {
                    var optionCounts = optionInfo.Item2;

                    foreach (var count in optionCounts)
                    {
                        var gene = Create(option, count);
                        if (seen.Contains(gene.ToString()))
                            continue;

                        seen.Add(gene.ToString());
                        rules.Add(gene);
                    }
                }
                else
                {
                    var gene = Create(option, null);
                    if (seen.Contains(gene.ToString()))
                        continue;

                    seen.Add(gene.ToString());
                    rules.Add(gene);
                }
            }

            return rules;
        }
    }

    public class ContentFilter : Rule
    {
        public Func<Square, int[]> ValueFromSquare { get; }

        public ContentFilter(string descriptionPrefix, ContentType? expectedContent, int? count,
            Func<Square, int[]> valueFromSquare)
            : base(descriptionPrefix, expectedContent, count)
        {
            ValueFromSquare = valueFromSquare;
        }

        public override ISet<int> Matches(IDictionary<int, Square> board, Square[] squares)
        {
            var result = new HashSet<int>();
            foreach (var square in squares)
            {
                var m = ValueFromSquare(square).Select(i => board[i].Content).ToList();
                if (m.Count(c => c == ExpectedContent) == Count)
                    result.Add(square.Index);
            }

            return result;
        }
    }

    public class RowContentFilter : ContentFilter
    {
        public RowContentFilter(ContentType? expectedContent, int? expectedCount)
            : base("its ROW has", expectedContent, expectedCount, s => s.Row)
        {
        }
    }

    public class ColumnContentFilter : ContentFilter
    {
        public ColumnContentFilter(ContentType? expectedContent, int? expectedCount)
            : base("its COLUMN has", expectedContent, expectedCount, s => s.Column)
        {
        }
    }

    public class LocationFilter : Rule
    {
        public Func<Square, bool> Function { get; }

        public LocationFilter(string expectedLocation, string containerDescription, Func<Square, bool> func)
            : base($"is in {expectedLocation} {containerDescription}")
        {
            Function = func;
        }

        public override ISet<int> Matches(IDictionary<int, Square> board, Square[] squares)
        {
            // var results = squares.Where(square => Function(square)).Select(square => square.Index).Distinct().ToList();

            var results = new HashSet<int>();
            foreach (var square in squares)
                if (Function(square))
                    results.Add(square.Index);
            return results;
        }
    }

    public class RowLocationFilter : LocationFilter
    {
        public RowLocationFilter(string expectedLocation, Func<Square, bool> func)
            : base(expectedLocation, "ROW", func)
        {
        }
    }

    public class ColumnLocationFilter : LocationFilter
    {
        public ColumnLocationFilter(string expectedLocation, Func<Square, bool> func)
            : base(expectedLocation, "COLUMN", func)
        {
        }
    }

    public class TopRowFilter : RowLocationFilter
    {
        public TopRowFilter()
            : base("TOP", square => square.TopRow)
        {
        }
    }

    public class MiddleRowFilter : RowLocationFilter
    {
        public MiddleRowFilter()
            : base("MIDDLE", square => square.MiddleRow)
        {
        }
    }

    public class BottomRowFilter : RowLocationFilter
    {
        public BottomRowFilter()
            : base("BOTTOM", square => square.BottomRow)
        {
        }
    }

    public class LeftColumnFilter : ColumnLocationFilter
    {
        public LeftColumnFilter()
            : base("LEFT", square => square.LeftColumn)
        {
        }
    }

    public class MiddleColumnFilter : ColumnLocationFilter
    {
        public MiddleColumnFilter()
            : base("MIDDLE", square => square.MiddleColumn)
        {
        }
    }

    public class RightColumnFilter : ColumnLocationFilter
    {
        public RightColumnFilter()
            : base("RIGHT", square => square.RightColumn)
        {
        }
    }

    public class DiagonalLocationFilter : LocationFilter
    {
        public DiagonalLocationFilter()
            : base("DIAGONAL", "", square => !(square.MiddleRow || square.MiddleColumn) || square.Center)
        {
        }
    }

    public class DiagonalContentFilter : Rule
    {
        public DiagonalContentFilter(ContentType? expectedContent, int? count)
            : base("its DIAGONAL has", expectedContent, count)
        {
        }

        public override ISet<int> Matches(IDictionary<int, Square> board, Square[] squares)
        {
            var result = new HashSet<int>();
            foreach (var square in squares)
            foreach (var diagonal in square.Diagonals)
            {
                var m = diagonal.Select(i => board[i].Content);
                if (m.Count(i => i == ExpectedContent) == Count)
                    result.Add(square.Index);
            }

            return result;
        }
    }

    public class WinFilter : Rule
    {
        public Rule RowRule { get; }
        public Rule ColumnRule { get; }
        public Rule DiagonalRule { get; }

        public WinFilter(ContentType content)
            : base(content == ContentType.Mine ? "WIN" : "block OPPONENT WIN")
        {
            RowRule = new RowContentFilter(content, 2);
            ColumnRule = new ColumnContentFilter(content, 2);
            DiagonalRule = new DiagonalContentFilter(content, 2);
        }

        public override ISet<int> Matches(IDictionary<int, Square> board, Square[] squares)
        {
            var inDiagonal = DiagonalRule.Matches(board, squares);
            if (inDiagonal.Count > 0)
                return inDiagonal;

            var inRow = RowRule.Matches(board, squares);
            if (inRow.Count > 0)
                return inRow;

            var inColumn = ColumnRule.Matches(board, squares);
            return inColumn;
        }
    }

    public class DiagonalOppositeFilter : Rule
    {
        public DiagonalOppositeFilter(ContentType? expectedContent)
            : base("DIAGONAL-OPPOSITE is", expectedContent)
        {
        }

        public override ISet<int> Matches(IDictionary<int, Square> board, Square[] squares)
        {
            var result = new HashSet<int>();
            foreach (var square in squares)
            {
                if (square.DiagonalOpposite == null)
                    continue;

                if (board[(int) square.DiagonalOpposite].Content == ExpectedContent)
                    result.Add(square.Index);
            }

            return result;
        }
    }

    public class RowOppositeFilter : Rule
    {
        public RowOppositeFilter(ContentType? expectedContent)
            : base("ROW-OPPOSITE is", expectedContent)
        {
        }

        public override ISet<int> Matches(IDictionary<int, Square> board, Square[] squares)
        {
            var result = new HashSet<int>();
            foreach (var square in squares)
            {
                if (square.RowOpposite == null)
                    continue;

                if (board[(int) square.RowOpposite].Content == ExpectedContent)
                    result.Add(square.Index);
            }

            return result;
        }
    }

    public class ColumnOppositeFilter : Rule
    {
        public ColumnOppositeFilter(ContentType? expectedContent)
            : base("COLUMN-OPPOSITE is", expectedContent)
        {
        }

        public override ISet<int> Matches(IDictionary<int, Square> board, Square[] squares)
        {
            //var result = new HashSet<int>();
            //foreach (var square in squares)
            //{
            //    if (square.ColumnOpposite == null)
            //        continue;

            //    if (board[(int) square.ColumnOpposite].Content == ExpectedContent)
            //        result.Add(square.Index);
            //}

            var result = new HashSet<int>(squares
                .Where(square =>
                    square.ColumnOpposite != null && board[(int) square.ColumnOpposite].Content == ExpectedContent)
                .Select(square => square.Index));
            return result;
        }
    }

    public class CenterFilter : Rule
    {
        public CenterFilter()
            : base("is in CENTER")
        {
        }

        public override ISet<int> Matches(IDictionary<int, Square> board, Square[] squares)
        {
            var result = new HashSet<int>(squares.Where(square => square.Center).Select(square => square.Index));
            return result;
        }
    }

    public class CornerFilter : Rule
    {
        public CornerFilter()
            : base("is a CORNER")
        {
        }

        public override ISet<int> Matches(IDictionary<int, Square> board, Square[] squares)
        {
            var result = new HashSet<int>();
            foreach (var square in squares)
            {
                if (square.Corner)
                    result.Add(square.Index);
            }

            return result;
        }
    }

    public class SideFilter : Rule
    {
        public SideFilter()
            : base("is SIDE")
        {
        }

        public override ISet<int> Matches(IDictionary<int, Square> board, Square[] squares)
        {
            var result = new HashSet<int>();
            foreach (var square in squares)
            {
                if (square.Side)
                    result.Add(square.Index);
            }

            return result;
        }
    }
}