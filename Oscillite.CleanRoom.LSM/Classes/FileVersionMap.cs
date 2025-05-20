using System.Collections.Generic;

namespace Oscillite.CleanRoom.LSM
{
    // These field names are embedded in the binary files as UTF-16 strings.
    // Their order and grouping were empirically recovered by inspecting real-world files
    // and identifying sequential ASCII/UTF-16 encoded strings.
    // The arrangement reflects the most common structure observed across multiple samples.

    public static class FileVersionMap
    {
        public static Dictionary<FileVersion, int> GetFileVersions() => new Dictionary<FileVersion, int>
        {
            { FileVersion.V1_0, InitializeFileVersion(1, 0) },
            { FileVersion.V1_1, InitializeFileVersion(1, 1) },
            { FileVersion.V1_2, InitializeFileVersion(1, 2) },
            { FileVersion.V1_3, InitializeFileVersion(1, 3) },
            { FileVersion.V1_7, InitializeFileVersion(1, 7) },
            { FileVersion.V1_8, InitializeFileVersion(1, 8) },
        };

        public static int InitializeFileVersion(int major, int minor)
        {
            return major | (minor << 16);
        }

        public static FileVersion? GetFileVersion(int version)
        {
            foreach (var kvp in GetFileVersions())
            {
                if (kvp.Value == version)
                    return kvp.Key;
            }
            return null; // Unknown version
        }

        public static bool IsKnownFileVersion(int version)
        {
            foreach (var kvp in GetFileVersions())
            {
                if (kvp.Value == version)
                    return true;
            }
            return false; // Unknown version
        }
    }
}
