using BlazorSudoku.Techniques;
using BlazorSudoku.Techniques.Chain;
using System.Diagnostics;

namespace SudokuTests.Performance
{
    public partial class Performance
    {


        [Theory]
        [ClassData(typeof(PerfData))]
        public void Perf(PerfScenario scenario)
        {
            Test(scenario.Technique, scenario.FileName, scenario.Runs, scenario.Expected, scenario.Expected);
        }

        [Theory]
        [ClassData(typeof(PerfData))]
        public void PerfDry(PerfScenario scenario)
        {
            Test(scenario.Technique, "AI_Escargot", scenario.Runs, scenario.Expected, scenario.Expected);

        }
       

        private static void Test(SudokuTechnique tech,string testFile, int N, int debug, int release)
        {
            var sudoku = Sudoku.Parse(File.ReadAllText($"SavedSudokus/{testFile}.sud"));

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


        private static void Test<T>(string testFile, int N, int debug, int release) where T : SudokuTechnique, new()
        {
            Test(new T(), testFile, N, debug, release);
        }

        private static void TestDry<T>(int N, int debug, int release) where T : SudokuTechnique, new()
        {
            Test<T>("AI_Escargot", N, debug, release);
        }
    }
}
