/* File: FitnessTests.cs
 *     from chapter 17 of _Genetic Algorithms with Python_
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

namespace GeneticAlgorithms.RegularExpressions;

[TestClass]
public class FitnessTests
{
    [TestMethod]
    public void CompareToEqual()
    {
        Fitness.UseRegexLength = false;

        var fit1 = new Fitness(1, 2, 3, 4);
        var fit2 = new Fitness(1, 2, 3, 0);
        Assert.IsTrue(fit1.CompareTo(fit2) == 0);
    }


    [TestMethod]
    public void CompareToLessThan()
    {
        Fitness.UseRegexLength = true;

        var fit1 = new Fitness(20, 2, 3, 4);
        var fit2 = new Fitness(1, 2, 3, 0);
        Assert.IsTrue(fit1.CompareTo(fit2) > 0);
    }

    [TestMethod]
    public void CompareToGreaterThan()
    {
        Fitness.UseRegexLength = true;

        var fit1 = new Fitness(1, 2, 3, 5);
        var fit2 = new Fitness(20, 2, 3, 0);
        Assert.IsTrue(fit1.CompareTo(fit2) < 0);
    }

    [TestMethod]
    public void CompareToEqualLength()
    {
        Fitness.UseRegexLength = true;

        var fit1 = new Fitness(1, 2, 3, 4);
        var fit2 = new Fitness(1, 2, 3, 4);
        Assert.IsTrue(fit1.CompareTo(fit2) == 0);
    }

    [TestMethod]
    public void CompareToLessThanLength()
    {
        Fitness.UseRegexLength = true;

        var fit1 = new Fitness(1, 2, 3, 4);
        var fit2 = new Fitness(1, 2, 3, 5);
        Assert.IsTrue(fit1.CompareTo(fit2) > 0);
    }

    [TestMethod]
    public void CompareToGreaterThanLength()
    {
        Fitness.UseRegexLength = true;

        var fit1 = new Fitness(1, 2, 3, 5);
        var fit2 = new Fitness(1, 2, 3, 4);
        Assert.IsTrue(fit1.CompareTo(fit2) < 0);
    }
}