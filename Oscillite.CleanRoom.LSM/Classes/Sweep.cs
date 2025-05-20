using System;

using Oscillite.CleanRoom.LSM.Properties;

namespace Oscillite.CleanRoom.LSM
{
    
    public class Sweep
    {
        private double seconds = 20;
        private SweepType type = SweepType.Time;

        
        private string Unit { get; set; } = "sec";

        
        private double Gain { get; set; } = 1.0;

        
        public string Name => $"{(Seconds * Gain)} {Unit}";

        
        public double Seconds
        {
            get
            {
                return seconds;
            }
            set
            {
                seconds = value;

                //We had to add some logic here to handle some edge cases
                UnitsHelper.GetUnitsFromSeconds(value, out string unit, out double gain);
                Unit = unit;
                Gain = gain;
            }
        }

        
        public SweepType Type { get { return type; } set { type = value; } }

    }
}