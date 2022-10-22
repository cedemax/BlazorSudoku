using System.Diagnostics;

namespace SudokuTests.Performance
{
    public partial class Performance
    {
        private static void Test<T>(string testFile, int N, int debug, int release) where T : SudokuTechnique, new()
        {
            var sudoku = Sudoku.Parse(File.ReadAllText($"SavedSudokus/{testFile}.sud"));

            var tech = new T();
            var timer = Stopwatch.StartNew();
            for (var i = 0; i < N; ++i)
            {
                var moves = tech.GetMoves(sudoku, int.MaxValue, int.MaxValue, false);
            }
            var time = timer.ElapsedMilliseconds;
#if DEBUG
            Assert.True(time < debug);
#else
            Assert.True(time < release);
#endif
        }

        private static void TestDry<T>(int N, int debug, int release) where T : SudokuTechnique, new()
        {
            Test<T>("AI_Escargot", N, debug, release);
        }
    }
}
