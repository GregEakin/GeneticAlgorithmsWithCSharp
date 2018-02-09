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
        //# board layout is
        //#   1  2  3
        //#   4  5  6
        //#   7  8  9

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
                case 1:
                    Corner = true;
                    TopRow = true;
                    Row = new[] {1, 2, 3};
                    LeftColumn = true;
                    Column = new[] {1, 4, 7};
                    RowOpposite = 3;
                    ColumnOpposite = 7;
                    DiagonalOpposite = 9;
                    Diagonals.Add(new[] { 1, 5, 9 });
                    break;
                case 2:
                    Side = true;
                    TopRow = true;
                    Row = new[] {1, 2, 3};
                    MiddleColumn = true;
                    Column = new[] {2, 5, 8};
                    ColumnOpposite = 8;
                    break;
                case 3:
                    Corner = true;
                    TopRow = true;
                    Row = new[] {1, 2, 3};
                    RightColumn = true;
                    Column = new[] {3, 6, 9};
                    RowOpposite = 1;
                    ColumnOpposite = 9;
                    DiagonalOpposite = 7;
                    Diagonals.Add(new[] { 7, 5, 3 });
                    break;
                case 4:
                    Side = true;
                    MiddleRow = true;
                    Row = new[] {4, 5, 6};
                    LeftColumn = true;
                    Column = new[] {1, 4, 7};
                    RowOpposite = 6;
                    break;
                case 5:
                    Center = true;
                    MiddleRow = true;
                    Row = new[] {4, 5, 6};
                    MiddleColumn = true;
                    Column = new[] {2, 5, 8};
                    Diagonals.Add(new[] { 1, 5, 9 });
                    Diagonals.Add(new[] { 7, 5, 3 });
                    break;
                case 6:
                    Side = true;
                    MiddleRow = true;
                    Row = new[] {4, 5, 6};
                    RightColumn = true;
                    Column = new[] {3, 6, 9};
                    RowOpposite = 4;
                    break;
                case 7:
                    Corner = true;
                    BottomRow = true;
                    Row = new[] {7, 8, 9};
                    LeftColumn = true;
                    Column = new[] {1, 4, 7};
                    RowOpposite = 9;
                    ColumnOpposite = 1;
                    DiagonalOpposite = 3;
                    Diagonals.Add(new[] { 7, 5, 3 });
                    break;
                case 8:
                    Side = true;
                    MiddleColumn = true;
                    Column = new[] {2, 5, 8};
                    BottomRow = true;
                    Row = new[] {7, 8, 9};
                    ColumnOpposite = 2;
                    break;
                case 9:
                    Corner = true;
                    BottomRow = true;
                    Row = new[] {7, 8, 9};
                    RightColumn = true;
                    Column = new[] {3, 6, 9};
                    RowOpposite = 7;
                    ColumnOpposite = 3;
                    DiagonalOpposite = 1;
                    Diagonals.Add(new[] { 1, 5, 9 });
                    break;
                default:
                    throw new AccessViolationException();
            }
        }
    }
}