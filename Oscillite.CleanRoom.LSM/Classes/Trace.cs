using System;


namespace Oscillite.CleanRoom.LSM
{
	
public class Trace
    {
        public int Id { get; set; }

        public bool Enabled { get; set; }

        public bool Inverted { get; set; } = false;

        public TraceCouplingType Coupling { get; set; } = TraceCouplingType.Coupling_DC;

        public PeakDetectStatus PeakDetect { get; set; } = PeakDetectStatus.Off;

        public FilterState Filter { get; set; } = FilterState.Off;

        public TraceProbe Probe { get; set; } = new TraceProbe();

        public TraceScale Scale { get; set; } = new TraceScale() { FullScaleValue = 20, ProbeGain = 1, Unit = TraceScale.Units.V };

        public Position<double> Position { get; set; }

        public Position<double> RasterSpacing { get; set; }

        public Trigger AssociatedTrigger { get; set; } = null;

        public ThresholdSlope ThresholdSlope { get; set; }
    }
}
