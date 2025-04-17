using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX;

namespace Oscillite
{
    public class WaveformChannel
    {
        public Vector2[] Data { get; set; }
        public RawColor4 Color { get; set; }
        public float VoltsPerDivision { get; set; }
        public float Offset { get; set; }
        public bool Visible { get; set; }
        public SolidColorBrush Brush { get; set; }

        public float FullScale { get; set; }
        public float Scale { get; set; } = 1.0f; // Zoom factor
        public List<Ruler> Rulers { get; set; } = new List<Ruler>();
        public float EffectiveVoltsPerDivision => (FullScale / 8f) / Scale;
        public float MaxExpectedVoltage { get; set; }
        public string Unit { get; internal set; } = "V";
        public int Index { get; internal set; }
        private string name;
        public string Name {
            get
            {
                if(name == null)
                {
                    name = (Index + 1).ToString();
                }
                        return name;
            }
            set
            {
            }
        }

        public float? ZoomedVoltageMin { get; set; } = null;
        public float? ZoomedVoltageMax { get; set; } = null;

        public WaveformChannel(int index)
        {
            Index = index;
            Visible = true;
            VoltsPerDivision = 1.0f;
            Scale = 1.0f;  // ✅ Initialize to neutral zoom
            Data = new Vector2[0];
        }
    }

}
