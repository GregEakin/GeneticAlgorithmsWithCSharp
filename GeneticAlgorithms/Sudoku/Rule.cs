namespace GeneticAlgorithms.Sudoku
{
    public class Rule
    {
        public int Index { get; }
        public int OtherIndex { get; }

        public Rule(int index, int otherIndex)
        {
            Index = index;
            OtherIndex = otherIndex;
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case Rule that:
                    return Index == that.Index && OtherIndex == that.OtherIndex;
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return Index * 100 + OtherIndex;
        }
    }
}