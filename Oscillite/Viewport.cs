using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oscillite
{
    // Step 1: Core structure of a modern Viewport-based oscilloscope viewer

    public class Viewport
    {
        public float TimeStart { get; set; }
        public float TimeEnd { get; set; }

        // Per-channel vertical zoom
        public Dictionary<int, (float VoltageMin, float VoltageMax)> ChannelVoltageRanges = new Dictionary<int, (float VoltageMin, float VoltageMax)>();

        public float TimeSpan => TimeEnd - TimeStart;

        public bool HasZoom => TimeEnd > TimeStart && ChannelVoltageRanges.Count > 0;

        public void ResetZoom()
        {
            TimeStart = 0f;
            TimeEnd = 10f;
            ChannelVoltageRanges.Clear();
        }

        public void SetChannelVoltageRange(int channelIndex, float min, float max)
        {
            ChannelVoltageRanges[channelIndex] = (min, max);
        }

        public bool TryGetVoltageRange(int channelIndex, out float min, out float max)
        {
            if (ChannelVoltageRanges.TryGetValue(channelIndex, out var range))
            {
                min = range.VoltageMin;
                max = range.VoltageMax;
                return true;
            }
            min = max = 0f;
            return false;
        }
    }
}
