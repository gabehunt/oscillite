using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Pressure5000kPaTraceScaleList : TraceScaleList
	{
		public Pressure5000kPaTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_PRESSURE_5000, hasAutoScale: false)
		{
			Add(new TraceScale(5000.0, TraceScale.Units.MPa, probeGain));
			Add(new TraceScale(10000.0, TraceScale.Units.MPa, probeGain));
			Add(new TraceScale(25000.0, TraceScale.Units.MPa, probeGain));
			Add(new TraceScale(35000.0, TraceScale.Units.MPa, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
