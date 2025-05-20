using System;

using Oscillite.CleanRoom.LSM.Properties;


namespace Oscillite.CleanRoom.LSM
{
	
	public class TraceScale
	{
		public enum Units
		{
			mV,
			V,
			kV,
			psi,
			inHg,
			mmHg,
			kPa,
			MPa,
			bar,
			kgcm2,
			mA,
			A,
			Degree,
			ms,
			s,
			Percent,
			Hz,
			kHz,
			Ohms,
			kOhms,
			MOhms,
			nF,
			uF,
			mF,
			degF,
			degC
		}

		private Units units;

		private float unitsGain = 1f;
		
		public string Name => $"{(int)FullScaleValue * unitsGain} {UnitString}";

        public double FullScaleValue { get; set; }

        public string UnitString
        {
            get
            {
                switch (units)
                {
                    case Units.mV: return "Millivolts"; 
                    case Units.kV: return "Kilovolts"; 
                    case Units.psi: return "Psi"; 
                    case Units.inHg: return "InHg";
                    case Units.mmHg: return "MmHg";
                    case Units.kPa: return "Kilopascal";
                    case Units.MPa: return "Megapascal";
                    case Units.bar: return "Bar";
                    case Units.kgcm2: return "Kg/cm²";
                    case Units.mA: return "Milliamps";
                    case Units.A: return "Amps";
                    case Units.Degree: return "Degree";
                    case Units.ms: return "Milliseconds";
                    case Units.s: return "Seconds";
                    case Units.Percent: return "Percent";
                    case Units.Hz: return "Hertz";
                    case Units.kHz: return "Kilohertz";
                    case Units.Ohms: return "Ohms";
                    case Units.kOhms: return "KiloOhms";
                    case Units.MOhms: return "MegaOhms";
                    case Units.nF: return "Nanofarad";
                    case Units.uF: return "Microfarad";
                    case Units.mF: return "Millifarad";
                    case Units.degF: return "Degree Fahrenheit";
                    case Units.degC: return "Degree Celsius";
                    case Units.V:
                    default:
                        return "Volts";
                }
            }
        }
		
		public Units Unit { get; set; }

        public float ProbeGain { get; set; }

		public TraceScale()
		{
		}

		public TraceScale(double fullScaleValue, Units scaleUnits, float probeGain)
		{
			FullScaleValue = fullScaleValue;
			units = scaleUnits;
			ProbeGain = probeGain;
			switch (units)
			{
                case Units.kHz:
                case Units.kOhms:
                case Units.kV:
                case Units.MPa:
                    unitsGain = 0.001f;
					break;
                case Units.mV:
                case Units.mA:
                case Units.mF:
                case Units.ms:
                    unitsGain = 1000f;
					break;
				case Units.MOhms:
					unitsGain = 1E-06f;
					break;
				case Units.nF:
					unitsGain = 1E+09f;
					break;
				case Units.uF:
					unitsGain = 1000000f;
					break;
			}
		}

	}
}
