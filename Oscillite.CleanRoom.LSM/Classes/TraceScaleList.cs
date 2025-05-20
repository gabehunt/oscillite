using System;
using System.Collections.Generic;
using System.Linq;

namespace Oscillite.CleanRoom.LSM
{
	
public class TraceScaleList : List<TraceScale>
    {
        public TraceScale SelectedScale { get; set; }
        public bool HasAutoScale { get; set; } = false;

        public TraceScale this[string name] => this.FirstOrDefault(scale => scale.Name == name);

        public TraceScale this[double fullScaleValue] => this.FirstOrDefault(scale => scale.FullScaleValue == fullScaleValue);

        public new TraceScale this[int index]
        {
            get => base[index];
            set => base[index] = value;
        }

        protected TraceScaleList(TraceProbeType probeType, bool hasAutoScale)
        {
            SelectedScale = new TraceScale();
            HasAutoScale = hasAutoScale;
        }
    }
}
