using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class TestLeadPulseWidthTraceScaleList : TraceScaleList
	{
		public TestLeadPulseWidthTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_PULSE_WIDTH, hasAutoScale: false)
		{
			Add(new TraceScale(0.005, TraceScale.Units.ms, probeGain));
			Add(new TraceScale(0.01, TraceScale.Units.ms, probeGain));
			Add(new TraceScale(0.025, TraceScale.Units.ms, probeGain));
			Add(new TraceScale(0.05, TraceScale.Units.ms, probeGain));
			Add(new TraceScale(0.1, TraceScale.Units.ms, probeGain));
			Add(new TraceScale(0.25, TraceScale.Units.ms, probeGain));
			Add(new TraceScale(0.5, TraceScale.Units.ms, probeGain));
			Add(new TraceScale(2.0, TraceScale.Units.s, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
