using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Pressure500kgcm2TraceScaleList : TraceScaleList
	{
		public Pressure500kgcm2TraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_PRESSURE_500, hasAutoScale: false)
		{
			Add(new TraceScale(5.0, TraceScale.Units.kgcm2, probeGain));
			Add(new TraceScale(10.0, TraceScale.Units.kgcm2, probeGain));
			Add(new TraceScale(25.0, TraceScale.Units.kgcm2, probeGain));
			Add(new TraceScale(35.0, TraceScale.Units.kgcm2, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
