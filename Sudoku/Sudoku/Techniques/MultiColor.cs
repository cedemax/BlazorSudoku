using BlazorSudoku.Hints;
namespace BlazorSudoku.Techniques
{
    public class MultiColor : SudokuTechnique
    {
        public override int MinComplexity => 32;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit)
        {
            if (complexityLimit < MinComplexity)
                return new();

            var moves = new List<SudokuMove>();

            for (var n = 0; n < sudoku.N; ++n)
            {
                var done = new HashSet<(SudokuCell cell, int n)>();
                var starts = sudoku.UnsetCells.Where(x => x.ConjugatePairs(n).Any());
                var color = 1;
                var coloring = new Dictionary<SudokuCell, int>();
                foreach (var start in starts)
                {
                    if (coloring.ContainsKey(start))
                        continue;
                    var added = ColorMulti(start, n, coloring,color);
                    if (added == 1)
                        coloring.Remove(coloring.FirstOrDefault(x => x.Value == color).Key);
                    if (added <= 1)
                        continue;   // nothing colored
                    color++;
                }

                var colorPairs = Enumerable.Range(1, color-1).GetCombinations(2);
                foreach(var colorPair in colorPairs)
                {
                    var colorA = colorPair[0];
                    var colorB = colorPair[1];

                    var move = new SudokuMove($"Dual Color on {n + 1}", colorPair.Length* coloring.Count * 4);
                    // too complex
                    if (move.Complexity > complexityLimit)
                        continue;

                    var coloringA = coloring.Where(x => Math.Abs(x.Value) == colorA).ToDictionary(x => x.Key, x => x.Value);
                    var coloringB = coloring.Where(x => Math.Abs(x.Value) == colorB).ToDictionary(x => x.Key, x => x.Value);

                    var pairA = coloringA.Keys.FirstOrDefault(x => coloringB.Keys.Any(y => x.Sees(y)));

                    var pairs = new List<(SudokuCell A, SudokuCell B)>();

                    foreach(var (cA,vA) in coloringA)
                        foreach (var (cB, vB) in coloringB)
                            if (cA.Sees(cB))
                                pairs.Add((cA, cB));

                    foreach(var type1 in pairs.GroupBy(x => (coloringA[x.A],coloringB[x.B])))
                    {
                        var oppositeA = -type1.Key.Item1;
                        var oppositeB = -type1.Key.Item2;

                        foreach(var cell in sudoku.Cells)
                        {
                            if (!done.Contains((cell,n)) &&
                                cell.PossibleValues.Contains(n) &&
                                cell.VisibleUnset.Any(x => coloringA.ContainsKey(x) && coloringA[x] == oppositeA) &&
                                cell.VisibleUnset.Any(x => coloringB.ContainsKey(x) && coloringB[x] == oppositeB))
                            {
                                move.Operations.Add(new SudokuAction(cell, SudokuActionType.RemoveOption, n, "Removed by multi-color type 1"));
                                done.Add((cell, n));
                            }
                        }
                    }
                    var allColors = new int[] { colorA, -colorA, colorB, -colorB };
                    var subColorings = new[] { coloringA, coloringB };
                    foreach (var subColoring in subColorings)
                    {
                        var otherColoring = subColorings.First(x => x != subColoring);
                        foreach (var type2 in subColoring.Values.Distinct())
                        {
                            var otherColorsSeen = allColors.Where(x => Math.Abs(x) != type2).ToDictionary(x => x, x => 0);
                            foreach (var (c, v) in subColoring.Where(x => x.Value == type2))
                                foreach (var co in c.VisibleUnset.Where(x => otherColoring.ContainsKey(x)))
                                    otherColorsSeen[otherColoring[co]] += 1;
                            if(otherColorsSeen.All(x => x.Value >= 1))
                            {
                                // type 2
                                foreach (var (c, v) in subColoring.Where(x => x.Value == type2))
                                {
                                    if (!done.Contains((c, n)))
                                    {
                                        done.Add((c, v));
                                        move.Operations.Remove(new SudokuAction(c, SudokuActionType.RemoveOption, n, "Removed by multi-color type 2"));
                                    }
                                }
                                  
                            }
                        }
                    }
                    if (!move.IsEmpty)
                    {
                        foreach(var subColoring in subColorings)
                            foreach (var (k, v) in subColoring)
                                move.Hints.Add(new SudokuCellHint(k, SudokuHint.Coloring(Math.Sign(v)*(Array.IndexOf(subColorings,subColoring) +1))));

                        moves.Add(move);
                        if (moves.Count >= limit)
                            return moves;
                    }
                }
            }

           
            return moves;
        }

       

        private int ColorMulti(SudokuCell cell, int value, Dictionary<SudokuCell, int> dict, int color)
        {
            var added = 0;
            if (!dict.ContainsKey(cell))
            {
                if (!cell.PossibleValues.Contains(value))
                    throw new Exception();
                dict[cell] = color;
                added++;
                foreach (var cp in cell.ConjugatePairs(value))
                    added += ColorMulti(cp, value, dict, -color);
            }
            return added;
        }
    }
}
