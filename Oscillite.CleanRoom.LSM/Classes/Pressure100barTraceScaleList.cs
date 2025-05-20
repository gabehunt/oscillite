using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Pressure100barTraceScaleList : TraceScaleList
	{
		public Pressure100barTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_PRESSURE_100, hasAutoScale: false)
		{
			Add(new TraceScale(1.0, TraceScale.Units.bar, probeGain));
			Add(new TraceScale(2.0, TraceScale.Units.bar, probeGain));
			Add(new TraceScale(5.0, TraceScale.Units.bar, probeGain));
			Add(new TraceScale(10.0, TraceScale.Units.bar, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
