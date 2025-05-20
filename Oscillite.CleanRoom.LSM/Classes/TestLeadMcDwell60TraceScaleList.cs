using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class TestLeadMcDwell60TraceScaleList : TraceScaleList
	{
		public TestLeadMcDwell60TraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_MIXTURE_CONTROL_DWELL_60, hasAutoScale: false)
		{
			Add(new TraceScale(20.0, TraceScale.Units.Degree, probeGain));
			Add(new TraceScale(40.0, TraceScale.Units.Degree, probeGain));
			Add(new TraceScale(60.0, TraceScale.Units.Degree, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
