using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Pressure100kPaTraceScaleList : TraceScaleList
	{
		public Pressure100kPaTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_PRESSURE_100, hasAutoScale: false)
		{
			Add(new TraceScale(100.0, TraceScale.Units.kPa, probeGain));
			Add(new TraceScale(200.0, TraceScale.Units.kPa, probeGain));
			Add(new TraceScale(500.0, TraceScale.Units.kPa, probeGain));
			Add(new TraceScale(700.0, TraceScale.Units.kPa, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
