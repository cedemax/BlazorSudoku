using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class Fish : SudokuTechnique
    {
        public override int MinComplexity => 16;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit = int.MaxValue)
        {
            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            for (var n = 2; n <= 4; ++n)
            {
                foreach (var bases in sudoku.GetNonOverlappingSets(n))
                {
                    var fishDigitsBase = bases.Select(x => x.Unset).Intersect();
                    if (fishDigitsBase.Count == 0)
                        continue;

                    var baseCells = bases.SelectMany(x => x.Cells).ToArray();

                    var coverAlternatives = sudoku.Domains.Where(x => !bases.Contains(x) && x.Overlaps(bases));

                    foreach (var covers in sudoku.GetNonOverlappingSets(n, coverAlternatives))
                    {
                        var fishDigits = covers.Skip(1).Aggregate<SudokuDomain, IEnumerable<int>>(bases[0].Unset, (a, b) => a.Intersect(b.Unset)).ToHashSet();
                        fishDigits.IntersectWith(fishDigitsBase);
                        if (fishDigits.Count == 0)
                            continue;

                        var coverOnlyCells = covers.SelectMany(x => x.Cells)
                            .Where(x => !x.Domains.Intersect(bases).Any())
                            .ToArray();

                        // shared digits, now make sure it is a fish
                        foreach (var fishDigit in fishDigits)
                        {
                            var fishEliminations = coverOnlyCells.Where(x => x.PossibleValues.Contains(fishDigit)).ToArray();
                            if (fishEliminations.Length == 0)
                                continue;

                            var baseCandidates = baseCells
                               .Where(x => x.PossibleValues.Contains(fishDigit)).ToHashSet();
                            var coveredBaseCandidates = baseCandidates.Where(x => x.Domains.Intersect(covers).Any()).ToArray();
                            var finCount = baseCandidates.Count - coveredBaseCandidates.Length;
                            SudokuCell[] fins = finCount == 0?Array.Empty<SudokuCell>(): baseCandidates.Except(coveredBaseCandidates).ToArray();
                            if (finCount > 0)
                            {
                                var finEliminations =  fins.Select(x => x.VisibleUnset.Where(x => x.PossibleValues.Contains(fishDigit) && !coveredBaseCandidates.Contains(x))).Intersect();
                                fishEliminations = finEliminations.Intersect(fishEliminations).ToArray();
                                if (fishEliminations.Length == 0)
                                    continue;
                            }

                            // it is a fish

                            var nonColRow = bases.Concat(covers).Count(x => !x.IsColOrRow);
                            var move = new SudokuMove(GetName(n, nonColRow, finCount), GetComplexity(n,nonColRow,finCount));

                            foreach (var fishElimination in fishEliminations)
                                move.Operations.Add(new SudokuAction(fishElimination, SudokuActionType.RemoveOption, fishDigit, "eliminated by fish"));

                            if (!move.IsEmpty)
                            {
                                foreach (var bd in bases)
                                    move.Hints.Add(new SudokuDomainHint(bd, SudokuHint.Base));
                                foreach (var cd in covers)
                                    move.Hints.Add(new SudokuDomainHint(cd, SudokuHint.Cover));
                                foreach (var bc in bases.SelectMany(x => x.Cells).Intersect(covers.SelectMany(x => x.Cells)).Where(x => x.PossibleValues.Contains(fishDigit)))
                                    move.Hints.Add(new SudokuCellOptionHint(bc, fishDigit, SudokuHint.Direct));

                                foreach(var fin in fins)
                                    move.Hints.Add(new SudokuCellOptionHint(fin, fishDigit, SudokuHint.Fin));

                                moves.Add(move);
                                if (moves.Count >= limit)
                                    return moves;
                            }



                        }
                    }
                }
            }
            return moves;
        }

        private int GetComplexity(int n, int nonColRow, int fins) => (int)Math.Round(4 * n * n * (1 + Math.Sqrt(nonColRow)) * (1 + Math.Sqrt(fins)));

        private string GetName(int n,int nonColRow,int fins)
        {
            var name = n switch
            {
                2 => "X-Wing",
                3 => "Swordfish",
                4 => "Jellyfish",
                5 => "Squirmbag",
                6 => "Whale",
                7 => "Leviathan",
                _ => "Fish"
            };
            if (nonColRow == 1)
                name = $"Franken {name}";
            if (nonColRow > 1)
                name = $"Mutant {name}";
            if(fins > 0)
                name = $"Finned {name}";
            return name;
        }
    }
}
