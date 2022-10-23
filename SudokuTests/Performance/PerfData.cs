using BlazorSudoku.Techniques;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace SudokuTests.Performance
{
    public class PerfScenario : IXunitSerializable
    {
        public SudokuTechnique Technique { get; set; }
        public string FileName { get; set; }
        public int Runs { get; set; }
        public int Expected { get; set; }

        public override string ToString()
        {
            return Technique.Name;
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Technique = SudokuTechnique.Deserialize(info.GetValue<string>("Technique"));
            FileName = info.GetValue<string>("FileName");
            Runs = info.GetValue<int>("Runs");
            Expected = info.GetValue<int>("Expected");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("Technique", Technique.Serialize);
            info.AddValue("FileName", FileName);
            info.AddValue("Runs", Runs);
            info.AddValue("Expected", Expected);
        }
        public PerfScenario() { }
        public PerfScenario(SudokuTechnique technique, string fileName, int runs, int expected)
        {
            Technique = technique;
            FileName = fileName;
            Runs = runs;
            Expected = expected;
        }   
    }

    public class PerfData : IEnumerable<object[]>
    {
        public static PerfScenario[] Data => new PerfScenario[]
        {
            new PerfScenario(new DirectEliminationNoMarks(),"2Star",1000,80 ),
            new PerfScenario(new OnlyOptionNoTick(),"NoTickPerf",1000,20 ),
            new PerfScenario(new SelectOnlies(),"3Star",1000,20 ),
            new PerfScenario(new EliminateDirect(),"3Star",1000,80 ),
            new PerfScenario(new OnlyOption(),"OnlyOptionTest",1000,60 ),
            new PerfScenario(new DomainForcing(),"DomainForcingTest",1000,300 ),
            new PerfScenario(new Fish(2,false,false),"XWingTest",1000,80 ),
            new PerfScenario(new Fish(3,false,false), "SwordfishTest", 1000,80 ),
            new PerfScenario(new Fish(4, false, false), "JellyfishTest", 1000,80 ),
            new PerfScenario(new Fish(2, true, false), "Finned X-Wing", 1000,80 ),
            new PerfScenario(new Fish(3, true, false), "Finned X-Wing", 1000,80 ),
            new PerfScenario(new Fish(4, true, false), "Finned X-Wing", 1000,80 ),
            new PerfScenario(new Fish(2, true, true), "Finned X-Wing", 1000,80 ),
            new PerfScenario(new Fish(3, true, true), "Finned X-Wing", 1000,80 ),
            new PerfScenario(new Fish(4, true, true), "Finned X-Wing", 1000,80 ),
            new PerfScenario(new Fish(), "Finned X-Wing", 1000,80 ),
            new PerfScenario(new HiddenGroup(2), "4Star", 1000,80 ),
            new PerfScenario(new HiddenGroup(3), "4Star", 1000,80 ),
            new PerfScenario(new HiddenGroup(4), "4Star", 1000,80 ),
            new PerfScenario(new NakedGroup(2), "NakedPairTest", 1000,20 ),
            new PerfScenario(new NakedGroup(3), "NakedTripletTest", 1000,200 ),
            new PerfScenario(new NakedGroup(4), "NakedQuadTest", 1000,280 ),
            new PerfScenario(new SimpleColor(), "SCTest", 1000,80 ),
            new PerfScenario(new MultiColor(), "MC1", 1000,80 ),
            new PerfScenario(new XYWing(), "Hard", 1000,80 ),
            new PerfScenario(new XYZWing(), "Hard", 1000,80 ),
            new PerfScenario(new XYChain(), "XYChainTest", 1000,80 ),
            new PerfScenario(new ContradictionChain(), "Disc Nice Loop", 1000,80)
       };

        public IEnumerator<object[]> GetEnumerator()
        {
            foreach(var data in Data)
                yield return new object[] { data };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
