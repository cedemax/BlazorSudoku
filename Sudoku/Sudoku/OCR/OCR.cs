using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSudoku { 
    public class OCRResult
    {
        public List<OCRCell> Result { get; set; }
    }
    public class OCRCell
    {
        public string Value { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }
}
