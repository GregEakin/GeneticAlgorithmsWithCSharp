using System.Collections.Generic;
using System.Text;

namespace PrimeTesting.queens
{
    public class Board
    {
        private readonly char[,] _board;

        public char this[int x, int y] => _board[x, y];

        public Board(IReadOnlyList<int> genes, int size)
        {
            _board = new char[size, size];

            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
                _board[i, j] = '.';

            for (var i = 0; i < genes.Count; i += 2)
            {
                var row = genes[i];
                var col = genes[i + 1];
                _board[row, col] = 'Q';
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            for (var j = _board.GetLength(1) - 1; j >= 0; j--)
            {
                for (var i = 0; i < _board.GetLength(0); i++)
                    builder.AppendFormat("{0} ", _board[i, j]);
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}