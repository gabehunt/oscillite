using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oscillite.CleanRoom.LSM
{
    public static class ScopeFileExtensionMap
    {
        private static readonly HashSet<string> LabScopeExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".lsc", ".lss", ".lsm", ".lsp"
        };

        private static readonly HashSet<string> IgnitionScopeExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            //not supported
            ".isc", ".iss", ".ism", ".isp"
        };

        private static readonly HashSet<string> GraphingMeterExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mmc", ".mms", ".mmm", ".mmp"
        };

        public static readonly HashSet<string> FrameFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".lss", ".iss", ".mms", ".lsm", ".ism", ".mmm", ".lsp", ".isp", ".mmp"
        };

        public static readonly HashSet<string> SFrameFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".lss", ".iss", ".mms"
        };


        public static bool IsLabScope(string ext) => LabScopeExtensions.Contains(ext);
        public static bool IsGraphingMeter(string ext) => GraphingMeterExtensions.Contains(ext);
    }
}
