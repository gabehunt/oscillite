using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class TestLeadMcDwell90TraceScaleList : TraceScaleList
	{
		public TestLeadMcDwell90TraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_MIXTURE_CONTROL_DWELL_90, hasAutoScale: false)
		{
			Add(new TraceScale(30.0, TraceScale.Units.Degree, probeGain));
			Add(new TraceScale(60.0, TraceScale.Units.Degree, probeGain));
			Add(new TraceScale(90.0, TraceScale.Units.Degree, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
