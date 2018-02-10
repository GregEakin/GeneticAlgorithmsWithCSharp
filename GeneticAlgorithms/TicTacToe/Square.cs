/* File: Square.cs
 *     from chapter 18 of _Genetic Algorithms with Python_
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

namespace GeneticAlgorithms.TicTacToe
{
    public class Square
    {
        //# board layout is a magic square
        //# each row, column, and diagonal sums to 15
        //#   8  3  4
        //#   1  5  9
        //#   6  7  2

        public static readonly int[] Indexes = { 8, 3, 4, 1, 5, 9, 6, 7, 2 };

        public ContentType Content { get; }
        public int Index { get; }
        public List<int[]> Diagonals { get; } = new List<int[]>();
        public bool Center { get; }
        public bool Corner { get; }
        public bool Side { get; }
        public bool TopRow { get; }
        public bool MiddleRow { get; }
        public bool BottomRow { get; }
        public bool LeftColumn { get; }
        public bool MiddleColumn { get; }
        public bool RightColumn { get; }
        public int[] Row { get; }
        public int[] Column { get; }
        public int? DiagonalOpposite { get; }
        public int? RowOpposite { get; }
        public int? ColumnOpposite { get; }

        public Square(int index, ContentType content = ContentType.Empty)
        {
            Index = index;
            Content = content;

            switch (index)
            {
                case 8:
                    Corner = true;
                    TopRow = true;
                    Row = new[] {8, 3, 4};
                    LeftColumn = true;
                    Column = new[] {8, 1, 6};
                    RowOpposite = 4;
                    ColumnOpposite = 6;
                    DiagonalOpposite = 2;
                    Diagonals.Add(new[] { 8, 5, 2 });
                    break;
                case 3:
                    Side = true;
                    TopRow = true;
                    Row = new[] {8, 3, 4};
                    MiddleColumn = true;
                    Column = new[] {3, 5, 7};
                    ColumnOpposite = 7;
                    break;
                case 4:
                    Corner = true;
                    TopRow = true;
                    Row = new[] {8, 3, 4};
                    RightColumn = true;
                    Column = new[] {4, 9, 2};
                    RowOpposite = 8;
                    ColumnOpposite = 2;
                    DiagonalOpposite = 6;
                    Diagonals.Add(new[] { 6, 5, 4 });
                    break;
                case 1:
                    Side = true;
                    MiddleRow = true;
                    Row = new[] {1, 5, 9};
                    LeftColumn = true;
                    Column = new[] {8, 1, 6};
                    RowOpposite = 9;
                    break;
                case 5:
                    Center = true;
                    MiddleRow = true;
                    Row = new[] {1, 5, 9};
                    MiddleColumn = true;
                    Column = new[] {3, 5, 7};
                    Diagonals.Add(new[] { 8, 5, 2 });
                    Diagonals.Add(new[] { 6, 5, 4 });
                    break;
                case 9:
                    Side = true;
                    MiddleRow = true;
                    Row = new[] {1, 5, 9};
                    RightColumn = true;
                    Column = new[] {4, 9, 2};
                    RowOpposite = 1;
                    break;
                case 6:
                    Corner = true;
                    BottomRow = true;
                    Row = new[] {6, 7, 2};
                    LeftColumn = true;
                    Column = new[] {8, 1, 6};
                    RowOpposite = 2;
                    ColumnOpposite = 8;
                    DiagonalOpposite = 4;
                    Diagonals.Add(new[] { 6, 5, 4 });
                    break;
                case 7:
                    Side = true;
                    MiddleColumn = true;
                    Column = new[] {3, 5, 7};
                    BottomRow = true;
                    Row = new[] {6, 7, 2};
                    ColumnOpposite = 3;
                    break;
                case 2:
                    Corner = true;
                    BottomRow = true;
                    Row = new[] {6, 7, 2};
                    RightColumn = true;
                    Column = new[] {4, 9, 2};
                    RowOpposite = 6;
                    ColumnOpposite = 4;
                    DiagonalOpposite = 8;
                    Diagonals.Add(new[] { 8, 5, 2 });
                    break;
                default:
                    throw new AccessViolationException();
            }
        }
    }
}