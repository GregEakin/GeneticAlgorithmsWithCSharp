using System.Collections.Generic;

namespace GeneticAlgorithms.LogicCircuits
{
    public interface ICircuit
    {
        bool? Output { get; }
        int InputCount { get; }
    }

    public class Not : ICircuit
    {
        private readonly ICircuit _input;

        public Not(ICircuit input)
        {
            _input = input;
        }

        public bool? Output => !_input?.Output;

        public int InputCount => 1;

        public override string ToString() => _input == null ? "Not(?)" : $"Not({_input})";
    }

    public delegate bool FnTest(bool indexA, bool indexB);

    public abstract class GateWith2Inputs : ICircuit
    {
        private readonly ICircuit _inputA;
        private readonly ICircuit _inputB;
        private readonly string _label;
        private readonly FnTest _fnTest;

        protected GateWith2Inputs(ICircuit inputA, ICircuit inputB, string label, FnTest fnTest)
        {
            _inputA = inputA;
            _inputB = inputB;
            _label = label;
            _fnTest = fnTest;
        }

        public bool? Output
        {
            get
            {
                var aValue = _inputA?.Output;
                if (aValue == null)
                    return null;
                var bValue = _inputB?.Output;
                if (bValue == null)
                    return null;
                return _fnTest((bool) aValue, (bool) bValue);
            }
        }

        public int InputCount => 2;

        public override string ToString() =>
            _inputA == null || _inputB == null ? $"{_label}(?)" : $"{_label}({_inputA}, {_inputB})";
    }

    public class And : GateWith2Inputs
    {
        public And(ICircuit inputA, ICircuit inputB)
            : base(inputA, inputB, nameof(And), (a, b) => a && b)
        {
        }
    }

    public class Or : GateWith2Inputs
    {
        public Or(ICircuit inputA, ICircuit inputB)
            : base(inputA, inputB, nameof(Or), (a, b) => a || b)
        {
        }
    }

    public class Xor : GateWith2Inputs
    {
        public Xor(ICircuit inputA, ICircuit inputB)
            : base(inputA, inputB, nameof(Xor), (a, b) => a != b)
        {
        }
    }

    public class Source : ICircuit
    {
        private readonly char _sourceId;
        private readonly Dictionary<char, bool?> _sourceContainer;

        public Source(char sourceId, Dictionary<char, bool?> sourceContainer)
        {
            _sourceId = sourceId;
            _sourceContainer = sourceContainer;
        }

        public bool? Output => _sourceContainer[_sourceId];

        public int InputCount => 0;

        public override string ToString() => _sourceId.ToString();
    }
}