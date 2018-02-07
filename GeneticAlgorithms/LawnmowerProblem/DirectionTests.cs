/* File: DirectionTests.cs
 *     from chapter 15 of _Genetic Algorithms with Python_
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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.LawnmowerProblem
{
    [TestClass]
    public class DirectionTests
    {
        [TestMethod]
        public void MoveFromTest()
        {
            var north = Directions.North;
            var location = new Location(5, 5);
            var next = north.MoveFrom(location, 4);
            Assert.AreEqual(5, next.X);
            Assert.AreEqual(1, next.Y);
        }
    }

    [TestClass]
    public class DirectionsTests
    {
        [TestMethod]
        public void NortTest()
        {
            var north = Directions.North;
            Assert.AreEqual(0, north.Index);
            Assert.AreEqual('^', north.Symbol);
            Assert.AreEqual(0, north.XOffset);
            Assert.AreEqual(-1, north.YOffset);
        }

        [TestMethod]
        public void EastTest()
        {
            var east = Directions.East;
            Assert.AreEqual(1, east.Index);
            Assert.AreEqual('>', east.Symbol);
            Assert.AreEqual(1, east.XOffset);
            Assert.AreEqual(0, east.YOffset);
        }

        [TestMethod]
        public void SouthTest()
        {
            var south = Directions.South;
            Assert.AreEqual(2, south.Index);
            Assert.AreEqual('v', south.Symbol);
            Assert.AreEqual(0, south.XOffset);
            Assert.AreEqual(1, south.YOffset);
        }

        [TestMethod]
        public void WestTest()
        {
            var west = Directions.West;
            Assert.AreEqual(3, west.Index);
            Assert.AreEqual('<', west.Symbol);
            Assert.AreEqual(-1, west.XOffset);
            Assert.AreEqual(0, west.YOffset);
        }

        [TestMethod]
        public void NorthTurnLeftTest()
        {
            var west = Directions.GetDirectionAfterTurnLeft90Degrees(Directions.North);
            Assert.AreSame(Directions.West, west);
        }

        [TestMethod]
        public void NorthTurnRightTest()
        {
            var east = Directions.GetDirectionAfterTurnRight90Degrees(Directions.North);
            Assert.AreSame(Directions.East, east);
        }

        [TestMethod]
        public void SouthTurnLeftTest()
        {
            var east = Directions.GetDirectionAfterTurnLeft90Degrees(Directions.South);
            Assert.AreSame(Directions.East, east);
        }

        [TestMethod]
        public void SouthTurnRightTest()
        {
            var west = Directions.GetDirectionAfterTurnRight90Degrees(Directions.South);
            Assert.AreSame(Directions.West, west);
        }

        [TestMethod]
        public void EastTurnLeftTest()
        {
            var north = Directions.GetDirectionAfterTurnLeft90Degrees(Directions.East);
            Assert.AreSame(Directions.North, north);
        }

        [TestMethod]
        public void EastTurnRightTest()
        {
            var south = Directions.GetDirectionAfterTurnRight90Degrees(Directions.East);
            Assert.AreSame(Directions.South, south);
        }

        [TestMethod]
        public void WestTurnLeftTest()
        {
            var south = Directions.GetDirectionAfterTurnLeft90Degrees(Directions.West);
            Assert.AreSame(Directions.South, south);
        }

        [TestMethod]
        public void WestTurnRightTest()
        {
            var north = Directions.GetDirectionAfterTurnRight90Degrees(Directions.West);
            Assert.AreSame(Directions.North, north);
        }
    }

    [TestClass]
    public class LocationTests
    {
        [TestMethod]
        public void MoveTest()
        {
            var location = new Location(5, 5);
            var next = location.Move(-2, 3);
            Assert.AreEqual(3, next.X);
            Assert.AreEqual(8, next.Y);
        }
    }

    [TestClass]
    public class MowerTests
    {
        [TestMethod]
        public void TurnLeftTest()
        {
            var location = new Location(5, 5);
            var dir = Directions.North;
            var mower = new Mower(location, dir);
            mower.TurnLeft();
            Assert.AreSame(Directions.West, mower.Direction);
            Assert.AreEqual(location, mower.Location);
            Assert.AreEqual(1, mower.StepCount);
        }

        [TestMethod]
        public void MowTest()
        {
            var field = new ValidatingField(7, 3, FieldContents.Grass);
            var mower = new Mower(new Location(1, 1), Directions.East);
            field.Display(mower);

            for (var i = 0; i < 4; i++)
            {
                mower.Mow(field);
                field.Display(mower);
            }

            for (var i = 1; i < 5; i++)
                Assert.AreEqual(i.ToString().PadLeft(2), field[i + 1, 1]);
        }

        [TestMethod]
        public void JumpTest()
        {
            var location = new Location(3, 2);
            var dir = Directions.West;
            var mower = new Mower(location, dir);
            var field = new ValidatingField(4, 4, FieldContents.Grass);
            field.Display(mower);

            mower.Jump(field, 2, 1);
            field.Display(mower);
            Assert.AreEqual(" 1", field[1, 1]);
            Assert.AreEqual(new Location(1, 1), mower.Location);
        }

        [TestMethod]
        public void JumpFailedTest()
        {
            var location = new Location(3, 2);
            var dir = Directions.East;
            var mower = new Mower(location, dir);
            var field = new ValidatingField(4, 4, FieldContents.Grass);
            field.Display(mower);

            mower.Jump(field, 2, 1);
            field.Display(mower);
            Assert.AreEqual(new Location(3, 2), mower.Location);
        }
    }

    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void RepeatTest()
        {
            var genes = new List<INode> {new Turn(), new Repeat(2, 3), new Mow(), new Mow(), new Turn()};
            var program = new Program(genes);
            Assert.AreEqual("Turn, Repeat({Mow, Mow}, 3), Turn", string.Join(", ", program.Main));
            Assert.AreEqual(0, program.Funcs.Count);
        }

        [TestMethod]
        public void CallTest()
        {
            var genes = new List<INode> {new Turn(), new Call(0), new Turn(), new Func(), new Turn(), new Mow() };
            var program = new Program(genes);
            Assert.AreEqual("Turn, Call-0, Turn", string.Join(", ", program.Main));
            Assert.AreEqual("Func: {Turn, Mow}", string.Join("; ", program.Funcs));
        }

        [TestMethod]
        public void FuncFuncTest()
        {
            var genes = new List<INode> { new Turn(), new Func(), new Turn(), new Func(), new Turn(), new Mow() };
            var program = new Program(genes);
            Assert.AreEqual("Turn, Call-Func, Turn", string.Join(", ", program.Main));
            Assert.AreEqual("Func: {Turn, Mow}", string.Join("; ", program.Funcs));
        }

        [TestMethod]
        public void FuncFalseTest()
        {
            var genes = new List<INode> { new Turn(), new Turn(), new Func(), new Turn(), new Mow() };
            var program = new Program(genes);
            Assert.AreEqual("Turn, Turn", string.Join(", ", program.Main));
            Assert.AreEqual("Func: {Turn, Mow}", string.Join("; ", program.Funcs));
        }

        [TestMethod]
        public void FuncTrueTest()
        {
            var genes = new List<INode> { new Call(0), new Turn(), new Func(true), new Turn(), new Mow() };
            var program = new Program(genes);
            Assert.AreEqual("Call-0, Turn", string.Join(", ", program.Main));
            Assert.AreEqual("Func 0: {Turn, Mow}", string.Join("; ", program.Funcs));
        }

        [TestMethod]
        public void FuncTwoTest()
        {
            var genes = new List<INode> { new Call(0), new Call(1), new Func(true), new Turn(), new Mow(), new Func(true), new Mow(), new Turn() };
            var program = new Program(genes);
            Assert.AreEqual("Call-0, Call-1", string.Join(", ", program.Main));
            Assert.AreEqual("Func 0: {Mow, Turn}; Func 1: {Turn, Mow}", string.Join("; ", program.Funcs));
        }

        [TestMethod]
        public void FuncTwo22Test()
        {
            var genes = new List<INode> { new Func(), new Func(true), new Turn(), new Mow(), new Func(true), new Mow(), new Turn() };
            var program = new Program(genes);
            Assert.AreEqual("Call-Func", string.Join(", ", program.Main));
            Assert.AreEqual("Func 0: {Mow, Turn}; Func 1: {Turn, Mow}", string.Join("; ", program.Funcs));
        }
    }

    [TestClass]
    public class FieldTests
    {
        [TestMethod]
        public void FieldTest()
        {
            var field = new ValidatingField(3, 2, FieldContents.Grass);
            Assert.AreEqual(3, field.Width);
            Assert.AreEqual(2, field.Height);
            for (var x = 0; x < 3; x++)
            for (var y = 0; y < 2; y++)
                Assert.AreEqual(FieldContents.Grass, field[x, y]);
            Assert.AreEqual(0, field.CountMowed());
        }

        [TestMethod]
        public void CountMowedTest()
        {
            var field = new ValidatingField(3, 2, FieldContents.Grass)
            {
                [0, 0] = FieldContents.Mowed,
                [0, 1] = FieldContents.Mowed,
                [1, 0] = FieldContents.Mowed
            };
            Assert.AreEqual(3, field.CountMowed());
        }

        [TestMethod]
        public void DisplayTest()
        {
            var field = new ValidatingField(5, 7, FieldContents.Grass)
            {
                [0, 0] = FieldContents.Mowed,
                [0, 1] = FieldContents.Mowed,
                [1, 0] = FieldContents.Mowed
            };
            var mower = new Mower(new Location(2, 3), Directions.West);
            field.Display(mower);
        }
    }

    [TestClass]
    public class ValidatingFieldTests
    {
        [TestMethod]
        public void OriginFixTest()
        {
            var field = new ValidatingField(2, 3, FieldContents.Grass);
            var tuple = field.FixLocation(new Location(0, 0));
            Assert.AreEqual(new Location(0, 0), tuple.Item1);
            Assert.IsTrue(tuple.Item2);
        }

        [TestMethod]
        public void NegativeXTest()
        {
            var field = new ValidatingField(2, 3, FieldContents.Grass);
            var tuple = field.FixLocation(new Location(-1, 0));
            Assert.IsNull(tuple.Item1);
            Assert.IsFalse(tuple.Item2);
        }

        [TestMethod]
        public void BigXTest()
        {
            var field = new ValidatingField(2, 3, FieldContents.Grass);
            var tuple = field.FixLocation(new Location(3, 0));
            Assert.IsNull(tuple.Item1);
            Assert.IsFalse(tuple.Item2);
        }

        [TestMethod]
        public void NegativeYTest()
        {
            var field = new ValidatingField(2, 3, FieldContents.Grass);
            var tuple = field.FixLocation(new Location(0, -1));
            Assert.IsNull(tuple.Item1);
            Assert.IsFalse(tuple.Item2);
        }

        [TestMethod]
        public void BigYTest()
        {
            var field = new ValidatingField(2, 3, FieldContents.Grass);
            var tuple = field.FixLocation(new Location(0, 4));
            Assert.IsNull(tuple.Item1);
            Assert.IsFalse(tuple.Item2);
        }
    }

    [TestClass]
    public class ToroidFieldTests
    {
        [TestMethod]
        public void WrapAroundTest()
        {
            var field = new ToroidField(2, 3, FieldContents.Grass);
            var tuple = field.FixLocation(new Location(-1, 4));
            Assert.AreEqual(new Location(1, 1), tuple.Item1);
            Assert.IsTrue(tuple.Item2);
        }

        [TestMethod]
        public void ModTest()
        {
            Assert.AreEqual(7, ToroidField.Mod(-3, 10));
            Assert.AreEqual(5, ToroidField.Mod(5, 10));
            Assert.AreEqual(8, ToroidField.Mod(18, 10));
        }
    }
}