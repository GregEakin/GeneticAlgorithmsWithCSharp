/* File: Mow.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.LawnmowerProblem
{
    public interface INode
    {
        void Execute(Mower mower, Field field);
    }

    public class Mow : INode
    {
        public void Execute(Mower mower, Field field)
        {
            mower.Mow(field);
        }

        public override string ToString()
        {
            return nameof(Mow);
        }
    }

    public class Turn : INode
    {
        public void Execute(Mower mower, Field field)
        {
            mower.TurnLeft();
        }

        public override string ToString()
        {
            return nameof(Turn);
        }
    }

    public class Jump : INode
    {
        public int Forward { get; }
        public int Right { get; }

        public Jump(int forward, int right)
        {
            Forward = forward;
            Right = right;
        }

        public void Execute(Mower mower, Field field)
        {
            mower.Jump(field, Forward, Right);
        }

        public override string ToString()
        {
            return $"{nameof(Jump)}({Forward},{Right})";
        }
    }

    public class Repeat : INode
    {
        public int OpCount { get; }
        public int Times { get; }
        public List<INode> Ops { get; set; }

        public Repeat(int opCount, int times, List<INode> ops)
        {
            OpCount = opCount;
            Times = times;
            Ops = ops;
        }

        public Repeat(int opCount, int times)
            : this(opCount, times, new List<INode>())
        {
        }

        public void Execute(Mower mower, Field field)
        {
            for (var i = 0; i < Times; i++)
                foreach (var op in Ops)
                    op.Execute(mower, field);
        }

        public override string ToString()
        {
            var x = Ops.Any()
                ? $"{{{string.Join(", ", Ops)}}}"
                : OpCount.ToString();
            return $"{nameof(Repeat)}({x}, {Times})";
        }
    }

    public class Func : INode
    {
        public List<INode> Ops { get; set; }
        public bool ExpectCall { get; }
        public int? Id { get; set; }

        public Func(List<INode> ops, bool expectCall)
        {
            Ops = ops;
            ExpectCall = expectCall;
        }

        public Func(bool expectCall = false)
            : this(new List<INode>(), expectCall)
        {
        }

        public void Execute(Mower mower, Field field)
        {
            foreach (var op in Ops)
                op.Execute(mower, field);
        }

        public override string ToString()
        {
            var x = Id != null ? $" {Id}" : "";
            return $"{nameof(Func)}{x}: {{{string.Join(", ", Ops)}}}";
        }
    }

    public class Call : INode
    {
        public int? FuncId { get; }
        public List<INode> Funcs { get; set; }

        public Call(int? funcId, List<INode> funcs = null)
        {
            FuncId = funcId;
            Funcs = funcs;
        }

        public void Execute(Mower mower, Field field)
        {
            var funcId = FuncId ?? 0;
            if (Funcs.Count > funcId)
                Funcs[funcId].Execute(mower, field);
        }

        public override string ToString()
        {
            return $"{nameof(Call)}-{FuncId?.ToString() ?? "Func"}";
        }
    }

    public class Program
    {
        public List<INode> Main { get; }
        public List<INode> Funcs { get; } = new List<INode>();

        public Program(List<INode> genes)
        {
            Main = genes.ToList();
            for (var index = Main.Count - 1; index >= 0; index--)
            {
                switch (Main[index])
                {
                    case Repeat repeat:
                        var start1 = index + 1;
                        var end1 = Math.Min(index + repeat.OpCount + 1, Main.Count);
                        repeat.Ops = Main.Skip(start1).Take(end1 - start1).ToList();
                        Main.RemoveRange(start1, end1 - start1);
                        continue;

                    case Call call:
                        call.Funcs = Funcs;
                        break;

                    case Func func:
                        if (Funcs.Count > 0 && !func.ExpectCall)
                        {
                            Main[index] = new Call(null, Funcs);
                            continue;
                        }

                        var start2 = index + 1;
                        var end2 = Main.Count;
                        var func2 = new Func();
                        if (func.ExpectCall)
                            func2.Id = Funcs.Count;
                        func2.Ops = Main.Skip(start2).Take(end2 - start2)
                            .Where(t => !(t is Repeat) || ((Repeat) t).Ops.Any()).ToList();
                        Funcs.Add(func2);
                        Main.RemoveRange(index, end2 - index);
                        break;
                }
            }

            foreach (var func in Funcs.Cast<Func>())
                for (var index = func.Ops.Count - 1; index >= 0; index--)
                {
                    if (!(func.Ops[index] is Call call))
                        continue;

                    var funcId = call.FuncId;
                    if (funcId == null)
                        continue;

                    if (funcId < Funcs.Count && !((Func) Funcs[(int) funcId]).Ops.Any())
                        continue;

                    func.Ops.RemoveAt(index);
                }

            for (var index = Main.Count - 1; index >= 0; index--)
            {
                if (!(Main[index] is Call call))
                    continue;

                var funcId = call.FuncId;
                if (funcId == null)
                    continue;

                if (funcId < Funcs.Count && ((Func) Funcs[(int) funcId]).Ops.Any())
                    continue;

                Main.RemoveAt(index);
            }
        }

        public void Evaluate(Mower mower, Field field)
        {
            foreach (var instruction in Main)
                instruction.Execute(mower, field);
        }

        public void Print()
        {
            if (Funcs != null)
                foreach (var func in Funcs.Cast<Func>())
                {
                    if (func.Id != null && !func.Ops.Any())
                        continue;
                    Console.WriteLine(func);
                }

            Console.WriteLine("Program: {0}", string.Join(", ", Main));
        }
    }
}