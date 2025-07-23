using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.Helpers
{
    public class SKMouseWheelEventArgs
    {
        public int Delta { get; }
        public SKPoint Location { get; }

        public SKMouseWheelEventArgs(int delta, SKPoint location)
        {
            Delta = delta;
            Location = location;
        }
    }
}
