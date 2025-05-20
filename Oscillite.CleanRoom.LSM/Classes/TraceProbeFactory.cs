using System;

namespace Oscillite.CleanRoom.LSM
{
    public static class TraceProbeFactory
    {
        public static void ConfigureProbe(
            TraceProbe probe,
            PressureUnits pressureUnits,
            TemperatureUnits temperatureUnits,
            VacuumUnits vacuumUnits)
        {
            probe.VacuumUnits = vacuumUnits;
            probe.PressureUnits = pressureUnits;
            probe.TemperatureUnits = temperatureUnits;

            switch (probe.Type)
            {
                case TraceProbeType.TRACE_PROBE_CAPACITANCE:
                    probe.Name = "Capacitance";
                    probe.TraceScaleList = new CapacitanceTraceScaleList(probe.Gain, probe.Offset);
                    break;

                //Current
                case TraceProbeType.TRACE_PROBE_LOW_AMPS_20:
                    probe.Name = "20A Probe";
                    probe.Gain = 10f;
                    probe.TraceScaleList = new LowAmps20TraceScaleList(probe.Gain, probe.Offset);
                    break;
                case TraceProbeType.TRACE_PROBE_LOW_AMPS_40:
                    probe.Name = "40A Probe";
                    probe.Gain = 100f;
                    probe.TraceScaleList =  new LowAmps40TraceScaleList(probe.Gain, probe.Offset);
                    break;
                case TraceProbeType.TRACE_PROBE_LOW_AMPS_60:
                    probe.Name = "60A Probe";
                    probe.Gain = 100f;
                    probe.TraceScaleList =  new LowAmps60TraceScaleList(probe.Gain, probe.Offset);
                    break;
                case TraceProbeType.TRACE_PROBE_SHUNT:
                    probe.Name = "Shunt Amp Probe";
                    probe.Gain = 10f;
                    probe.TraceScaleList =  new AmpScales(probe.Gain, probe.Offset);
                    break;
                case TraceProbeType.TRACE_PROBE_HIGH_AMPS_200:
                    probe.Name = "200A Probe";
                    probe.Gain = 100f;
                    probe.TraceScaleList =  new HighAmps200TraceScaleList(probe.Gain, probe.Offset);
                    break;
                case TraceProbeType.TRACE_PROBE_HIGH_AMPS_2000:
                    probe.Name = "2000A Probe";
                    probe.Gain = 1000f;
                    probe.TraceScaleList =  new HighAmps2000TraceScaleList(probe.Gain, probe.Offset);
                    break;
                //Diode
                case TraceProbeType.TRACE_PROBE_DIODE:
                    probe.Name = "Diode Probe";
                    probe.TraceScaleList =  new DiodeTraceScaleList(probe.Gain, probe.Offset);
                    break;
                //Duty Cycle
                case TraceProbeType.TRACE_PROBE_DUTY_CYCLE:
                    probe.Name = "Duty Cycle Probe";
                    probe.TraceScaleList =  new TestLeadDutyCycleTraceScaleList(probe.Gain, probe.Offset);
                    break;
                //Frequency
                case TraceProbeType.TRACE_PROBE_FREQUENCY:
                    probe.Name = "Frequency Probe";
                    probe.TraceScaleList =  new TestLeadFrequencyTraceScaleList(probe.Gain, probe.Offset);
                    break;
                //Mixture
                case TraceProbeType.TRACE_PROBE_MIXTURE_CONTROL_DWELL_60:
                    probe.Name = "Mixture Dwell 60 Probe";
                    probe.TraceScaleList =  new TestLeadMcDwell60TraceScaleList(probe.Gain, probe.Offset);
                    break;
                case TraceProbeType.TRACE_PROBE_MIXTURE_CONTROL_DWELL_90:
                    probe.Name = "Mixture Dwell 90 Probe";
                    probe.TraceScaleList =  new TestLeadMcDwell90TraceScaleList(probe.Gain, probe.Offset);
                    break;
                //Pressure
                case TraceProbeType.TRACE_PROBE_PRESSURE_100:
                    probe.Name = "Pressure 100 Probe";
                    switch (pressureUnits)
                    {
                        case PressureUnits.bar:
                            probe.Gain = 1.7236875f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Pressure100barTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case PressureUnits.kgcm2:
                            probe.Gain = 1.7581314f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Pressure100kgcm2TraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case PressureUnits.kPa:
                            probe.Gain = 172.36874f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Pressure100kPaTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case PressureUnits.psi:
                            probe.Gain = 25f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Pressure100psiTraceScaleList(probe.Gain, probe.Offset);
                            break;
                    }
                    break;
                case TraceProbeType.TRACE_PROBE_PRESSURE_500:
                    probe.Name = "Pressure 500 Probe";
                    switch (pressureUnits)
                    {
                        case PressureUnits.bar:
                            probe.Gain = 8.618438f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Pressure500barTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case PressureUnits.kgcm2:
                            probe.Gain = 8.790657f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Pressure500kgcm2TraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case PressureUnits.kPa:
                            probe.Gain = 861.84375f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Pressure500kPaTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case PressureUnits.psi:
                            probe.Gain = 125f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Pressure500psiTraceScaleList(probe.Gain, probe.Offset);
                            break;
                    }
                    break;
                case TraceProbeType.TRACE_PROBE_PRESSURE_5000:
                    probe.Name = "Pressure 5000 Probe";
                    switch (pressureUnits)
                    {
                        case PressureUnits.bar:
                            probe.Gain = 86.18437f;
                            probe.TraceScaleList =  new Pressure5000barTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case PressureUnits.kgcm2:
                            probe.Gain = 87.90657f;
                            probe.TraceScaleList =  new Pressure5000kgcm2TraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case PressureUnits.kPa:
                            probe.Gain = 8618.4375f;
                            probe.TraceScaleList =  new Pressure5000kPaTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case PressureUnits.psi:
                            probe.Gain = 1250f;
                            probe.TraceScaleList =  new Pressure5000psiTraceScaleList(probe.Gain, probe.Offset);
                            break;
                    }
                    break;
                //Pulse width
                case TraceProbeType.TRACE_PROBE_INJECTOR_PULSE_WIDTH:
                    probe.Name = "Injector Pulse Width Probe";
                    probe.TraceScaleList =  new TestLeadInjectorPulseWidthTraceScaleList(probe.Gain, probe.Offset);
                    break;
                case TraceProbeType.TRACE_PROBE_PULSE_WIDTH:
                    probe.Name = "Pulse Width Probe";
                    probe.TraceScaleList =  new TestLeadPulseWidthTraceScaleList(probe.Gain, probe.Offset);
                    break;
                //Resistance
                case TraceProbeType.TRACE_PROBE_OHMS:
                    probe.Name = "Ohms Probe";
                    probe.TraceScaleList =  new OhmsTraceScaleList(probe.Gain, probe.Offset);
                    break;
                //Temperature
                case TraceProbeType.TRACE_PROBE_EEDM506D_TEMPERATURE:
                    probe.Name = "Temperature Probe";
                    switch (temperatureUnits)
                    {
                        case TemperatureUnits.degC:
                            probe.Gain = 1000f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new TemperatureCelsiusTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case TemperatureUnits.degF:
                            probe.Gain = 1000f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new TemperatureFahrenheitTraceScaleList(probe.Gain, probe.Offset);
                            break;
                    }
                    break;
                case TraceProbeType.TRACE_PROBE_MT5030_VACUUM:
                    probe.Name = "Vacuum Probe";
                    switch (vacuumUnits)
                    {
                        case VacuumUnits.inHg:
                            probe.Gain = 1000f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Vacuum100inHgTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case VacuumUnits.kPa:
                            probe.Gain = 3386.3887f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Vacuum100kPaTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case VacuumUnits.mmHg:
                            probe.Gain = 25400f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Vacuum100mmHgTraceScaleList(probe.Gain, probe.Offset);
                            break;
                    }
                    break;
                case TraceProbeType.TRACE_PROBE_MT5030_PRESSURE:
                    probe.Name = "Pressure Probe";
                    switch (pressureUnits)
                    {
                        case PressureUnits.bar:
                            probe.Gain = 68.94757f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Pressure500barTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case PressureUnits.kgcm2:
                            probe.Gain = 70.30696f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Pressure500kgcm2TraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case PressureUnits.kPa:
                            probe.Gain = 6894.7573f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Pressure500kPaTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case PressureUnits.psi:
                            probe.Gain = 1000f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Pressure500psiTraceScaleList(probe.Gain, probe.Offset);
                            break;
                    }
                    break;
                //Vacuum
                case TraceProbeType.TRACE_PROBE_VACUUM_100:
                    probe.Name = "Vacuum 100 Probe";
                    switch (vacuumUnits)
                    {
                        case VacuumUnits.inHg:
                            probe.Gain = 50.900635f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Vacuum100inHgTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case VacuumUnits.kPa:
                            probe.Gain = 172.36874f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Vacuum100kPaTraceScaleList(probe.Gain, probe.Offset);
                            break;
                        case VacuumUnits.mmHg:
                            probe.Gain = 1292.8788f;
                            probe.Offset = 0f;
                            probe.TraceScaleList =  new Vacuum100mmHgTraceScaleList(probe.Gain, probe.Offset);
                            break;
                    }
                    break;
                //Voltage
                case TraceProbeType.TRACE_PROBE_VOLTS:
                    probe.Name = "Volts DC Probe";
                    probe.TraceScaleList =  new TestLeadTraceScaleList(probe.Gain, probe.Offset);
                    break;
                case TraceProbeType.TRACE_PROBE_VOLTS_AC:
                    probe.Name = "Volts AC Probe";
                    probe.TraceScaleList =  new TestLeadVoltsACTraceScaleList(probe.Gain, probe.Offset);
                    break;
                case TraceProbeType.TRACE_PROBE_VOLTS_DC:
                    probe.Name = "Volts DC Probe";
                    probe.TraceScaleList =  new TestLeadVoltsDCTraceScaleList(probe.Gain, probe.Offset);
                    break;
            }
        }
    }
}