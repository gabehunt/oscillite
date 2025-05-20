using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class HighAmps200TraceScaleList : TraceScaleList
	{
		public HighAmps200TraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_HIGH_AMPS_200, hasAutoScale: true)
		{
			Add(new TraceScale(10.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(20.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(50.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(100.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(200.0, TraceScale.Units.A, probeGain));
            base.SelectedScale = base[base.Count - 1];
        }
	}
}
