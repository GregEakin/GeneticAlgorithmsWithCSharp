/* File: Benchmark.cs
 *     from chapter 4 of _Genetic Algorithms with Python_
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

using System.Text;

namespace GeneticAlgorithms.Queens;

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