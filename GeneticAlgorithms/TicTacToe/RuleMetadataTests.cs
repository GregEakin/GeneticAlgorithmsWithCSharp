﻿/* File: RuleMetadataTests.cs
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

using GeneticAlgorithms.Utilities;

namespace GeneticAlgorithms.TicTacToe;

[TestClass]
public class RuleMetadataTests
{
    private readonly Tuple<ContentType?, int[]>[] _options =
    [
        new(ContentType.Opponent, [0, 1, 2]),
        new(ContentType.Mine, [0, 1, 2])
    ];

    private readonly RuleMetadata.CreateDelegate _createDelegate =
        (expectedContent, count) => new RowContentFilter(expectedContent, count);

    [TestMethod]
    public void DefaultCtorTest()
    {
        var ruleMetadata = new RuleMetadata(_createDelegate);

        Assert.AreSame(_createDelegate, ruleMetadata.Create);
        Assert.IsNull(ruleMetadata.Options);
        Assert.IsFalse(ruleMetadata.NeedsSpecificContent);
        Assert.IsFalse(ruleMetadata.NeedsSpecificCount);
    }

    [TestMethod]
    public void ContentCtorTest()
    {
        var ruleMetadata = new RuleMetadata(_createDelegate, _options, false, false);

        Assert.AreSame(_createDelegate, ruleMetadata.Create);
        Assert.AreSame(_options, ruleMetadata.Options);
        Assert.IsFalse(ruleMetadata.NeedsSpecificContent);
        Assert.IsFalse(ruleMetadata.NeedsSpecificCount);
    }

    [TestMethod]
    public void InvalidCtorTest()
    {
        ExpectedException.AssertThrows<ArgumentException>(() =>
        {
            var _ = new RuleMetadata(_createDelegate, _options, false);
        });
    }

    [TestMethod]
    public void CountCtorTest()
    {
        var ruleMetadata = new RuleMetadata(_createDelegate, _options, true, false);

        Assert.AreSame(_createDelegate, ruleMetadata.Create);
        Assert.AreSame(_options, ruleMetadata.Options);
        Assert.IsTrue(ruleMetadata.NeedsSpecificContent);
        Assert.IsFalse(ruleMetadata.NeedsSpecificCount);
    }

    [TestMethod]
    public void OptionsCtorTest()
    {
        var ruleMetadata = new RuleMetadata(_createDelegate, _options);

        Assert.AreSame(_createDelegate, ruleMetadata.Create);
        Assert.AreSame(_options, ruleMetadata.Options);
        Assert.IsTrue(ruleMetadata.NeedsSpecificContent);
        Assert.IsTrue(ruleMetadata.NeedsSpecificCount);
    }

    [TestMethod]
    public void CreateRulesWithoutSpecificContentTest()
    {
        var ruleMetadata = new RuleMetadata(_createDelegate);
        Assert.IsFalse(ruleMetadata.NeedsSpecificContent);
        var rules = ruleMetadata.CreateRules();

        Assert.AreEqual(1, rules.Count);
        Assert.IsInstanceOfType(rules[0], typeof(RowContentFilter));
        var filter = (RowContentFilter) rules[0];
        Assert.IsNull(filter.ExpectedContent);
        Assert.IsNull(filter.Count);
    }

    [TestMethod]
    public void CreateRulesWithSpecificContentWithoutSpecificCountTest()
    {
        var ruleMetadata = new RuleMetadata(_createDelegate, _options, true, false);
        Assert.IsTrue(ruleMetadata.NeedsSpecificContent);
        Assert.IsFalse(ruleMetadata.NeedsSpecificCount);
        var rules = ruleMetadata.CreateRules();

        Assert.AreEqual(2, rules.Count);
        Assert.IsInstanceOfType(rules[0], typeof(RowContentFilter));
        var filter0 = (RowContentFilter) rules[0];
        Assert.AreEqual(_options[0].Item1, filter0.ExpectedContent);
        Assert.IsNull(filter0.Count);

        Assert.IsInstanceOfType(rules[1], typeof(RowContentFilter));
        var filter1 = (RowContentFilter) rules[1];
        Assert.AreEqual(_options[1].Item1, filter1.ExpectedContent);
        Assert.IsNull(filter1.Count);
    }

    [TestMethod]
    public void CreateRulesWithSpecificContentWithSpecificCountTest()
    {
        var ruleMetadata = new RuleMetadata(_createDelegate, _options);
        Assert.IsTrue(ruleMetadata.NeedsSpecificContent);
        Assert.IsTrue(ruleMetadata.NeedsSpecificCount);
        var rules = ruleMetadata.CreateRules();

        Assert.AreEqual(6, rules.Count);
        var index = -1;
        foreach (var filter in rules.Cast<RowContentFilter>())
        {
            index++;
            Assert.AreEqual(_options[index / 3].Item1, filter.ExpectedContent);
            Assert.AreEqual(index % 3, filter.Count);
        }
    }
}