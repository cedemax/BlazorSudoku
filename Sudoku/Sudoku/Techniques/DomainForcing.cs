using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class DomainForcing : SudokuTechnique
    {
        public override int MinComplexity => 10;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit,int complexityLimit)
        {
            if (complexityLimit < MinComplexity)
                return new();

            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            foreach (var domainA in sudoku.UnsetDomains)
            {
                foreach (var domainB in domainA.IntersectingUnsetDomains)
                {
                    var cellsAB = sudoku.DomainIntersections[(domainA,domainB)];
                    var cellsAnotB = sudoku.DomainExceptions[(domainA, domainB)];
                    var cellsBnotA = sudoku.DomainExceptions[(domainB, domainA)];

                    if (cellsAB.Count >= 1 && cellsAnotB.Count >= 1 && cellsBnotA.Count >= 1)
                    {
                        // the values that can be in AB
                        var valuesAB = cellsAB.SelectMany(x => x.PossibleValues).ToHashSet();
                        var valuesAnotB = cellsAnotB.SelectMany(x => x.PossibleValues).ToHashSet();
                        var valuesBnotA = cellsBnotA.SelectMany(x => x.PossibleValues).ToHashSet();

                        var move = new SudokuMove("Domain forcing", MinComplexity);

                        foreach (var value in valuesAB)
                        {
                            // value is exclusive to AB
                            if (!valuesAnotB.Contains(value))
                            {
                                foreach (var cell in cellsBnotA)
                                {
                                    if (cell.IsSet || done.Contains((cell, value)) || !cell.PossibleValues.Contains(value))
                                        continue;
                                    move.Operations.Add(new SudokuAction(cell, SudokuActionType.RemoveOption, value, $"must be in the intersection of domains {domainA} and {domainB}"));
                                    done.Add((cell, value));
                                }
                            }
                            // value is exclusive to AB
                            if (!valuesBnotA.Contains(value))
                            {
                                foreach (var cell in cellsAnotB)
                                {
                                    if (cell.IsSet || done.Contains((cell, value)) || !cell.PossibleValues.Contains(value))
                                        continue;
                                    move.Operations.Add(new SudokuAction(cell, SudokuActionType.RemoveOption, value, $"must be in the intersection of domains {domainA} and {domainB}"));
                                    done.Add((cell, value));
                                }
                            }
                        }
                        if (!move.IsEmpty)
                        {
                            move.Hints.Add(new SudokuDomainHint(domainA, SudokuHint.Direct));
                            move.Hints.Add(new SudokuDomainHint(domainB, SudokuHint.Direct));

                            moves.Add(move);
                            if (moves.Count >= limit)
                                return moves;
                        }
                    }
                }
              
            }
            return moves;
        }

    }
}
