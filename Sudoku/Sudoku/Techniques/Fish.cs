﻿using BlazorSudoku.Hints;
using System.Buffers;

namespace BlazorSudoku.Techniques
{
    public class Fish : SudokuTechnique
    {
        private readonly int maxGroupSize;
        private readonly bool allowFins;
        private readonly bool allowMutant;

        public override string Name => $"Fish up to: {GetName(maxGroupSize,allowMutant?3:0,allowFins?3:0)}";
        public override string Serialize => $"{base.Serialize}{maxGroupSize}|{allowFins}|{allowMutant}";

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
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit, bool hint = true)
        {
            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            var counts = new Dictionary<int, int>();
            var bcounts = new Dictionary<int, int>();

            var viableDomains = sudoku.UnsetDomains.Where(x => 
                x.Set.Count > 0 && x.Unset.Count >= 2 &&
                (allowMutant || x.IsColOrRow)
            ).ToArray();
            var remainingDigits = sudoku.UnsetDomains.Select(x => x.Unset).Union();

            for (var n = 2; n <= maxGroupSize; ++n)
            {
                counts[n] = 0;
                bcounts[n] = 0;
            }
         
            foreach (var fishDigit in remainingDigits)
            {
                var digitViableDomains = viableDomains.Where(x => x.PossibleValueRefs[fishDigit].CountTrue() >= 2).ToArray();
                if (digitViableDomains.Length == 0)
                    continue;

                // all cells that have the digit.
                var digitRefs = digitViableDomains.Select(x => x.PossibleValueRefs[fishDigit]).Union();    

                for (var n = 2; n <= maxGroupSize; ++n)
                {
                    if (complexityLimit < GetComplexity(n, 0, 0))
                        continue;
                    var count = 0;
                    var bcount = 0;

                    //var baseCandidates = new SudokuCell[n * sudoku.N];
                    //var fishEliminations = new SudokuCell[n * sudoku.N];
                    //var fins = new SudokuCell[n * sudoku.N];
                    //var finEliminations = new SudokuCell[n * sudoku.N];
                    var coverAlternatives = new SudokuDomain[digitViableDomains.Length];
                    var coverAlternatives2 = new SudokuDomain[digitViableDomains.Length];

                    foreach (var bases in Sudoku.GetNonOverlappingSets(n, digitViableDomains))
                    {
                        bcount++;

                        var baseCandidateRefs = bases.Select(x => x.PossibleValueRefs[fishDigit]).Union();
                        var baseCandidatesCount = baseCandidateRefs.CountTrue();

                        // not sure if this can ever happen
                        if (baseCandidatesCount < n*2)
                            continue;

                        var coverAlternativesCount = digitViableDomains.Where(y => y.Overlaps(bases)).CopyTo(coverAlternatives);

                        //coverAlternatives = digitViableDomains.Where(y => !bases.Contains(y) && y.Overlaps(bases)).ToArray();
                        var cas = coverAlternatives.Take(coverAlternativesCount);

                        foreach (var ca in cas)
                            ca.TempField = ca.Cells.Intersect(baseCandidateRefs).CountTrue();   // the amount of cover provided by this covering set

                        var topNminus1 = cas.OrderByDescending(x => x.TempField).Take(n - 1).Sum(x => x.TempField);

                        // eliminate all covers that cannot possibly provide sufficient cover
                        var coverAlternativesCount2 = coverAlternatives.Take(coverAlternativesCount).Where(x => x.TempField >= 2 && (baseCandidatesCount - (x.TempField + topNminus1)) <= 3).CopyTo(coverAlternatives2);
                        cas = coverAlternatives2.Take(coverAlternativesCount2);

                        foreach (var covers in Sudoku.GetNonOverlappingSets(n, cas, baseCandidatesCount-3))
                        {
                            // eliminate all covers that cannot possibly provide sufficient cover
                            if (baseCandidatesCount - covers.Sum(x => x.TempField) > 3)
                                continue;


                            var nonColRow = bases.Concat(covers).Count(x => !x.IsColOrRow);

                            // check if we should continue or not
                            if (!allowMutant && nonColRow > 0)
                                continue;

                            // update complexity check
                            if (complexityLimit < int.MaxValue && complexityLimit < GetComplexity(n, nonColRow, 0))
                                continue;

                            var coverCellRefs = covers.Select(x => x.UnsetCellRefs).Union();
                            var fishEliminationRefs = coverCellRefs.Intersect(digitRefs).Except(baseCandidateRefs);

                            if (fishEliminationRefs.CountTrue() == 0)
                                continue;

                            count++;

                            var finRefs = baseCandidateRefs.Except(coverCellRefs);
                            var finCount = finRefs.CountTrue();

                            // check if we should continue or not
                            if (!allowFins && finCount > 0)
                                continue;

                            // unrealistic number of fins
                            if (finCount > 3)
                                continue;


                            if (finCount > 0)
                            {

                                // update complexity check
                                if (complexityLimit < int.MaxValue && complexityLimit < GetComplexity(n, nonColRow, finCount))
                                    continue;

                                var finEliminationRefs = BAPool.Get<SudokuCell>(fishEliminationRefs.Count);

                                var finEliminationsCount = 0;
                                foreach(var fishElimination in sudoku.GetCells(fishEliminationRefs))
                                {
                                    var all = true;
                                    foreach(var fin in sudoku.GetCells(finRefs))
                                    {
                                        if (!fin.VisibleUnset.Contains(fishElimination))
                                        {
                                            all = false;
                                            break;
                                        }
                                    }
                                    if (all)
                                    {
                                        finEliminationsCount++;
                                        finEliminationRefs.Add(fishElimination);
                                    }
                                }
                                // not possible to eliminate fins
                                if (finEliminationsCount == 0)
                                    continue;

                                fishEliminationRefs = finEliminationRefs;
                            }

                            // it is a fish
                            var move = new SudokuMove(GetName(n, nonColRow, finCount), GetComplexity(n, nonColRow, finCount));

                            foreach(var fishElimination in sudoku.GetCells(fishEliminationRefs))
                                    move.Operations.Add(new SudokuAction(fishElimination, SudokuActionType.RemoveOption, fishDigit, "eliminated by fish"));

                            if (!move.IsEmpty)
                            {
                                if (hint)
                                {
                                    foreach (var bd in bases)
                                        move.Hints.Add(new SudokuDomainHint(bd, SudokuHint.Base));
                                    foreach (var cd in covers)
                                        move.Hints.Add(new SudokuDomainHint(cd, SudokuHint.Cover));
                                    foreach (var bc in bases.SelectMany(x => x.Cells).Intersect(covers.SelectMany(x => x.Cells)).Where(x => x.PossibleValues.Contains(fishDigit)))
                                        move.Hints.Add(new SudokuCellOptionHint(bc, fishDigit, SudokuHint.Direct));

                                    foreach(var fin in sudoku.GetCells(finRefs))
                                        move.Hints.Add(new SudokuCellOptionHint(fin, fishDigit, SudokuHint.Fin));
                                }

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