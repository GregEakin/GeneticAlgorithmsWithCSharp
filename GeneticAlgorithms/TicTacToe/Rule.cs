/* File: Rule.cs
 *     from chapter 18 of _Genetic Algorithms with Python_
 *     writen by Clinton Sheppard
 *
 * Author: Greg Eakin <gregory.eakin@gmail.com>
 * Copyright (c) 2018 Greg Eakin
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
 * implied.  See the License for the specific language governing
 * permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.TicTacToe
{
    public abstract class Rule
    {
        public delegate bool FunctionDelegate(Square square);

        public delegate int[] ValueFromSquareDelegate(Square square);

        public string DescriptionPrefix { get; }
        public ContentType? ExpectedContent { get; }
        public int? Count { get; }

        protected Rule(string descriptionPrefix, ContentType? expectedContent = null, int? count = null)
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

        public abstract ISet<int> GetMatches(IDictionary<int, Square> board, Square[] squares);
    }

    public class RuleMetadata
    {
        public delegate Rule CreateDelegate(ContentType? a, int? b);

        public CreateDelegate Create { get; }
        public Tuple<ContentType?, int[]>[] Options { get; }
        public bool NeedsSpecificContent { get; }
        public bool NeedsSpecificCount { get; }

        public RuleMetadata(CreateDelegate create, Tuple<ContentType?, int[]>[] options = null,
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
                    rules.AddRange(optionCounts
                        .Select(count => Create(option, count))
                        .Where(gene => seen.Add(gene.ToString())));
                }
                else
                {
                    var gene = Create(option, null);
                    if (seen.Add(gene.ToString()))
                        rules.Add(gene);
                }
            }

            return rules;
        }
    }

    public class Noop : Rule
    {
        public Noop()
            : base("Noop")
        {
        }

        public override ISet<int> GetMatches(IDictionary<int, Square> board, Square[] squares)
        {
            return new HashSet<int>();
        }
    }

    public class ContentFilter : Rule
    {
        public ValueFromSquareDelegate ValueFromSquare { get; }

        protected ContentFilter(string descriptionPrefix, ContentType? expectedContent, int? count,
            ValueFromSquareDelegate valueFromSquare)
            : base(descriptionPrefix, expectedContent, count)
        {
            ValueFromSquare = valueFromSquare;
        }

        public override ISet<int> GetMatches(IDictionary<int, Square> board, Square[] squares)
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
        public FunctionDelegate Function { get; }

        protected LocationFilter(string expectedLocation, string containerDescription, FunctionDelegate func)
            : base($"is in {expectedLocation} {containerDescription}")
        {
            Function = func;
        }

        public override ISet<int> GetMatches(IDictionary<int, Square> board, Square[] squares)
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
        protected RowLocationFilter(string expectedLocation, FunctionDelegate func)
            : base(expectedLocation, "ROW", func)
        {
        }
    }

    public class ColumnLocationFilter : LocationFilter
    {
        protected ColumnLocationFilter(string expectedLocation, FunctionDelegate func)
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

        public override ISet<int> GetMatches(IDictionary<int, Square> board, Square[] squares)
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

    public class DiagonalOppositeFilter : Rule
    {
        public DiagonalOppositeFilter(ContentType? expectedContent)
            : base("DIAGONAL-OPPOSITE is", expectedContent)
        {
        }

        public override ISet<int> GetMatches(IDictionary<int, Square> board, Square[] squares)
        {
            var result = new HashSet<int>(squares
                .Where(square =>
                    square.DiagonalOpposite != null && board[(int) square.DiagonalOpposite].Content == ExpectedContent)
                .Select(square => square.Index));
            return result;
        }
    }

    public class RowOppositeFilter : Rule
    {
        public RowOppositeFilter(ContentType? expectedContent)
            : base("ROW-OPPOSITE is", expectedContent)
        {
        }

        public override ISet<int> GetMatches(IDictionary<int, Square> board, Square[] squares)
        {
            var result = new HashSet<int>(squares
                .Where(square =>
                    square.RowOpposite != null && board[(int) square.RowOpposite].Content == ExpectedContent)
                .Select(square => square.Index));
            return result;
        }
    }

    public class ColumnOppositeFilter : Rule
    {
        public ColumnOppositeFilter(ContentType? expectedContent)
            : base("COLUMN-OPPOSITE is", expectedContent)
        {
        }

        public override ISet<int> GetMatches(IDictionary<int, Square> board, Square[] squares)
        {
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

        public override ISet<int> GetMatches(IDictionary<int, Square> board, Square[] squares)
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

        public override ISet<int> GetMatches(IDictionary<int, Square> board, Square[] squares)
        {
            var result = new HashSet<int>(squares.Where(square => square.Corner).Select(square => square.Index));
            return result;
        }
    }

    public class SideFilter : Rule
    {
        public SideFilter()
            : base("is SIDE")
        {
        }

        public override ISet<int> GetMatches(IDictionary<int, Square> board, Square[] squares)
        {
            var result = new HashSet<int>(squares.Where(square => square.Side).Select(square => square.Index));
            return result;
        }
    }
}