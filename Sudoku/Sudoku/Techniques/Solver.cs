﻿namespace BlazorSudoku.Techniques
{
    public class Solver : SudokuTechnique
    {
        private List<SudokuTechnique> techs;

        public Solver() : this(Array.Empty<SudokuTechnique>()) { }
        public Solver(IEnumerable<SudokuTechnique> techs)
        {
            this.techs = techs.OrderBy(x => x.MinComplexity).ToList();

        }

        public override int MinComplexity => 10000;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit = int.MaxValue)
        {
            var ret = new List<SudokuMove>();
            if (techs.Count == 0)
                techs = GetAllTechiques().Where(x => x is not Solver).OrderBy(x => x.MinComplexity).ToList();

            foreach(var solver in techs)
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
