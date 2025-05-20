using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Pressure100psiTraceScaleList : TraceScaleList
	{
		public Pressure100psiTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_PRESSURE_100, hasAutoScale: false)
		{
			Add(new TraceScale(10.0, TraceScale.Units.psi, probeGain));
			Add(new TraceScale(25.0, TraceScale.Units.psi, probeGain));
			Add(new TraceScale(50.0, TraceScale.Units.psi, probeGain));
			Add(new TraceScale(100.0, TraceScale.Units.psi, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
