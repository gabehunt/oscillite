using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oscillite.CleanRoom.LSM
{
    public static class SweepTimeMap
    {
        /// <summary>
        /// Known good trace probe values used in LSM binary trace headers.
        /// Discovered by analyzing real-world files.
        /// May be missing some values not yet discovered.
        /// </summary>
        public static Dictionary<int, object> GetDefaultSweepTimes()
        {
            return new Dictionary<int, object>
            {
                { 0, 1E-05 },
                { 1, 2E-05 },
                { 2, 5E-05 },
                { 3, 0.0001 },
                { 4, 0.0002 },
                { 5, 0.0005 },
                { 6, 0.001 },
                { 7, 0.002 },
                { 8, 0.005 },
                { 9, 0.01 },
                { 10, 0.02 },
                { 11, 0.05 },
                { 12, 0.1 },
                { 13, 0.2 },
                { 14, 0.5 },
                { 15, 1.0 },
                { 16, 2.0 },
                { 17, 5.0 },
                { 18, 10.0 },
                { 19, 20.0 },
                { 20, 30.0 },
                { 21, 60.0 },
                { 22, 120.0 },
                { 23, 180.0 },
                { 24, 240.0 },
                { 25, 300.0 },
                { 50, SweepType.Ignition_Parade_5ms },
                { 51, SweepType.Ignition_Cylinder_5ms },
                { 52, SweepType.Ignition_Raster_5ms },
                { 53, SweepType.Ignition_Superimposed_5ms },
                { 54, 0.001 },
                { 55, 0.001 },
                { 56, 0.001 },
                { 57, 0.001 },
                { 58, 0.002 },
                { 59, 0.002 },
                { 60, 0.002 },
                { 61, 0.002 },
                { 62, SweepType.Ignition_Parade_5ms },
                { 63, SweepType.Ignition_Cylinder_5ms },
                { 64, SweepType.Ignition_Raster_5ms },
                { 65, SweepType.Ignition_Superimposed_5ms },
                { 66, SweepType.Ignition_Parade_10ms },
                { 67, SweepType.Ignition_Cylinder_10ms },
                { 68, SweepType.Ignition_Raster_10ms },
                { 69, SweepType.Ignition_Superimposed_10ms }
            };
        }
    }
}
