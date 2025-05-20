using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class TemperatureFahrenheitTraceScaleList : TraceScaleList
	{
		public TemperatureFahrenheitTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_EEDM506D_TEMPERATURE, hasAutoScale: false)
		{
			Add(new TraceScale(100.0, TraceScale.Units.degF, probeGain));
			Add(new TraceScale(200.0, TraceScale.Units.degF, probeGain));
			Add(new TraceScale(500.0, TraceScale.Units.degF, probeGain));
			Add(new TraceScale(1000.0, TraceScale.Units.degF, probeGain));
			Add(new TraceScale(2000.0, TraceScale.Units.degF, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
