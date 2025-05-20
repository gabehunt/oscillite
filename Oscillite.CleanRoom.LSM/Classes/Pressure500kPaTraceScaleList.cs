using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Pressure500kPaTraceScaleList : TraceScaleList
	{
		public Pressure500kPaTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_PRESSURE_500, hasAutoScale: false)
		{
			Add(new TraceScale(500.0, TraceScale.Units.kPa, probeGain));
			Add(new TraceScale(1000.0, TraceScale.Units.kPa, probeGain));
			Add(new TraceScale(2500.0, TraceScale.Units.kPa, probeGain));
			Add(new TraceScale(3500.0, TraceScale.Units.kPa, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
