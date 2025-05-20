using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class LowAmps20TraceScaleList : TraceScaleList
	{
		public LowAmps20TraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_LOW_AMPS_20, hasAutoScale: true)
		{
			Add(new TraceScale(0.5, TraceScale.Units.mA, probeGain));
			Add(new TraceScale(1.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(2.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(5.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(10.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(20.0, TraceScale.Units.A, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
