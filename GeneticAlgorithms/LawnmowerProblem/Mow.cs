using System;
using System.Collections.Generic;
using System.Linq;

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
            return "mow";
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
            return "turn";
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
            mower.TurnLeft();
        }

        public override string ToString()
        {
            return $"jump({Forward},{Right}";
        }
    }

    public class Repeat : INode
    {
        public int OpCount { get; }
        public int Times { get; }
        public INode[] Ops { get; set; }

        public Repeat(int opCount, int times, INode[] ops)
        {
            OpCount = opCount;
            Times = times;
            Ops = ops;
        }

        public Repeat(int opCount, int times)
            : this(opCount, times, new INode[0])
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
            var x = Ops.Length > 0
                ? string.Join(" ", Ops.ToString())
                : OpCount.ToString();
            return $"repeat({x} , {Times}";
        }
    }

    public class Func : INode
    {
        public INode[] Ops { get; set; }
        public bool ExpectCall { get; }
        public int? Id { get; set; } = null;

        public Func(INode[] ops, bool expectCall)
        {
            Ops = ops;
            ExpectCall = expectCall;
        }

        public Func(bool expectCall = false)
            : this(new INode[0], expectCall)
        {
        }

        public void Execute(Mower mower, Field field)
        {
            foreach (var op in Ops)
                op.Execute(mower, field);
        }

        public override string ToString()
        {
            return $"Func{string.Join<INode>(" ", Ops)}: {Id}";
        }
    }

    public class Call : INode
    {
        public int? FuncId { get; }
        public INode[] Funcs { get; set; }

        public Call(int? funcId, INode[] funcs)
        {
            FuncId = funcId;
            Funcs = funcs;
        }

        public Call(int? funcId)
            : this(funcId, new INode[0])
        {
        }

        public void Execute(Mower mower, Field field)
        {
            var funcId = FuncId ?? 0;
            if (Funcs.Length > funcId)
                Funcs[funcId].Execute(mower, field);
        }

        public override string ToString()
        {
            return $"Call-{FuncId?.ToString() ?? "Func"}";
        }
    }

    public class Program
    {
        public INode[] Main { get; }
        public Func[] Funcs { get; }

        public Program(INode[] genes)
        {
            var temp = new List<INode>(genes);
            var funcs = new List<Func>();

            for (var index = temp.Count - 1; index >= 0; index--)
            {
                switch (temp[index])
                {
                    case Repeat repeat:
                        var start1 = index + 1;
                        var end1 = Math.Min(index + repeat.OpCount + 1, temp.Count);
                        var x = temp.Skip(start1).Take(end1 - start1).ToArray();
                        temp[index] = new Repeat(repeat.OpCount, repeat.Times, x);
                        temp.RemoveRange(start1, end1 - start1);
                        continue;

                    case Call call:
                        call.Funcs = funcs.Cast<INode>().ToArray();
                        break;

                    case Func func:
                        if (funcs.Count > 0 && !func.ExpectCall)
                        {
                            temp[index] = new Call(null, funcs.Cast<INode>().ToArray());
                            continue;
                        }

                        var start2 = index + 1;
                        var end2 = temp.Count;
                        var func2 = new Func();
                        if (func.ExpectCall)
                            func2.Id = funcs.Count;
                        func2.Ops = temp.Skip(start2).Take(end2 - start2)
                            .Where(t => !(t is Repeat) || ((Repeat) t).Ops.Length > 0).ToArray();
                        funcs.Add(func2);
                        temp.RemoveRange(index, end2 - index);
                        break;
                }
            }

            foreach (var func in funcs)
            {
                for (var index = func.Ops.Length - 1; index >= 0; index--)
                {
                    if (!(func.Ops[index] is Call call))
                        continue;
                    var funcId = call.FuncId;
                    if (funcId == null)
                        continue;
                    if (funcId >= funcs.Count || funcs[(int) funcId].Ops.Length == 0)
                    {
                        var x = new List<INode>(func.Ops);
                        x.RemoveAt(index);
                        func.Ops = x.ToArray();
                    }
                }

                for (var index = func.Ops.Length - 1; index >= 0; index--)
                {
                    if (!(func.Ops[index] is Call call))
                        continue;
                    var funcId = call.FuncId;
                    if (funcId == null)
                        continue;
                    if (funcId >= funcs.Count || funcs[(int) funcId].Ops.Length == 0)
                        temp.RemoveAt(index);
                }
            }

            Main = temp.ToArray();
            Funcs = funcs.ToArray();
        }

        public void Evaluate(Mower mower, Field field)
        {
            foreach (var instruction in Main)
                instruction.Execute(mower, field);
        }

        public void Print()
        {
            if (Funcs == null) return;
            foreach (var func in Funcs)
            {
                if (func.Id != null && func.Ops.Length == 0)
                    continue;
                Console.WriteLine(func);
            }
        }
    }
}