/* File: CircuitTests2.cs
 *     from chapter 16 of _Genetic Algorithms with Python_
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

namespace GeneticAlgorithms.LogicCircuits;

[TestClass]
public class CircuitTests2
{
    [TestMethod]
    public void NotTOutputTrueTest()
    {
        var sourceContainer = new Dictionary<char, bool?> {{'A', false}};
        var not = new Not(new Source('A', sourceContainer));
        Assert.IsNotNull(not.Output);
        Assert.IsTrue((bool) not.Output);
    }

    [TestMethod]
    public void NotTOutputFalseTest()
    {
        var sourceContainer = new Dictionary<char, bool?> {{'A', true}};
        var not = new Not(new Source('A', sourceContainer));
        Assert.IsNotNull(not.Output);
        Assert.IsFalse((bool) not.Output);
    }

    [TestMethod]
    public void NotTOutputNullTest()
    {
        var sourceContainer = new Dictionary<char, bool?> {{'A', null}};
        var not = new Not(new Source('A', sourceContainer));
        Assert.IsNull(not.Output);
    }

    [TestMethod]
    public void NotTOutputNullContainerTest()
    {
        var not = new Not(null);
        Assert.IsNull(not.Output);
    }

    [TestMethod]
    public void NotInputCountTest()
    {
        var not = new Not(null);
        Assert.AreEqual(1, not.InputCount);
    }

    [TestMethod]
    public void NotToStringTest()
    {
        var sourceContainer = new Dictionary<char, bool?> {{'A', true}};
        var not = new Not(new Source('A', sourceContainer));
        Assert.AreEqual("Not(A)", not.ToString());
    }

    [TestMethod]
    public void AndTOutputTrueTest()
    {
        var sourceContainer = new Dictionary<char, bool?> {{'A', true}, {'B', true}};
        var and = new And(new Source('A', sourceContainer), new Source('B', sourceContainer));
        Assert.IsNotNull(and.Output);
        Assert.IsTrue((bool) and.Output);
    }

    [TestMethod]
    public void AndTOutputFalse1Test()
    {
        var sourceContainer = new Dictionary<char, bool?> {{'A', false}, {'B', true}};
        var and = new And(new Source('A', sourceContainer), new Source('B', sourceContainer));
        Assert.IsNotNull(and.Output);
        Assert.IsFalse((bool) and.Output);
    }

    [TestMethod]
    public void AndTOutputFalse2Test()
    {
        var sourceContainer = new Dictionary<char, bool?> {{'A', true}, {'B', false}};
        var and = new And(new Source('A', sourceContainer), new Source('B', sourceContainer));
        Assert.IsNotNull(and.Output);
        Assert.IsFalse((bool) and.Output);
    }

    [TestMethod]
    public void AndTOutputFalse3Test()
    {
        var sourceContainer = new Dictionary<char, bool?> {{'A', false}, {'B', false}};
        var and = new And(new Source('A', sourceContainer), new Source('B', sourceContainer));
        Assert.IsNotNull(and.Output);
        Assert.IsFalse((bool) and.Output);
    }

    [TestMethod]
    public void AndTOutputNullTest()
    {
        var sourceContainer = new Dictionary<char, bool?> {{'A', true}, {'B', null}};
        var and = new And(new Source('A', sourceContainer), new Source('B', sourceContainer));
        Assert.IsNull(and.Output);
    }

    [TestMethod]
    public void AndTOutputNullContainerTest()
    {
        var and = new And(null, null);
        Assert.IsNull(and.Output);
    }

    [TestMethod]
    public void AndInputCountTest()
    {
        var and = new And(null, null);
        Assert.AreEqual(2, and.InputCount);
    }

    [TestMethod]
    public void AndToStringTest()
    {
        var sourceContainer = new Dictionary<char, bool?> {{'A', true}, {'B', false}};
        var and = new And(new Source('A', sourceContainer), new Source('B', sourceContainer));
        Assert.AreEqual("And(A, B)", and.ToString());
    }

    [TestMethod]
    public void ComplexTest()
    {
        var sourceContainer = new Dictionary<char, bool?> {{'A', true}, {'B', false}, {'C', true}, {'D', false}};
        var source0 = new Source('A', sourceContainer);
        var source1 = new Source('B', sourceContainer);
        var source2 = new Source('C', sourceContainer);
        var source3 = new Source('D', sourceContainer);
        var and = new And(source0, source1);
        var or = new Or(source2, source1);
        var xor = new Xor(source3, source0);
        var not = new Not(and);
        var or1 = new Or(or, xor);
        var xor1 = new Xor(not, or1);
        Assert.AreEqual("Xor(Not(And(A, B)), Or(Or(C, B), Xor(D, A)))", xor1.ToString());
        Assert.IsNotNull(xor1.Output);
        Assert.IsFalse((bool) xor1.Output);
    }
}