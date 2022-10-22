using BlazorSudoku.Techniques;
using System.Diagnostics;

namespace SudokuTests.Performance
{
    public partial class Performance
    {
        [Fact]
        public void OnlyOptionNoTickPerformance()
        {
            var sudoku = Sudoku.Parse(File.ReadAllText("SavedSudokus/NoTickPerf.sud"));

            var tech = new OnlyOptionNoTick();
            var timer = Stopwatch.StartNew();
            for(var i = 0; i < 10000; ++i)
            {
                var moves = tech.GetMoves(sudoku, 999999, 999999);
            }
            var time = timer.ElapsedMilliseconds;
#if DEBUG
            Assert.True(time < 100);
#else
            Assert.True(time < 200);
#endif

        }
    }
}
