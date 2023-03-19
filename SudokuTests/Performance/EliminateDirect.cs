using BlazorSudoku.Techniques;
using System.Diagnostics;

namespace SudokuTests.Performance
{
    public partial class Performance
    {
        [Fact]
        public void EliminateDirectPerformance()
        {
            Test<EliminateDirect>("3Star", 1000, 100, 80);
        }

        [Fact]
        public void EliminateDirectPerformance_Dry()
        {
            TestDry<EliminateDirect>(1000, 60, 40);
        }
    }
}
