using BlazorSudoku.Techniques;
using System.Reflection;

namespace BlazorSudoku
{
    public abstract class SudokuTechnique
    {
        public abstract int MinComplexity {get;}
        public SudokuMove? GetMove(Sudoku sudoku)
        {
            return GetMoves(sudoku, 1).FirstOrDefault();
        }

        public abstract List<SudokuMove> GetMoves(Sudoku sudoku,int limit = int.MaxValue, int complexityLimit = int.MaxValue);

        public SudokuMove? TrySetValue(Sudoku sudoku)
        {
            var md = GetMoves(sudoku, int.MaxValue)
                .SelectMany(x => x.Operations.Select(y => (y.Cell, x)))
                .GroupBy(x => x.Cell)
                .ToDictionary(x => x.Key, x => x.Select(y => y.x).Distinct().ToArray());
            foreach (var (cell, moves) in md)
            {
                var possible = cell.Domains.Select(x => x.Unset).Intersect().Intersect(cell.PossibleValues).ToHashSet();
                var movesUsed = new List<SudokuMove>();
                foreach (var move in moves)
                {
                    var moveUsed = false;
                    foreach (var op in move.Operations.Where(x => x.Cell == cell))
                    {
                        switch (op.Action)
                        {
                            case SudokuActionType.RemoveOption:
                                if (possible.Contains(op.Value))
                                {
                                    possible.Remove(op.Value);
                                    moveUsed = true;
                                }
                                break;
                            case SudokuActionType.SetValue:
                            case SudokuActionType.SetOnlyPossible:
                                if (!possible.Contains(op.Value))
                                    throw new Exception("Cant set impossible value");
                                possible.RemoveWhere(x => x != op.Value);
                                moveUsed = true;
                                break;
                        }
                    }
                    if (moveUsed)
                        movesUsed.Add(move);
                    if (possible.Count == 1)
                        break;
                }

                if (possible.Count == 1)
                {
                    var name = string.Join(" -> ", movesUsed.Select(x => x.Name));
                    var complexity = movesUsed.Sum(x => x.Complexity);
                    var move = new SudokuMove(name,complexity);
                    move.Operations.Add(new SudokuAction(cell, SudokuActionType.SetValue, possible.First(), ""));
                    foreach (var mv in movesUsed)
                        move.Hints.AddRange(mv.Hints);
                    return move;
                }
            }
            return null;
        }


        public static SudokuTechnique[] GetAllTechiques()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && (x.BaseType == typeof(SudokuTechnique) || x.BaseType.BaseType == typeof(SudokuTechnique))).Select(x => Activator.CreateInstance(x) as SudokuTechnique).ToArray();
        }
    }
}
