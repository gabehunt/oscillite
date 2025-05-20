using System;

using Oscillite.CleanRoom.LSM.Properties;


namespace Oscillite.CleanRoom.LSM
{
	
public class TraceProbe
    {
        public TraceProbeType Type { get; set; } = TraceProbeType.TRACE_PROBE_VOLTS;
        public string Name { get; set; }
        public TraceScaleList TraceScaleList { get; set; }
        public TraceScale SelectedScale
        {
            get => TraceScaleList.SelectedScale;
            set => TraceScaleList.SelectedScale = value;
        }
        public float Gain { get; set; } = 1f;
        public float Offset { get; set; } = 0f;

        public VacuumUnits VacuumUnits = VacuumUnits.mmHg;
        public PressureUnits PressureUnits;
        public TemperatureUnits TemperatureUnits = TemperatureUnits.degF;

        public bool IsSupported { get; set; }
        public bool IsCalibrationSupported { get; set; }

        public TraceProbe()
        {
            Gain = 1f;
            Offset = 0f;
            TraceProbeFactory.ConfigureProbe(this, PressureUnits.psi, TemperatureUnits.degC, VacuumUnits.mmHg);
        }

        public TraceProbe(TraceProbeType type)
        {
            Gain = 1f;
            Offset = 0f;
            Type = type;
            TraceProbeFactory.ConfigureProbe(this, PressureUnits.psi, TemperatureUnits.degC, VacuumUnits.mmHg);
        }

        public TraceProbe(TraceProbeType type, PressureUnits pressureUnits)
        {
            Gain = 1f;
            Offset = 0f;
            Type = type;
            TraceProbeFactory.ConfigureProbe(this, pressureUnits, TemperatureUnits.degC, VacuumUnits.mmHg);
        }

        public TraceProbe(TraceProbeType type, VacuumUnits vacuumUnits)
        {
            Gain = 1f;
            Offset = 0f;
            Type = type;
            TraceProbeFactory.ConfigureProbe(this, PressureUnits.psi, TemperatureUnits.degC, vacuumUnits);
        }

        public TraceProbe(TraceProbeType type, TemperatureUnits temperatureUnits)
        {
            Type = type;
            Gain = 1f;
            Offset = 0f;
            TraceProbeFactory.ConfigureProbe(this, PressureUnits.psi, temperatureUnits, VacuumUnits.mmHg);
        }

        public bool IsCalculatedProbe()
        {
            return Type == TraceProbeType.TRACE_PROBE_DUTY_CYCLE ||
                   Type == TraceProbeType.TRACE_PROBE_FREQUENCY ||
                   Type == TraceProbeType.TRACE_PROBE_INJECTOR_PULSE_WIDTH ||
                   Type == TraceProbeType.TRACE_PROBE_MIXTURE_CONTROL_DWELL_60 ||
                   Type == TraceProbeType.TRACE_PROBE_MIXTURE_CONTROL_DWELL_90 ||
                   Type == TraceProbeType.TRACE_PROBE_PULSE_WIDTH;
        }

        public bool IsPressureProbe(TraceProbeType probeType)
        {
            return probeType == TraceProbeType.TRACE_PROBE_PRESSURE_100 ||
                   probeType == TraceProbeType.TRACE_PROBE_PRESSURE_500 ||
                   probeType == TraceProbeType.TRACE_PROBE_PRESSURE_5000 ||
                   probeType == TraceProbeType.TRACE_PROBE_MT5030_PRESSURE;
        }

        public bool IsVacuumProbe(TraceProbeType probeType)
        {
            return probeType == TraceProbeType.TRACE_PROBE_VACUUM_100 ||
                   probeType == TraceProbeType.TRACE_PROBE_MT5030_VACUUM;
        }

        public bool IsTemperatureProbe(TraceProbeType probeType)
        {
            return probeType == TraceProbeType.TRACE_PROBE_EEDM506D_TEMPERATURE;
        }

        public void SetPressureUnits(PressureUnits pressureUnits)
        {
            TraceProbeFactory.ConfigureProbe(this, pressureUnits, TemperatureUnits.degC, VacuumUnits.mmHg);
        }

        public void SetVacuumUnits(VacuumUnits vacuumUnits)
        {
            TraceProbeFactory.ConfigureProbe(this, PressureUnits.psi, TemperatureUnits.degC, vacuumUnits);
        }

        public void SetTemperatureUnits(TemperatureUnits temperatureUnits)
        {
            TraceProbeFactory.ConfigureProbe(this, PressureUnits.psi, temperatureUnits, VacuumUnits.mmHg);
        }
    }
}
