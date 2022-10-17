using BlazorSudoku.Hints;
using System.Buffers;

namespace BlazorSudoku.Techniques
{
    public class Fish : SudokuTechnique
    {
        private readonly int maxGroupSize;
        private readonly bool allowFins;
        private readonly bool allowMutant;

        public Fish() { maxGroupSize = 4; allowFins = true; allowMutant = true; }
        public Fish(int maxGroupSize,bool fins,bool nonColRow)
        {
            if (maxGroupSize < 2)
                throw new ArgumentOutOfRangeException(nameof(maxGroupSize));
            this.maxGroupSize = maxGroupSize;
            this.maxGroupSize = maxGroupSize;
            this.allowFins = fins;
            this.allowMutant = nonColRow;
        }

        public override int MinComplexity => 60;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit,int complexityLimit)
        {
            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            var counts = new Dictionary<int, int>();
            var bcounts = new Dictionary<int, int>();

            var viableDomains = sudoku.UnsetDomains.Where(x => x.Set.Count > 0).ToArray();

            var remainingDigits = sudoku.UnsetDomains.SelectMany(x => x.Unset).ToHashSet();
            for (var n = 2; n <= maxGroupSize; ++n)
            {
                counts[n] = 0;
                bcounts[n] = 0;
            }
         
            foreach (var fishDigit in remainingDigits)
            {
                var digitViableDomains = viableDomains.Where(x => x.Unset.Contains(fishDigit)).ToArray();
                for (var n = 2; n <= maxGroupSize; ++n)
                {
                    var complexity = GetComplexity(n, 0, 0);
                    if (complexityLimit < complexity)
                        continue;
                    var count = 0;
                    var bcount = 0;

                    var baseCandidates = new SudokuCell[n * sudoku.N];
                    var fishEliminations = new SudokuCell[n * sudoku.N];
                    var fins = new SudokuCell[n * sudoku.N];
                    var finEliminations = new SudokuCell[n * sudoku.N];

                    foreach (var bases in sudoku.GetNonOverlappingSets(n, digitViableDomains))
                    {
                        bcount++;
                        //var fishDigitsBase = bases.Select(x => x.Unset).Intersect();
                        //if (fishDigitsBase.Count == 0)
                        //    continue;

                        var baseCandidatesCount = 0;
                        foreach (var b in bases)
                            foreach (var cell in b.Cells)
                                if(cell.PossibleValues.Contains(fishDigit))
                                    baseCandidates[baseCandidatesCount++] = cell;


                        var coverAlternatives = digitViableDomains.Where(y => !bases.Contains(y) && y.Overlaps(bases)).ToArray();

                        var maxFinsRemoved = coverAlternatives.ToDictionary(x => x,x => x.Cells.Intersect(baseCandidates.Take(baseCandidatesCount)).Count());

                        var topNminus1 = maxFinsRemoved.Values.OrderBy(x => -x).Take(n - 1).Sum();
                        // eliminate all covers that cannot possibly provide sufficient cover
                        coverAlternatives = coverAlternatives.Where(x => (baseCandidatesCount - (maxFinsRemoved[x] + topNminus1)) <= 3).ToArray();

                        foreach (var covers in sudoku.GetNonOverlappingSets(n, coverAlternatives))
                        {
                            // eliminate all covers that cannot possibly provide sufficient cover
                            if (baseCandidatesCount - covers.Select(x => maxFinsRemoved[x]).Sum() > 3)
                                continue;

                            //var fishDigits = covers.Select(x => x.Unset).Intersect();
                            //fishDigits.IntersectWith(fishDigitsBase);
                            //if (fishDigits.Count == 0)
                            //    continue;

                            var nonColRow = bases.Concat(covers).Count(x => !x.IsColOrRow);

                            // check if we should continue or not
                            if (!allowMutant && nonColRow > 0)
                                continue;

                            // update complexity check
                            complexity = GetComplexity(n, nonColRow, 0);
                            if (complexityLimit < complexity)
                                continue;

                            var fishEliminationsCount = 0;
                            foreach (var b in covers)
                                foreach (var cell in b.Cells)
                                    if (cell.PossibleValues.Contains(fishDigit) && !AnyRefIntersects(cell.Domains, bases))
                                        fishEliminations[fishEliminationsCount++] = cell;

                            if (fishEliminationsCount == 0)
                                continue;

                            count++;

                            var finCount = 0;
                            for (var i = 0; i < baseCandidatesCount; ++i)
                                if (!AnyRefIntersects(baseCandidates[i].Domains, covers))
                                    fins[finCount++] = baseCandidates[i];
                            
                            // check if we should continue or not
                            if (!allowFins && finCount > 0)
                                continue;

                            // unrealistic number of fins
                            if (finCount > 3)
                                continue;

                            if (finCount > 0)
                            {
                                // update complexity check
                                if (complexityLimit < GetComplexity(n, nonColRow, finCount))
                                    continue;

                                var finEliminationsCount = 0;
                                for (var i = 0; i < fishEliminationsCount; ++i)
                                {
                                    var all = true;
                                    for (var j = 0; j < finCount; ++j)
                                    {
                                        if (!fins[j].VisibleUnset.Contains(fishEliminations[i]))
                                        {
                                            all = false;
                                            break;
                                        }
                                    }
                                    if (all)
                                        finEliminations[finEliminationsCount++] = fishEliminations[i];
                                }
                                if (finEliminationsCount == 0)
                                    continue;

                                fishEliminations = finEliminations;
                                fishEliminationsCount = finEliminationsCount;
                            }

                            // it is a fish
                            var move = new SudokuMove(GetName(n, nonColRow, finCount), GetComplexity(n, nonColRow, finCount));

                            for (var i = 0; i < fishEliminationsCount; ++i)
                                move.Operations.Add(new SudokuAction(fishEliminations[i], SudokuActionType.RemoveOption, fishDigit, "eliminated by fish"));

                            if (!move.IsEmpty)
                            {
                                foreach (var bd in bases)
                                    move.Hints.Add(new SudokuDomainHint(bd, SudokuHint.Base));
                                foreach (var cd in covers)
                                    move.Hints.Add(new SudokuDomainHint(cd, SudokuHint.Cover));
                                foreach (var bc in bases.SelectMany(x => x.Cells).Intersect(covers.SelectMany(x => x.Cells)).Where(x => x.PossibleValues.Contains(fishDigit)))
                                    move.Hints.Add(new SudokuCellOptionHint(bc, fishDigit, SudokuHint.Direct));

                                for (var i = 0; i < finCount; ++i)
                                    move.Hints.Add(new SudokuCellOptionHint(fins[i], fishDigit, SudokuHint.Fin));

                                moves.Add(move);
                                if (moves.Count >= limit)
                                    return moves;
                            }

                            //}
                        }


                    }
                    bcounts[n] += bcount;
                    counts[n] += count;
                }
                
            }
            return moves;
        }


        private static bool AnyRefIntersects<T>(IEnumerable<T> A,IEnumerable<T> B)
        {
            foreach (var a in A)
                foreach (var b in B)
                    if (object.ReferenceEquals(a,b))
                        return true;
            return false;
        }

        private int GetComplexity(int n, int nonColRow, int fins) => (int)Math.Round((n * n) * (1 + Math.Sqrt(nonColRow)) * (1 + Math.Sqrt(fins)))* MinComplexity/4;

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
            {
                name = $"Finned {name}";

            }
            return name;
        }
    }
}
