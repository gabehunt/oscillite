using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Oscillite.CleanRoom.LSM
{
    // we discovered these in sample files, but ignition was so rarely used we didn't support it as of yet.
    public enum SweepType
    {
        Time,
        Ignition_Parade_5ms,
        Ignition_Parade_10ms,
        Ignition_Cylinder_5ms,
        Ignition_Cylinder_10ms,
        Ignition_Raster_5ms,
        Ignition_Raster_10ms,
        Ignition_Superimposed_5ms,
        Ignition_Superimposed_10ms
    }
}
