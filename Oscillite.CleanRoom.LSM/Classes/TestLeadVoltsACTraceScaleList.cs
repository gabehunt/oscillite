using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class TestLeadVoltsACTraceScaleList : TraceScaleList
	{
		public TestLeadVoltsACTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_VOLTS_AC, hasAutoScale: true)
		{
			Add(new TraceScale(0.1, TraceScale.Units.mV, probeGain));
			Add(new TraceScale(0.2, TraceScale.Units.mV, probeGain));
			Add(new TraceScale(0.5, TraceScale.Units.mV, probeGain));
			Add(new TraceScale(1.0, TraceScale.Units.V, probeGain));
			Add(new TraceScale(2.0, TraceScale.Units.V, probeGain));
			Add(new TraceScale(5.0, TraceScale.Units.V, probeGain));
			Add(new TraceScale(10.0, TraceScale.Units.V, probeGain));
			Add(new TraceScale(20.0, TraceScale.Units.V, probeGain));
			Add(new TraceScale(50.0, TraceScale.Units.V, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
