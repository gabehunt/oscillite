using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Oscillite
{
    public class Axis
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public int Divisions { get; set; }
        public float Position { get; set; }  // X position for Y-axes, Y position for X-axis
        public bool IsTimeAxis { get; set; }
        public Color4 Color { get; set; }
        public string Units { get; set; }

        public Axis()
        {
            Divisions = 10;
            Color = new Color4(1.0f, 1.0f, 1.0f, 1.0f); // White
        }
    }

}
