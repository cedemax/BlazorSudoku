using BlazorSudoku.Techniques;
using System.Diagnostics;

namespace SudokuTests.Performance
{
    public class Performance
    {
        [Fact]
        public void DirectEliminationNoTickPerformance()
        {
            var sudoku = Sudoku.Parse(File.ReadAllText("SavedSudokus/2Star.sud"));

            var tech = new DirectEliminationNoMarks();
            var timer = Stopwatch.StartNew();
            for(var i = 0; i < 100; ++i)
            {
                var moves = tech.GetMoves(sudoku, 999999, 999999);
            }
            var time = timer.ElapsedMilliseconds;
#if DEBUG
            Assert.True(time < 100);
#else
            Assert.True(time < 20);
#endif

        }
    }
}
