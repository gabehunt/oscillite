using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class AmpScales : TraceScaleList
	{
		public AmpScales(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_SHUNT, hasAutoScale: true)
		{
			Add(new TraceScale(0.1, TraceScale.Units.mA, probeGain));
			Add(new TraceScale(0.2, TraceScale.Units.mA, probeGain));
			Add(new TraceScale(0.5, TraceScale.Units.mA, probeGain));
			Add(new TraceScale(1.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(2.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(5.0, TraceScale.Units.A, probeGain));
			Add(new TraceScale(10.0, TraceScale.Units.A, probeGain));
			base.SelectedScale = base[10.0];
		}
	}
}
