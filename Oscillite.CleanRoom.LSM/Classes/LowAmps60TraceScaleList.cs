using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class LowAmps60TraceScaleList : TraceScaleList
	{
		public LowAmps60TraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_LOW_AMPS_60, hasAutoScale: true)
		{
			Add(new TraceScale(10.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(20.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(40.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(60.0, TraceScale.Units.A, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
