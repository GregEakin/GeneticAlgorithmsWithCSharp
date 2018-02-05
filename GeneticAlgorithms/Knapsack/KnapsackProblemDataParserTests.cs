/* File: KnapsackProblemDataParserTests.cs
 *     from chapter 9 of _Genetic Algorithms with Python_
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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.Knapsack
{
    [TestClass]
    public class KnapsackProblemDataParserTests
    {
        [TestMethod]
        public void LoadDataTest()
        {
            var problemInfo = KnapsackProblemDataParser.LoadData(@"..\..\Data\exnsd16.ukp");
            Assert.AreEqual(889304, problemInfo.MaxWeight);
            Assert.AreEqual(2000, problemInfo.Resources.Length);
            Assert.AreEqual(2, problemInfo.Solution.Length);

            // nb    idx   w        p
            // 156 x R288, 5687, 0, 6585
            //   1 x R987, 2131, 0, 2420

            var iq1 = new ItemQuantity(problemInfo.Resources[288-1], 156);
            var iq2 = new ItemQuantity(problemInfo.Resources[987-1], 1);
            CollectionAssert.AreEquivalent(new[] {iq1, iq2}, problemInfo.Solution);
        }

        [TestMethod]
        public void FindConstraintTest1()
        {
            var data = new KnapsackProblemDataParser();
            var constraint = KnapsackProblemDataParser.FindConstraint("c: 123", data);
            Assert.AreEqual(KnapsackProblemDataParser.FindDataStart, constraint);
            Assert.AreEqual(123, data.MaxWeight);
        }

        [TestMethod]
        public void FindConstraintTest2()
        {
            var data = new KnapsackProblemDataParser();
            var constraint = KnapsackProblemDataParser.FindConstraint("Something else", data);
            Assert.AreEqual(KnapsackProblemDataParser.FindConstraint, constraint);
        }

        [TestMethod]
        public void FindDataStartTest1()
        {
            var data = new KnapsackProblemDataParser();
            var constraint = KnapsackProblemDataParser.FindDataStart("begin data", data);
            Assert.AreEqual(KnapsackProblemDataParser.ReadResourceOrFindDataEnd, constraint);
        }

        [TestMethod]
        public void FindDataStartTest2()
        {
            var data = new KnapsackProblemDataParser();
            var constraint = KnapsackProblemDataParser.FindDataStart("Something else", data);
            Assert.AreEqual(KnapsackProblemDataParser.FindDataStart, constraint);
        }

        [TestMethod]
        public void ReadResourceOrFindDataEndTest1()
        {
            var data = new KnapsackProblemDataParser();
            var constraint = KnapsackProblemDataParser.ReadResourceOrFindDataEnd("end data", data);
            Assert.AreEqual(KnapsackProblemDataParser.FindSolutionStart, constraint);
        }

        [TestMethod]
        public void ReadResourceOrFindDataEndTest2()
        {
            var data = new KnapsackProblemDataParser();
            var constraint = KnapsackProblemDataParser.ReadResourceOrFindDataEnd("3369\t3712", data);
            Assert.AreEqual(KnapsackProblemDataParser.ReadResourceOrFindDataEnd, constraint);
            Assert.AreEqual("R1", data.Resources[0].Name);
            Assert.AreEqual(3712, data.Resources[0].Value);
            Assert.AreEqual(0, data.Resources[0].Volume);
            Assert.AreEqual(3369, data.Resources[0].Weight);
        }

        [TestMethod]
        public void FindSolutionStartTest1()
        {
            var data = new KnapsackProblemDataParser();
            var constraint = KnapsackProblemDataParser.FindSolutionStart("sol:", data);
            Assert.AreEqual(KnapsackProblemDataParser.ReadSolutionResourceOrFindSolutionEnd, constraint);
        }

        [TestMethod]
        public void FindSolutionStartTest2()
        {
            var data = new KnapsackProblemDataParser();
            var constraint = KnapsackProblemDataParser.FindSolutionStart("3369\t3712", data);
            Assert.AreEqual(KnapsackProblemDataParser.FindSolutionStart, constraint);
        }

        [TestMethod]
        public void ReadSolutionResourceOrFindSolutionEndTest1()
        {
            var data = new KnapsackProblemDataParser();
            var resource = new Resource("R1", 5687, 6585, 0);
            data.Resources.Add(resource);

            var constraint =
                KnapsackProblemDataParser.ReadSolutionResourceOrFindSolutionEnd("\t1\t156\t5687\t6585", data);
            Assert.AreEqual(KnapsackProblemDataParser.ReadSolutionResourceOrFindSolutionEnd, constraint);
            Assert.AreSame(resource, data.Solution[0].Item);
            Assert.AreEqual(156, data.Solution[0].Quantity);
        }

        [TestMethod]
        public void ReadSolutionResourceOrFindSolutionEndTest2()
        {
            var data = new KnapsackProblemDataParser();
            var constraint = KnapsackProblemDataParser.ReadSolutionResourceOrFindSolutionEnd("", data);
            Assert.IsNull(constraint);
        }
    }
}