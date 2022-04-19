namespace BlazorSudoku.Techniques
{
    public class Solver : SudokuTechnique
    {
        public override int MinComplexity => 10000;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit = int.MaxValue)
        {
            var ret = new List<SudokuMove>();
            foreach(var solver in GetAllTechiques().Where(x => x is not Solver).OrderBy(x => x.MinComplexity))
            {
                var remainingSpace = ret.Where(x => x.Complexity > solver.MinComplexity).Count();
                if (limit - remainingSpace == 0)
                    continue;

                var moves = solver.GetMoves(sudoku, limit - remainingSpace);
                ret.AddRange(moves);
            }
            return ret.OrderBy(x => x.Complexity).Take(limit).ToList();
        }
    }
}
