using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Pressure5000kgcm2TraceScaleList : TraceScaleList
	{
		public Pressure5000kgcm2TraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_PRESSURE_5000, hasAutoScale: false)
		{
			Add(new TraceScale(50.0, TraceScale.Units.kgcm2, probeGain));
			Add(new TraceScale(100.0, TraceScale.Units.kgcm2, probeGain));
			Add(new TraceScale(250.0, TraceScale.Units.kgcm2, probeGain));
			Add(new TraceScale(350.0, TraceScale.Units.kgcm2, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
