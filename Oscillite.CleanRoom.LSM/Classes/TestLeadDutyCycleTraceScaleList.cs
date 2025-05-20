using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class TestLeadDutyCycleTraceScaleList : TraceScaleList
	{
		public TestLeadDutyCycleTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_DUTY_CYCLE, hasAutoScale: false)
		{
			Add(new TraceScale(20.0, TraceScale.Units.Percent, probeGain));
			Add(new TraceScale(40.0, TraceScale.Units.Percent, probeGain));
			Add(new TraceScale(60.0, TraceScale.Units.Percent, probeGain));
			Add(new TraceScale(80.0, TraceScale.Units.Percent, probeGain));
			Add(new TraceScale(100.0, TraceScale.Units.Percent, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
