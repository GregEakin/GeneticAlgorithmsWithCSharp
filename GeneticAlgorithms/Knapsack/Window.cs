namespace GeneticAlgorithms.knapsack
{
    public class Window
    {
        public int Min { get; }
        public int Max { get; }
        public int Size { get; set; }

        public Window(int min, int max, int size)
        {
            Min = min;
            Max = max;
            Size = size;
        }

        public void Slide()
        {
            Size = (Size > Min) ? Size - 1 : Max;
        }
    }
}