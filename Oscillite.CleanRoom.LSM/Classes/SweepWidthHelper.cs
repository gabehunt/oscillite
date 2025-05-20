using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oscillite.CleanRoom.LSM
{
    public static class SweepWidthHelper
    {
        public static short GetSweepWidth(Sweep sweepRate)
        {
            switch (sweepRate.ToString())
            {
                case "10 µs":
                case "20 µs":
                case "50 µs":
                case "100 µs":
                case "200 µs":
                    return 300;
                default:
                    return 500;
            }
        }
    }
}
