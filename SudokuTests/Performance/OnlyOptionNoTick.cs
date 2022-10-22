using BlazorSudoku.Techniques;
using System.Diagnostics;

namespace SudokuTests.Performance
{
    public partial class Performance
    {
        [Fact]
        public void OnlyOptionNoTickPerformance()
        {
            Test<EliminateDirect>("NoTickPerf", 1000, 100, 70);
        }

        [Fact]
        public void OnlyOptionNoTickPerformance_Dry()
        {
            TestDry<EliminateDirect>(1000, 30, 40);
        }
    }
}
