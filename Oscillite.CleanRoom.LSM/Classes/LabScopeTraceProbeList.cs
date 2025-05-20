using System;
using System.Collections.Generic;

namespace Oscillite.CleanRoom.LSM
{
	
	public class LabScopeTraceProbeList : TraceProbeList
	{
		public LabScopeTraceProbeList(PressureUnits pressureUnits = PressureUnits.psi, VacuumUnits vacuumUnits = VacuumUnits.inHg, TemperatureUnits temperatureUnits = TemperatureUnits.degF)
		{
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_VOLTS));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_VACUUM_100, vacuumUnits));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_PRESSURE_100, pressureUnits));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_PRESSURE_500, pressureUnits));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_PRESSURE_5000, pressureUnits));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_LOW_AMPS_20));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_LOW_AMPS_40));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_LOW_AMPS_60));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_HIGH_AMPS_200));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_HIGH_AMPS_2000));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_EEDM506D_TEMPERATURE, temperatureUnits));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_MT5030_VACUUM, vacuumUnits));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_MT5030_PRESSURE, pressureUnits));
		}
	}
}
