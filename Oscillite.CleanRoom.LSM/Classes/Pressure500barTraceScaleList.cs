using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Pressure500barTraceScaleList : TraceScaleList
	{
		public Pressure500barTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_PRESSURE_500, hasAutoScale: false)
		{
			Add(new TraceScale(5.0, TraceScale.Units.bar, probeGain));
			Add(new TraceScale(10.0, TraceScale.Units.bar, probeGain));
			Add(new TraceScale(25.0, TraceScale.Units.bar, probeGain));
			Add(new TraceScale(35.0, TraceScale.Units.bar, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
