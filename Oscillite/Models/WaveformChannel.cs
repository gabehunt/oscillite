using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

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
        public List<VoltageRuler> Rulers { get; set; } = new List<VoltageRuler>();
        public float EffectiveVoltsPerDivision => (FullScale / 8f) / Scale;
        public float MaxExpectedVoltage { get; set; }
        public string Unit { get; internal set; } = "V";
        public int Index { get; internal set; }
        private string name;
        public string Name => name ?? (name = $"CH{Index + 1}");

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

        public (float Min, float Max)? LastVisibleRange { get; set; }
        public string LastAxisDebugInfo { get; internal set; }

        public (float Min, float Max) GetVisibleVoltageRange(int divisionsY = 8)
        {
            (float min, float max) result;

            if (ZoomedVoltageMin.HasValue && ZoomedVoltageMax.HasValue)
            {
                result = (ZoomedVoltageMin.Value, ZoomedVoltageMax.Value);
            }
            else
            {
                float range = EffectiveVoltsPerDivision * divisionsY;
                float mid = 0f;
                result = (mid - range / 2f, mid + range / 2f);
            }

            // ✅ Clamp min/max distance to avoid collapsing to 0
            const float minDelta = 0.001f;
            if (Math.Abs(result.max - result.min) < minDelta)
            {
                float center = (result.min + result.max) / 2f;
                result.min = center - minDelta / 2f;
                result.max = center + minDelta / 2f;
            }

            LastVisibleRange = result;
            return result;
        }

    }

}
