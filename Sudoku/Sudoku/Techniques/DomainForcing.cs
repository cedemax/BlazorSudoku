using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class DomainForcing : SudokuTechnique
    {
        public override string Name => $"Domain Forcing Mark Removal";

        public override int MinComplexity => 10;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit, bool hint = true)
        {
            if (complexityLimit < MinComplexity)
                return new();

            var moves = new List<SudokuMove>();
            foreach (var domainA in sudoku.UnsetDomains)
            {
                foreach (var domainB in sudoku.GetDomains(domainA.IntersectingDomains.Intersect(sudoku.UnsetDomainRefs))
                    .Where(x => 
                        x.Key < domainA.Key // avoid double-checking
                    ))
                {
                    var cellsAB = domainA.UnsetCellRefs.Intersect(domainB.UnsetCellRefs);   // The Cells in A ∩ B
                    if (cellsAB.IsAllFalse())
                        continue;
                    var cellsAnotB = domainA.UnsetCellRefs.Except(domainB.UnsetCellRefs);   // The Cells in A - B
                    var cellsBnotA = domainB.UnsetCellRefs.Except(domainA.UnsetCellRefs);   // The Cells in B - A

                    // the values that can be in AB
                    var valuesAB = sudoku.GetPossibleValues(cellsAB);                       // The values in A ∩ B
                    var valuesAnotB = sudoku.GetPossibleValues(cellsAnotB);                 // The values in A - B
                    var valuesBnotA = sudoku.GetPossibleValues(cellsBnotA);                 // The values in B - A
                    var removeAnotB = valuesAnotB.Intersect(valuesAB.Except(valuesBnotA));  // The values in A ∩ B that do not occur in B - A but occur in A - B    -> These can be removed from A - B
                    var removeBnotA = valuesBnotA.Intersect(valuesAB.Except(valuesAnotB));  // The values in A ∩ B that do not occur in A - B but occur in B - A    -> These can be removed from B - A
                    if (!removeAnotB.IsEmpty || !removeBnotA.IsEmpty)
                    {
                        var move = new SudokuMove("Domain forcing", MinComplexity);

                        if (!removeAnotB.IsEmpty)
                        {
                            foreach (var cell in sudoku.GetCells(cellsAnotB))
                                foreach (var value in cell.PossibleValues.Intersect(removeAnotB))
                                    move.Operations.Add(new SudokuAction(cell, SudokuActionType.RemoveOption, value, $"must be in the intersection of domains {domainA} and {domainB}"));
                        }

                        if (!removeBnotA.IsEmpty)
                        {
                            foreach (var cell in sudoku.GetCells(cellsBnotA))
                                foreach (var value in cell.PossibleValues.Intersect(removeBnotA))
                                    move.Operations.Add(new SudokuAction(cell, SudokuActionType.RemoveOption, value, $"must be in the intersection of domains {domainA} and {domainB}"));
                        }

                        if (!move.IsEmpty)
                        {
                            if (hint)
                            {
                                move.Hints.Add(new SudokuDomainHint(domainA, SudokuHint.Direct));
                                move.Hints.Add(new SudokuDomainHint(domainB, SudokuHint.Direct));
                            }

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
