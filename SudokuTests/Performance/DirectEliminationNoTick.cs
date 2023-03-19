using BlazorSudoku.Techniques;
using System.Diagnostics;

namespace SudokuTests.Performance
{
    public partial class Performance
    {
        [Fact]
        public void DirectEliminationNoTickPerformance()
        {
            Test<DirectEliminationNoMarks>("2Star", 1000, 500, 300);
        }

        [Fact]
        public void DirectEliminationNoTickPerformance_Dry()
        {
            TestDry<DirectEliminationNoMarks>(1000, 300, 200);
        }
    }
}
