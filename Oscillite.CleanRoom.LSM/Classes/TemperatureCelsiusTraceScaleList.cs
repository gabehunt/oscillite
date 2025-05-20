using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class TemperatureCelsiusTraceScaleList : TraceScaleList
	{
		public TemperatureCelsiusTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_EEDM506D_TEMPERATURE, hasAutoScale: false)
		{
			Add(new TraceScale(50.0, TraceScale.Units.degC, probeGain));
			Add(new TraceScale(100.0, TraceScale.Units.degC, probeGain));
			Add(new TraceScale(200.0, TraceScale.Units.degC, probeGain));
			Add(new TraceScale(500.0, TraceScale.Units.degC, probeGain));
			Add(new TraceScale(1000.0, TraceScale.Units.degC, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
