﻿/* File: RuleTests.cs
 *     from chapter 18 of _Genetic Algorithms with Python_
 *     written by Clinton Sheppard
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

namespace GeneticAlgorithms.TicTacToe;

[TestClass]
public class RuleTests
{
    private Dictionary<int, Square> _board;

    [TestInitialize]
    public void SetupTest()
    {
        _board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
    }

    [TestMethod]
    public void CreateGeneTest()
    {
        var genes = TicTacToeTests.CreateGeneSet();
        Assert.AreEqual(35, genes.Length);
    }

    [TestMethod]
    public void RowContentFilterTest()
    {
        _board[8] = new Square(_board[8].Index, ContentType.Mine);
        _board[5] = new Square(_board[5].Index, ContentType.Opponent);
        var rule = new RowContentFilter(ContentType.Empty, 2);
        //Assert.AreEqual("its ROW has", rule.DescriptionPrefix);
        //Assert.AreEqual(ContentType.Empty, rule.ExpectedContent);
        //Assert.AreEqual(3, rule.Count);
        Assert.AreEqual("its ROW has 2 Empty ", rule.ToString());

        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();
        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {1, 3, 4, 9}, matches.ToArray());
    }

    [TestMethod]
    public void TopRowFilterTest()
    {
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new TopRowFilter();
        Assert.AreEqual("is in TOP ROW ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {8, 3, 4}, matches.ToArray());
    }

    [TestMethod]
    public void MiddleRowFilterTest()
    {
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new MiddleRowFilter();
        Assert.AreEqual("is in MIDDLE ROW ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {1, 5, 9}, matches.ToArray());
    }

    [TestMethod]
    public void BottomRowFilterTest()
    {
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new BottomRowFilter();
        Assert.AreEqual("is in BOTTOM ROW ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {2, 6, 7}, matches.ToArray());
    }

    [TestMethod]
    public void ColumnContentFilterTest()
    {
        _board[8] = new Square(_board[8].Index, ContentType.Mine);
        _board[5] = new Square(_board[5].Index, ContentType.Opponent);
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new ColumnContentFilter(ContentType.Empty, 2);
        Assert.AreEqual("its COLUMN has 2 Empty ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {1, 3, 6, 7}, matches.ToArray());
    }

    [TestMethod]
    public void LeftColumnFilterTest()
    {
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new LeftColumnFilter();
        Assert.AreEqual("is in LEFT COLUMN ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {1, 6, 8}, matches.ToArray());
    }

    [TestMethod]
    public void MiddleColumnFilterTest()
    {
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new MiddleColumnFilter();
        Assert.AreEqual("is in MIDDLE COLUMN ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {3, 5, 7}, matches.ToArray());
    }

    [TestMethod]
    public void RightColumnFilterTest()
    {
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new RightColumnFilter();
        Assert.AreEqual("is in RIGHT COLUMN ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {2, 4, 9}, matches.ToArray());
    }

    [TestMethod]
    public void DiagonalContentFilterTest()
    {
        _board[8] = new Square(_board[8].Index, ContentType.Mine);
        _board[5] = new Square(_board[5].Index, ContentType.Opponent);
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new DiagonalContentFilter(ContentType.Empty, 2);
        Assert.AreEqual("its DIAGONAL has 2 Empty ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {4, 6}, matches.ToArray());
    }

    [TestMethod]
    public void DiagonalLocationFilterTest()
    {
        _board[8] = new Square(_board[8].Index, ContentType.Mine);
        _board[3] = new Square(_board[3].Index, ContentType.Opponent);
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new DiagonalLocationFilter();
        Assert.AreEqual("is in DIAGONAL  ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {2, 4, 5, 6}, matches.ToArray());
    }

    [TestMethod]
    public void CornerFilterTest()
    {
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new CornerFilter();
        Assert.AreEqual("is a CORNER ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {2, 4, 6, 8}, matches.ToArray());
    }

    [TestMethod]
    public void SideFilterTest()
    {
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new SideFilter();
        Assert.AreEqual("is SIDE ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {1, 3, 9, 7}, matches.ToArray());
    }

    [TestMethod]
    public void CenterFilterTest()
    {
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new CenterFilter();
        Assert.AreEqual("is in CENTER ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {5}, matches.ToArray());
    }

    [TestMethod]
    public void RowOppositeFilterTest()
    {
        _board[8] = new Square(_board[8].Index, ContentType.Mine);
        _board[5] = new Square(_board[5].Index, ContentType.Opponent);
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new RowOppositeFilter(ContentType.Mine);
        Assert.AreEqual("ROW-OPPOSITE is Mine ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {4}, matches.ToArray());
    }

    [TestMethod]
    public void ColumnOppositeFilterTest()
    {
        _board[8] = new Square(_board[8].Index, ContentType.Mine);
        _board[5] = new Square(_board[5].Index, ContentType.Opponent);
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new ColumnOppositeFilter(ContentType.Mine);
        Assert.AreEqual("COLUMN-OPPOSITE is Mine ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {6}, matches.ToArray());
    }

    [TestMethod]
    public void DiagonalOppositeFilterTest()
    {
        _board[8] = new Square(_board[8].Index, ContentType.Mine);
        _board[5] = new Square(_board[5].Index, ContentType.Opponent);
        var empties = _board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

        var rule = new DiagonalOppositeFilter(ContentType.Mine);
        Assert.AreEqual("DIAGONAL-OPPOSITE is Mine ", rule.ToString());

        var matches = rule.GetMatches(_board, empties);
        CollectionAssert.AreEquivalent(new[] {2}, matches.ToArray());
    }
}