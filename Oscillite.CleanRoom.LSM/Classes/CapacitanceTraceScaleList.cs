using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class CapacitanceTraceScaleList : TraceScaleList
	{
		public CapacitanceTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_CAPACITANCE, hasAutoScale: true)
		{
			Add(new TraceScale(4E-08, TraceScale.Units.nF, probeGain));
			Add(new TraceScale(4E-07, TraceScale.Units.nF, probeGain));
			Add(new TraceScale(4E-06, TraceScale.Units.uF, probeGain));
			Add(new TraceScale(4E-05, TraceScale.Units.uF, probeGain));
			Add(new TraceScale(0.0004, TraceScale.Units.uF, probeGain));
			Add(new TraceScale(0.004, TraceScale.Units.mF, probeGain));
			Add(new TraceScale(0.01, TraceScale.Units.mF, probeGain));
			base.SelectedScale = base[base.Count -1];
		}
	}
}
