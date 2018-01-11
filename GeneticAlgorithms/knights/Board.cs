using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticAlgorithms.knights
{
    public class Board
    {
        private readonly char[,] _board;
        public int Width => _board.GetLength(0);
        public int Height => _board.GetLength(1);

        public char this[int x, int y] => _board[x, y];

        public Board(IReadOnlyList<Position> genes, int width, int height)
        {
            _board = new char[width, height];

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                _board[x, y] = '.';

            foreach (var position in genes)
                _board[position.X, position.Y] = 'N';
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            for (var y = Height - 1; y >= 0; y--)
            {
                builder.AppendFormat("{0}\t", y);
                for (var x = 0; x < Width; x++)
                    builder.AppendFormat("{0} ", _board[x, y]);
                builder.AppendLine();
            }

            builder.AppendFormat(" \t{0}", string.Join(" ", Enumerable.Range(0, Width)));
            return builder.ToString();
        }
    }
}