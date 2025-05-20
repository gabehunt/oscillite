using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Pressure500psiTraceScaleList : TraceScaleList
	{
		public Pressure500psiTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_PRESSURE_500, hasAutoScale: false)
		{
			Add(new TraceScale(50.0, TraceScale.Units.psi, probeGain));
			Add(new TraceScale(100.0, TraceScale.Units.psi, probeGain));
			Add(new TraceScale(250.0, TraceScale.Units.psi, probeGain));
			Add(new TraceScale(500.0, TraceScale.Units.psi, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
