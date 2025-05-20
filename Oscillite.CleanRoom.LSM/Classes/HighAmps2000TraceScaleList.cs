using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class HighAmps2000TraceScaleList : TraceScaleList
	{
		public HighAmps2000TraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_HIGH_AMPS_2000, hasAutoScale: true)
		{
			Add(new TraceScale(100.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(200.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(500.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(1000.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(2000.0, TraceScale.Units.A, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
