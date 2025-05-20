using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Pressure5000psiTraceScaleList : TraceScaleList
	{
		public Pressure5000psiTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_PRESSURE_5000, hasAutoScale: false)
		{
			Add(new TraceScale(500.0, TraceScale.Units.psi, probeGain));
			Add(new TraceScale(1000.0, TraceScale.Units.psi, probeGain));
			Add(new TraceScale(2500.0, TraceScale.Units.psi, probeGain));
			Add(new TraceScale(5000.0, TraceScale.Units.psi, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
