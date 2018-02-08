/* File: Board.cs
 *     from chapter 6 of _Genetic Algorithms with Python_
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
using System.Linq;
using System.Text;

namespace GeneticAlgorithms.Knights
{
    public class Board
    {
        private readonly char[,] _board;
        public int Width => _board.GetLength(0);
        public int Height => _board.GetLength(1);

        public char this[int x, int y] => _board[x, y];

        public Board(IReadOnlyList<Position> positions, int width, int height)
        {
            _board = new char[width, height];

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                _board[x, y] = '.';

            foreach (var position in positions)
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